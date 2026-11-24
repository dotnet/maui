#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Which phase a node's contribution comes from, for choosing between nodes on one property.</summary>
	internal enum ContributionKind : byte
	{
		None,
		BeforeFill,
		AfterFill,
		Active,
	}

	/// <summary>The value one (target, property) shows at a time, and where it comes from.</summary>
	internal struct Contribution
	{
		public ContributionKind Kind;

		// AfterFill: how long ago the interval ended (smaller wins). BeforeFill: how long until it starts (smaller wins).
		public double Key;
		public object? Value;
	}

	/// <summary>One animated (target, property) of a run, shared by every node that animates it.</summary>
	internal sealed class TrackSlot
	{
		readonly WeakReference<BindableObject> _target;

		public TrackSlot(int index, BindableObject target, BindableProperty property)
		{
			Index = index;
			_target = new WeakReference<BindableObject>(target);
			Property = property;
		}

		public int Index { get; }

		public BindableProperty Property { get; }

		public BindableObject? Target => _target.TryGetTarget(out var target) ? target : null;
	}

	/// <summary>
	/// A storyboard compiled against one play target: targets resolved, cues placed, values sampled and converted,
	/// and tracks grouped by (target, property). Evaluation is a pure function of root time.
	/// </summary>
	internal sealed class CompiledStoryboard
	{
		const double TickSnap = 1e-6;
		const double CountSnap = 1e-9;

		CompiledStoryboard(TimingNode root, List<TrackSlot> slots)
		{
			Root = root;
			Slots = slots;
		}

		public TimingNode Root { get; }

		public IReadOnlyList<TrackSlot> Slots { get; }

		/// <summary>When the run ends, in root time (ticks); infinite for a forever run.</summary>
		public double EndTicks => Root.ParentEnd;

		public bool IsForever => double.IsPositiveInfinity(Root.ParentEnd);

		public static CompiledStoryboard Compile(Storyboard storyboard, VisualElement playTarget, PlaybackOptions options)
		{
			var slots = new List<TrackSlot>();
			var slotLookup = new Dictionary<(BindableObject, BindableProperty), TrackSlot>(new SlotKeyComparer());
			var services = playTarget.FindMauiContext()?.Services ?? Application.Current?.FindMauiContext()?.Services;

			var root = CompileNode(storyboard, null, playTarget, storyboard.TargetName, slots, slotLookup, services, options, isRoot: true);
			return new CompiledStoryboard(root, slots);
		}

		static TimingNode CompileNode(Storyboard storyboard, TimingNode? parent, VisualElement playTarget, string? inheritedTargetName,
			List<TrackSlot> slots, Dictionary<(BindableObject, BindableProperty), TrackSlot> slotLookup, IServiceProvider? services,
			PlaybackOptions options, bool isRoot)
		{
			var targetName = storyboard.TargetName ?? inheritedTargetName;
			var node = new TimingNode(parent)
			{
				Begin = (isRoot && options.BeginTimeOverride is TimeSpan begin ? begin : storyboard.BeginTime).Ticks,
				Speed = storyboard.SpeedRatio * (isRoot ? options.SpeedRatio : 1.0),
				IterationDelay = storyboard.IterationDelay.Ticks,
				Repeat = storyboard.RepeatBehavior,
				Direction = storyboard.Direction,
				Fill = storyboard.FillMode,
			};

			foreach (var child in storyboard.Children)
			{
				if (child is not null)
					node.Children.Add(CompileNode(child, node, playTarget, targetName, slots, slotLookup, services, options, isRoot: false));
			}

			var keyFrames = storyboard.KeyFrames;
			if (keyFrames.Count > 0)
			{
				if (storyboard.Duration <= TimeSpan.Zero)
					throw new InvalidOperationException("A storyboard with keyframes needs a Duration greater than zero (MAUIX2303).");

				node.Duration = storyboard.Duration.Ticks;
				CompileTracks(node, storyboard, playTarget, targetName, slots, slotLookup, services);
			}
			else
			{
				// A container-only storyboard lasts until the end of its last child's active interval.
				var end = 0.0;
				foreach (var child in node.Children)
					end = Math.Max(end, child.ParentEnd);

				if (end <= 0)
					throw new InvalidOperationException("A storyboard needs keyframes with a Duration, or children (MAUIX2303).");

				node.Duration = storyboard.Duration > TimeSpan.Zero ? storyboard.Duration.Ticks : end;
			}

			node.ComputeActive();
			return node;
		}

		static void CompileTracks(TimingNode node, Storyboard storyboard, VisualElement playTarget, string? targetName,
			List<TrackSlot> slots, Dictionary<(BindableObject, BindableProperty), TrackSlot> slotLookup, IServiceProvider? services)
		{
			var keyFrames = storyboard.KeyFrames;
			var cues = PlaceCues(keyFrames, storyboard.Duration);
			var samples = new Dictionary<TrackSlot, List<TrackSample>>();
			var order = new List<TrackSlot>();

			for (var i = 0; i < keyFrames.Count; i++)
			{
				var keyFrame = keyFrames[i];
				foreach (var setter in keyFrame.Setters)
				{
					if (setter?.Property is not BindableProperty property)
						throw new InvalidOperationException("A keyframe setter has no Property (MAUIX2311).");

					var target = ResolveTarget(playTarget, setter.TargetName ?? targetName);
					var key = (target, property);
					if (!slotLookup.TryGetValue(key, out var slot))
					{
						slot = new TrackSlot(slots.Count, target, property);
						slots.Add(slot);
						slotLookup[key] = slot;
					}

					if (!samples.TryGetValue(slot, out var list))
					{
						list = new List<TrackSample>();
						samples[slot] = list;
						order.Add(slot);
					}

					if (list.Count > 0 && list[list.Count - 1].Cue == cues[i])
						throw new InvalidOperationException($"Two keyframes at the same cue set {property.PropertyName} (MAUIX2302).");

					list.Add(new TrackSample(cues[i], ResolveValue(setter.Value, property, target, playTarget), keyFrame.Easing));
				}
			}

			foreach (var slot in order)
			{
				var target = slot.Target!;
				var entry = AnimationInterpolators.Get(slot.Property.ReturnType, services);
				node.Tracks.Add((slot, entry.CreateTrack(target, slot.Property, samples[slot], storyboard.Easing)));
			}
		}

		/// <summary>
		/// Places cues in one pass in declaration order: specified cues must not decrease, and runs of unspecified
		/// cues are spread between their bounds (a leading run starts at 0, a trailing run ends at 1, and a lone
		/// unspecified keyframe sits at 1, so it animates from the current value).
		/// </summary>
		internal static double[] PlaceCues(IList<KeyFrame> keyFrames, TimeSpan duration)
		{
			var count = keyFrames.Count;
			var cues = new double[count];
			var specified = new bool[count];
			var previous = 0.0;

			for (var i = 0; i < count; i++)
			{
				var cue = keyFrames[i].Cue;
				if (!cue.IsUnspecified)
				{
					var fraction = cue.Resolve(duration);
					if (fraction < 0 || fraction > 1 + TickSnap)
						throw new InvalidOperationException($"Cue {cue} is outside the storyboard's duration (MAUIX2301).");

					fraction = Math.Min(1, fraction);
					if (fraction < previous)
						throw new InvalidOperationException($"Cue {cue} comes before the previous keyframe's cue; specified cues must not decrease (MAUIX2306).");

					cues[i] = previous = fraction;
					specified[i] = true;
				}
			}

			var anySpecified = false;
			for (var i = 0; i < count; i++)
				anySpecified |= specified[i];

			if (!anySpecified)
			{
				for (var i = 0; i < count; i++)
					cues[i] = count == 1 ? 1 : i / (double)(count - 1);
				return cues;
			}

			var index = 0;
			while (index < count)
			{
				if (specified[index])
				{
					index++;
				}
				else
				{
					var start = index;
					while (index < count && !specified[index])
						index++;

					PlaceUnspecifiedRun(cues, start, index);
				}
			}

			return cues;
		}

		// A run of unspecified cues [start, end) is spread between its bounds; a leading run is pinned to 0 and a
		// trailing run to 1.
		static void PlaceUnspecifiedRun(double[] cues, int start, int end)
		{
			var count = cues.Length;
			if (start == 0)
			{
				cues[0] = 0;
				start = 1;
			}

			if (end == count)
			{
				cues[count - 1] = 1;
				end = count - 1;
			}

			var lo = start == 0 ? 0 : cues[start - 1];
			var hi = end >= count ? 1 : cues[end];
			var n = end - start;
			for (var k = 0; k < n; k++)
				cues[start + k] = lo + (hi - lo) * (k + 1) / (n + 1);
		}

		static BindableObject ResolveTarget(VisualElement playTarget, string? name)
		{
			if (string.IsNullOrEmpty(name))
				return playTarget;

			var target = Setter.FindTargetByName(playTarget, name!);
			if (target is null)
				throw new InvalidOperationException($"TargetName '{name}' doesn't resolve in the name scope of {playTarget} (MAUIA1004).");

			return target;
		}

		// Values are sampled once, here: a binding against the play target's BindingContext, a dynamic resource
		// against the target's resources. The result is converted to the property's type.
		static object? ResolveValue(object? value, BindableProperty property, BindableObject target, VisualElement playTarget)
		{
			if (value is BindingBase binding)
				value = SampleBinding(binding, property, playTarget);
			else if (value is DynamicResource dynamicResource)
				value = (target as Element ?? playTarget).TryGetResource(dynamicResource.Key, out var resource) ? resource : null;

			if (!property.TryConvert(ref value))
				throw new InvalidOperationException($"Cannot convert {value} to type '{property.ReturnType}' for {property.PropertyName}.");

			return value;
		}

		// Sampled on the play target itself (an Element, with its BindingContext), through a private attached property
		// with the animated property's type and default: relative sources resolve from the play target as for any of its
		// bindings (MultiBinding children included), and an unresolved binding falls back to the animated property's own
		// default. The binding is removed straight after.
		static object? SampleBinding(BindingBase binding, BindableProperty property, VisualElement playTarget)
		{
			var probe = SampleProperties.For(property);
			var clone = binding.Clone();
			Prepare(binding, clone);
			try
			{
				// Inside the try: SetBinding stores the binding before applying it, so a throwing converter must not leave
				// it attached to the play target.
				playTarget.SetBinding(probe, clone);
				return playTarget.GetValue(probe);
			}
			finally
			{
				playTarget.RemoveBinding(probe);
				playTarget.ClearValue(probe);
			}
		}

		// Every binding in the tree reads only (a TwoWay or OneWayToSource clone would write the probe's value into the
		// source; a MultiBinding's clone keeps its children's modes) and keeps its fallback and target-null values (a
		// compiled binding's Clone drops them).
		static void Prepare(BindingBase original, BindingBase clone)
		{
			clone.Mode = BindingMode.OneWay;
			clone.FallbackValue = original.FallbackValue;
			clone.TargetNullValue = original.TargetNullValue;

			if (original is MultiBinding multiOriginal && clone is MultiBinding multiClone)
			{
				for (var i = 0; i < multiClone.Bindings.Count && i < multiOriginal.Bindings.Count; i++)
					Prepare(multiOriginal.Bindings[i], multiClone.Bindings[i]);
			}
		}

		/// <summary>Evaluates every slot at root time <paramref name="ticks"/>.</summary>
		public void Evaluate(double ticks, Contribution[] results)
		{
			for (var i = 0; i < results.Length; i++)
				results[i] = default;

			Root.Evaluate(ticks, results, ContributionKind.None, 0, 1);
		}

		/// <summary>Evaluates every slot at the root's END state (or START state when <paramref name="start"/>).</summary>
		public void EvaluateEdge(bool start, Contribution[] results)
		{
			for (var i = 0; i < results.Length; i++)
				results[i] = default;

			Root.EvaluateEdge(start, results);
		}

		/// <summary>Sets every track's implicit start: the value of an earlier sibling that ends before the track's node
		/// starts (so a sequence continues where the previous step left off), otherwise the value on screen now.</summary>
		public void SnapshotImplicitStarts(Func<TrackSlot, object?> currentValue)
		{
			var scratch = new Contribution[Slots.Count];
			Root.SnapshotImplicitStarts(currentValue, scratch);
		}

		internal static double Snap(double ticks)
		{
			var rounded = Math.Round(ticks);
			return Math.Abs(ticks - rounded) < TickSnap ? rounded : ticks;
		}

		internal static double SnapCount(double count)
		{
			var rounded = Math.Round(count);
			return Math.Abs(count - rounded) < CountSnap ? rounded : count;
		}

		sealed class SlotKeyComparer : IEqualityComparer<(BindableObject, BindableProperty)>
		{
			public bool Equals((BindableObject, BindableProperty) x, (BindableObject, BindableProperty) y)
				=> ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

			public int GetHashCode((BindableObject, BindableProperty) obj)
				=> System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item1) ^ obj.Item2.GetHashCode();
		}

		static class SampleProperties
		{
			static readonly Dictionary<BindableProperty, BindableProperty> s_properties = new();

			// One per animated property, so the default an unresolved binding falls back to is that property's own.
			public static BindableProperty For(BindableProperty property)
			{
				lock (s_properties)
				{
					if (!s_properties.TryGetValue(property, out var sample))
					{
						sample = BindableProperty.CreateAttached("AnimationSample", property.ReturnType, typeof(SampleProperties), property.DefaultValue);
						s_properties[property] = sample;
					}
					return sample;
				}
			}
		}
	}

	/// <summary>
	/// One storyboard in the timing tree. Times are in ticks: <see cref="Begin"/> in the parent's time, the rest in
	/// this node's local time.
	/// </summary>
	internal sealed class TimingNode
	{
		public TimingNode(TimingNode? parent)
		{
			Parent = parent;
		}

		public TimingNode? Parent { get; }

		public double Begin;
		public double Speed = 1;
		public double Duration;
		public double IterationDelay;
		public RepeatBehavior Repeat;
		public AnimationDirection Direction;
		public AnimationFillMode Fill;

		public readonly List<(TrackSlot Slot, CompiledTrack Track)> Tracks = new();
		public readonly List<TimingNode> Children = new();

		/// <summary>The active duration, in local time; infinite when forever.</summary>
		public double Active { get; private set; }

		/// <summary>When the active interval ends, in the parent's time.</summary>
		public double ParentEnd => double.IsPositiveInfinity(Active) ? double.PositiveInfinity : CompiledStoryboard.Snap(Begin + Active / Speed);

		double Span => Duration + IterationDelay;

		public void ComputeActive()
		{
			// An explicit repeat duration bounds the run even when its iteration is open-ended (a container of a
			// forever child).
			if (Repeat.HasDuration)
			{
				Active = Repeat.Duration.Ticks * Speed;
			}
			else if (Repeat.IsForever || double.IsPositiveInfinity(Duration))
			{
				Active = double.PositiveInfinity;
			}
			else
			{
				var n = Repeat.Count;
				Active = n <= 0 ? 0 : CompiledStoryboard.Snap(n * Duration + (Math.Ceiling(CompiledStoryboard.SnapCount(n)) - 1) * IterationDelay);
			}
		}

		bool Forward(long iteration) => Direction switch
		{
			AnimationDirection.Reverse => false,
			AnimationDirection.Alternate => iteration % 2 == 0,
			AnimationDirection.AlternateReverse => iteration % 2 != 0,
			_ => true,
		};

		double Directed(long iteration, double x) => Forward(iteration) ? x : Duration - x;

		/// <summary>The local time (0..Duration) and iteration of the END state.</summary>
		public (double Tau, long Iteration) EndState()
		{
			if (double.IsPositiveInfinity(Active))
				return (Directed(0, 0), 0);

			long iteration;
			double x;
			if (Repeat.HasDuration)
			{
				var span = Span;
				if (double.IsPositiveInfinity(span))
				{
					// An open-ended iteration bounded by the repeat duration: still the first iteration.
					iteration = 0;
					x = Active;
				}
				else
				{
					var position = CompiledStoryboard.SnapCount(Active / span);
					iteration = (long)Math.Floor(position);
					// An end exactly on a span boundary belongs to the previous iteration.
					if (iteration > 0 && position == Math.Floor(position))
						iteration--;
					x = Math.Min(CompiledStoryboard.Snap(Active - iteration * span), Duration);
				}
			}
			else
			{
				var n = CompiledStoryboard.SnapCount(Repeat.Count);
				if (n <= 0)
					return (Directed(0, 0), 0);

				iteration = (long)Math.Ceiling(n) - 1;
				x = CompiledStoryboard.Snap((n - iteration) * Duration);
			}

			return (Directed(iteration, Math.Max(0, Math.Min(x, Duration))), iteration);
		}

		/// <summary>Local time, iteration and phase at parent time <paramref name="parentTicks"/>.</summary>
		public (double Tau, long Iteration, ContributionKind Phase, double Key) Locate(double parentTicks)
		{
			var a = (parentTicks - Begin) * Speed;
			if (a < 0)
				return (Directed(0, 0), 0, ContributionKind.BeforeFill, -a / Speed);

			if (a < Active)
			{
				var span = Span;
				var iteration = span > 0 && !double.IsPositiveInfinity(span) ? (long)Math.Floor(CompiledStoryboard.SnapCount(a / span)) : 0;
				var u = iteration == 0 ? a : CompiledStoryboard.Snap(a - iteration * span);
				var x = Math.Min(u, Duration);
				return (Directed(iteration, x), iteration, ContributionKind.Active, a / Speed);
			}

			var (tau, endIteration) = EndState();
			return (tau, endIteration, ContributionKind.AfterFill, (a - Active) / Speed);
		}

		// Larger than any real fill key (ticks), so it only orders "not started" below "started or completed".
		const double BeforeFillRank = 1e18;

		/// <summary>
		/// Adds this node's contributions at parent time <paramref name="parentTicks"/>. Keys are in root time (ticks), so
		/// contributions from different nesting levels and speeds compare directly: <paramref name="rootPerParentTick"/>
		/// converts this node's parent time to root time.
		/// </summary>
		public void Evaluate(double parentTicks, Contribution[] results, ContributionKind inheritedKind, double inheritedKey, double rootPerParentTick)
		{
			var (tau, _, phase, key) = Locate(parentTicks);

			switch (phase)
			{
				case ContributionKind.BeforeFill when Fill != AnimationFillMode.Backward && Fill != AnimationFillMode.Both:
					return;
				case ContributionKind.AfterFill when Fill != AnimationFillMode.Forward && Fill != AnimationFillMode.Both:
					return;
			}

			key *= rootPerParentTick;

			// Inside a parent that is filling, everything below it is a fill too. The parent's key orders it against other
			// nodes; the child's own key (0 when it was active at the parent's frozen time) orders siblings inside it, so of
			// two completed steps the later one wins. A child that had not started yet ranks below every child that had.
			var kind = phase;
			if (inheritedKind != ContributionKind.None && inheritedKind != ContributionKind.Active)
			{
				kind = inheritedKind;
				key = inheritedKey + phase switch
				{
					ContributionKind.Active => 0,
					ContributionKind.BeforeFill => BeforeFillRank + key,
					_ => key,
				};
			}

			Contribute(tau, kind, key, results, rootPerParentTick / Speed);
		}

		public void EvaluateEdge(bool start, Contribution[] results, double rootPerParentTick = 1)
		{
			if (start)
			{
				Contribute(Directed(0, 0), ContributionKind.BeforeFill, 0, results, rootPerParentTick / Speed);
			}
			else
			{
				var (tau, _) = EndState();
				Contribute(tau, ContributionKind.AfterFill, 0, results, rootPerParentTick / Speed);
			}
		}

		bool _implicitStartsSet;

		void Contribute(double tau, ContributionKind kind, double key, Contribution[] results, double rootPerLocalTick)
		{
			if (Tracks.Count > 0)
			{
				var progress = Duration > 0 ? tau / Duration : 1;
				foreach (var (slot, track) in Tracks)
					Offer(ref results[slot.Index], kind, key, track.Evaluate(progress));
			}

			foreach (var child in Children)
				child.Evaluate(tau, results, kind, key, rootPerLocalTick);
		}

		// One rule for every (target, property): an ACTIVE contribution beats any fill, and of two active ones the most
		// recently started wins (a later declaration breaks a tie), so parts of a storyboard that overlap have a defined
		// result. Among AFTER fills the most recently ended wins; a BEFORE fill applies only while nothing on the property
		// has started, and the one that starts first wins.
		static void Offer(ref Contribution current, ContributionKind kind, double key, object? value)
		{
			var replace = current.Kind switch
			{
				ContributionKind.None => true,
				ContributionKind.Active => kind == ContributionKind.Active && key <= current.Key,
				ContributionKind.AfterFill => kind == ContributionKind.Active || (kind == ContributionKind.AfterFill && key < current.Key),
				_ => kind == ContributionKind.Active || kind == ContributionKind.AfterFill || (kind == ContributionKind.BeforeFill && key < current.Key),
			};

			if (!replace)
				return;

			current.Kind = kind;
			current.Key = key;
			current.Value = value;
		}

		public void SnapshotImplicitStarts(Func<TrackSlot, object?> currentValue, Contribution[] scratch)
		{
			// A predecessor's end value depends on its own implicit starts, so they are set first (on demand), whatever
			// the declaration order. The flag also guards against revisiting a node.
			if (_implicitStartsSet)
				return;

			_implicitStartsSet = true;
			foreach (var (slot, track) in Tracks)
			{
				if (track.HasImplicitStart)
					track.SetImplicitStart(PredecessorEndValue(slot, currentValue, scratch, out var value) ? value : currentValue(slot));
			}

			foreach (var child in Children)
				child.SnapshotImplicitStarts(currentValue, scratch);
		}

		// The END value of the latest-ending earlier sibling that animates the same slot.
		bool PredecessorEndValue(TrackSlot slot, Func<TrackSlot, object?> currentValue, Contribution[] scratch, out object? value)
		{
			value = null;

			// The latest-ending earlier sibling at this level; failing that, at each enclosing level in turn, so a step
			// wrapped in a container continues from the step before the container.
			TimingNode? best = null;
			for (var node = this; best is null && node.Parent is TimingNode parent; node = parent)
			{
				foreach (var sibling in parent.Children)
				{
					var endsBefore = !ReferenceEquals(sibling, node) && sibling.ParentEnd <= node.Begin;
					if (endsBefore && sibling.Animates(slot) && (best is null || sibling.ParentEnd > best.ParentEnd))
						best = sibling;
				}
			}

			if (best is null)
				return false;

			best.SnapshotImplicitStarts(currentValue, scratch);

			for (var i = 0; i < scratch.Length; i++)
				scratch[i] = default;

			best.EvaluateEdge(start: false, scratch);
			value = scratch[slot.Index].Value;
			return scratch[slot.Index].Kind != ContributionKind.None;
		}

		bool Animates(TrackSlot slot)
		{
			foreach (var (candidate, _) in Tracks)
			{
				if (ReferenceEquals(candidate, slot))
					return true;
			}

			foreach (var child in Children)
			{
				if (child.Animates(slot))
					return true;
			}

			return false;
		}
	}
}
