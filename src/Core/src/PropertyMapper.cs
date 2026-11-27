using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if HANDLER_INSTRUMENTATION
using Microsoft.Maui.Diagnostics;
#endif

#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	public abstract class PropertyMapper : IPropertyMapper
	{
		private protected readonly Dictionary<string, Action<IElementHandler, IElement>> _mapper = new(StringComparer.Ordinal);
		readonly Dictionary<string, MappingCustomization> _mappingCustomizations = new(StringComparer.Ordinal);
		Dictionary<string, FrameworkCell>? _frameworkCells;
		Dictionary<string, InheritedCapture>? _inheritedCaptures;
		bool _frameworkMappingsSealed;
		bool _preserveMappingCustomizations;
		IPropertyMapper[]? _chained;

		// Every change stamps the changed mapper with the next value of this counter, which is shared by all
		// property mappers.
		static long s_lastVersion;

		// Nonzero while the mapper resolves mappings to invoke or compose them rather than to hand them to a caller;
		// see GetPropertyDirect.
		[ThreadStatic]
		static int t_directResolution;

		// The merged view of this mapper (and its chain) is published as a single reference so that
		// everything derived from one merge - the keys, the mappings, and the key->mapping cache - is
		// always read as a matched set. Mutating the mapper (SetPropertyCore/Chained) drops the reference;
		// the next reader merges again. Readers must therefore snapshot it into a local exactly once.
		// Volatile so a reader on another thread can never see the reference before the merge it points at
		// is fully built.
		volatile MergedMappers? _merged;

		long _version;

		// A merged view also contains mappings from chained mappers, so it is rebuilt when they change.
		MergedMappers Merged
		{
			get
			{
				var merged = _merged;
				return merged is not null && merged.Version == ChainVersion
					? merged
					: SnapshotMappers();
			}
		}

		/// <summary>
		/// Identifies a state of this mapper and its chain. Each change stamps the changed mapper with a value larger
		/// than every earlier stamp of any property mapper, so the largest stamp in the chain changes whenever
		/// anything in it changes, including when a chained mapper is replaced by one that changed less often.
		/// </summary>
		internal long ChainVersion => Math.Max(Volatile.Read(ref _version), GetChainVersion(Chained));

		static long GetChainVersion(IPropertyMapper[]? chained)
		{
			long version = 0;
			if (chained is not null)
			{
				foreach (var mapper in chained)
				{
					if (mapper is PropertyMapper propertyMapper)
						version = Math.Max(version, propertyMapper.ChainVersion);
				}
			}

			return version;
		}

		public PropertyMapper()
		{
		}

		public PropertyMapper(params IPropertyMapper[]? chained)
		{
			Chained = chained;
		}

		protected virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			// Replacing an existing mapping, or assigning any key after the built-in mappings are sealed, is
			// an app customization. Record it so a later (lazy) framework remap composes underneath it.
			if (!_preserveMappingCustomizations
				&& (_mapper.ContainsKey(key) || _frameworkMappingsSealed))
			{
				// The action already went through this method, so an override's transformation applies once.
				AddMappingCustomization(key, _ => action, transformed: true);
				return;
			}

			_mapper[key] = action;

			ClearMergedMappers();
		}

		void StoreMapping(string key, Action<IElementHandler, IElement> action)
		{
			_mapper[key] = action;
			ClearMergedMappers();
		}

		void SetCustomizedPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			_preserveMappingCustomizations = true;
			try
			{
				SetPropertyCore(key, action);
			}
			finally
			{
				_preserveMappingCustomizations = false;
			}
		}

		internal void AddMappingCustomization(
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> customization,
			bool transformed = false)
		{
			var sequence = MappingCustomizationSequence.Next();
			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				// An app customization of a chained key builds on the chained mappings as they are now,
				// matching the historical capture of the previous mapping, while framework remaps of
				// those chained mappings remain visible.
				mappingCustomization = new MappingCustomization(this, key, sequence);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.AddCustomization(sequence, customization);
			mappingCustomization.Register(transformed);
		}

		/// <summary>
		/// Marks the mappings added so far as the built-in framework mappings. Each one is placed in a cell whose
		/// delegate stays the same when a framework remap later changes it, so delegates the app obtained earlier,
		/// for example to wrap them in a replacement, include the remap as if it had run at startup.
		/// </summary>
		internal void SealFrameworkMappings()
		{
			_frameworkMappingsSealed = true;
			_frameworkCells ??= new(StringComparer.Ordinal);
			foreach (var key in _mapper.Keys.ToList())
			{
				if (_frameworkCells.ContainsKey(key) || _mappingCustomizations.ContainsKey(key))
					continue;

				var cell = new FrameworkCell(_mapper[key]);
				_frameworkCells[key] = cell;
				_mapper[key] = cell.Mapping;
			}

			ClearMergedMappers();
		}

		internal void ModifyFrameworkMapping(
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification)
		{
			if (_frameworkCells is not null && _frameworkCells.TryGetValue(key, out var cell))
			{
				// Everything built on the cell, including recorded app customizations, observes the change.
				cell.Modify(modification);
				ClearMergedMappers();
				return;
			}

			if (!_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				// A remap of a key without a framework cell builds on the chained mappings as they are now, like the
				// remaps that captured the previous mapping when they ran at startup.
				mappingCustomization = new MappingCustomization(this, key, MappingCustomizationSequence.Current);
				_mappingCustomizations[key] = mappingCustomization;
			}

			mappingCustomization.ModifyFrameworkMapping(modification);
			mappingCustomization.Register();
		}

		/// <summary>
		/// Gets the mapping for <paramref name="key"/> as it was after the app customizations recorded up to
		/// <paramref name="sequence"/>, including all framework mappings and remaps.
		/// </summary>
		internal Action<IElementHandler, IElement>? GetPropertyAsOf(string key, long sequence)
		{
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization))
			{
				return mappingCustomization.TryGetMappingAsOf(sequence, out var mapping)
					? mapping
					: GetChainedPropertyAsOf(mappingCustomization.Chained, key, sequence);
			}

			// Honor GetProperty overrides; only an inherited chained mapping has an earlier state.
			var current = GetPropertyDirect(key);
			if (current is not null && !_mapper.ContainsKey(key) && ReferenceEquals(current, GetChainedPropertyDirect(key)))
				return GetChainedPropertyAsOf(Chained, key, sequence);

			return current;
		}

		Action<IElementHandler, IElement>? GetInheritedPropertyAsOf(string key, long sequence, IPropertyMapper[]? chained)
		{
			if (_mappingCustomizations.TryGetValue(key, out var mappingCustomization)
				&& mappingCustomization.TryGetMappingAsOf(sequence, out var mapping))
			{
				return mapping;
			}

			return GetChainedPropertyAsOf(chained, key, sequence);
		}

		static Action<IElementHandler, IElement>? GetChainedPropertyAsOf(IPropertyMapper[]? chained, string key, long sequence)
		{
			if (chained is not null)
			{
				foreach (var ch in chained)
				{
					var returnValue = ch is PropertyMapper propertyMapper
						? propertyMapper.GetPropertyAsOf(key, sequence)
						: ch.GetProperty(key);
					if (returnValue != null)
					{
						return returnValue;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the mapping <see cref="GetProperty"/> reports, honoring overrides, but resolves inherited mappings of
		/// sealed mappers to the chained mapping itself instead of a mapping bound to this mapper. Use it to invoke or
		/// compose the current mapping, or to compare it by reference.
		/// </summary>
		internal static Action<IElementHandler, IElement>? GetPropertyDirect(IPropertyMapper mapper, string key) =>
			mapper is PropertyMapper propertyMapper ? propertyMapper.GetPropertyDirect(key) : mapper.GetProperty(key);

		Action<IElementHandler, IElement>? GetPropertyDirect(string key)
		{
			t_directResolution++;
			try
			{
				return GetProperty(key);
			}
			finally
			{
				t_directResolution--;
			}
		}

		Action<IElementHandler, IElement>? GetChainedPropertyDirect(string key)
		{
			t_directResolution++;
			try
			{
				return GetChainedProperty(key);
			}
			finally
			{
				t_directResolution--;
			}
		}

		/// <summary>
		/// Gets a mapping for an inherited key that is bound to this mapper: it resolves on invocation to the mapping
		/// as it was after the app customizations recorded so far, including framework remaps of the key on this
		/// mapper that run later.
		/// </summary>
		Action<IElementHandler, IElement> CaptureInheritedProperty(string key)
		{
			var sequence = MappingCustomizationSequence.Current;
			var chained = Chained;
			lock (_mappingCustomizations)
			{
				_inheritedCaptures ??= new(StringComparer.Ordinal);
				if (_inheritedCaptures.TryGetValue(key, out var capture)
					&& capture.Sequence == sequence
					&& ReferenceEquals(capture.Chained, chained))
				{
					return capture.Mapping;
				}

				capture = new InheritedCapture(this, key, sequence, chained);
				_inheritedCaptures[key] = capture;
				return capture.Mapping;
			}
		}

		protected virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			if (!viewHandler.CanInvokeMappers())
			{
				return;
			}

			TryUpdatePropertyCore(key, viewHandler, virtualView);
		}

		internal bool TryUpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var cachedMappers = Merged.CachedMappers;
			if (cachedMappers.TryGetValue(key, out var action))
			{
				if (action is not null)
				{
#if HANDLER_INSTRUMENTATION
					if (!HandlerInstrumentation.HasListeners)
					{
						action(viewHandler, virtualView);
						return true;
					}

					using (HandlerInstrumentation.StartWhenListening("MapProperty", viewHandler, virtualView, key))
					{
						action(viewHandler, virtualView);
					}
#else
					action(viewHandler, virtualView);
#endif

					return true;
				}

				return false;
			}

			// CachedMappers initially contains only the UpdateProperties keys which may not contain the key we are looking for.
			// This should never happen, but there's a chance someone may have customized `GetKeys` to return a subset of the actual registered mapper keys.
			var mapper = GetPropertyDirect(key);
			cachedMappers[key] = mapper;

			if (mapper is not null)
			{
#if HANDLER_INSTRUMENTATION
				if (!HandlerInstrumentation.HasListeners)
				{
					mapper(viewHandler, virtualView);
					return true;
				}

				using (HandlerInstrumentation.StartWhenListening("MapProperty", viewHandler, virtualView, key))
				{
					mapper(viewHandler, virtualView);
				}
#else
				mapper(viewHandler, virtualView);
#endif

				return true;
			}

			return false;
		}

		/// <summary>
		/// Maps <paramref name="key"/> through the current chained mappers, as a typed mapping does for a virtual view
		/// of another type.
		/// </summary>
		internal void UpdateChainedProperty(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is null)
				return;

			foreach (var chain in chainedPropertyMappers)
			{
				// Try to leverage our internal method which uses merged mappers
				if (chain is PropertyMapper propertyMapper)
				{
					if (propertyMapper.TryUpdatePropertyCore(key, viewHandler, virtualView))
					{
						break;
					}
				}
				else if (chain.GetProperty(key) != null)
				{
					chain.UpdateProperty(viewHandler, virtualView, key);
					break;
				}
			}
		}

		public virtual Action<IElementHandler, IElement>? GetProperty(string key)
		{
			// A customized key holds the registration that includes the app customizations recorded so far. Later
			// customizations register new mappings, so a caller can wrap the returned one without recursion.
			if (_mapper.TryGetValue(key, out var action))
			{
				return action;
			}

			if (!_frameworkMappingsSealed || t_directResolution > 0)
			{
				return GetChainedProperty(key);
			}

			// A caller that wraps an inherited mapping of a built-in mapper must still observe a later framework
			// remap of the key on this mapper, as it would have when the remaps ran at startup.
			return GetChainedPropertyDirect(key) is null ? null : CaptureInheritedProperty(key);
		}

		Action<IElementHandler, IElement>? GetChainedProperty(string key)
		{
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is not null)
			{
				foreach (var ch in chainedPropertyMappers)
				{
					var returnValue = ch.GetProperty(key);
					if (returnValue != null)
					{
						return returnValue;
					}
				}
			}

			return null;
		}

		public void UpdateProperty(IElementHandler viewHandler, IElement? virtualView, string property)
		{
			if (virtualView == null || !viewHandler.CanInvokeMappers())
			{
				return;
			}

			TryUpdatePropertyCore(property, viewHandler, virtualView);
		}

		public void UpdateProperties(IElementHandler viewHandler, IElement? virtualView)
		{
			if (virtualView == null || !viewHandler.CanInvokeMappers())
			{
				return;
			}

			// One read of the merged reference: `keys[i]` is then guaranteed to be the key whose mapping is
			// `mappers[i]`, even if another thread replaces the merge (with a same-sized one or not) while
			// this pass is running.
			var merged = Merged;
			var keys = merged.Keys;
			var mappers = merged.Mappers;

			// Scope this bulk enumeration as a single "pass" for viewHandler (see PropertyMapperPassScope)
			// so mapper implementations can tell a genuine step of *this* bulk pass apart from an unrelated
			// single-key UpdateProperty call that merely happens to touch the same handler at a different
			// time. Ended in a finally so the pass is never left open - even if a mapper throws partway
			// through the enumeration - and so a nested pass always restores the pass that enclosed it.
			var pass = PropertyMapperPassScope.BeginPass(viewHandler, keys);
			try
			{
#if HANDLER_INSTRUMENTATION
				if (!HandlerInstrumentation.HasListeners)
				{
					for (int i = 0; i < mappers.Count; i++)
					{
						pass.CurrentKeyIndex = i;
						mappers[i](viewHandler, virtualView);
					}

					return;
				}

				for (int i = 0; i < mappers.Count; i++)
				{
					pass.CurrentKeyIndex = i;
					using (HandlerInstrumentation.StartWhenListening("MapProperty", viewHandler, virtualView, keys[i]))
					{
						mappers[i](viewHandler, virtualView);
					}
				}
#else
				for (int i = 0; i < mappers.Count; i++)
				{
					pass.CurrentKeyIndex = i;
					mappers[i](viewHandler, virtualView);
				}
#endif
			}
			finally
			{
				PropertyMapperPassScope.EndPass(pass);
			}
		}

		public IPropertyMapper[]? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				ClearMergedMappers();
			}
		}

		void ClearMergedMappers()
		{
			_merged = null;
			Volatile.Write(ref _version, Interlocked.Increment(ref s_lastVersion));
		}

		public virtual IEnumerable<string> GetKeys()
		{
			// We want to retain the initial order of the keys to avoid race conditions
			// when a property mapping is overridden by a new instance of property mapper.
			// As an example, the container view mapper should always run first.
			// Siblings mapper should not have keys intersection.
			var chainedPropertyMappers = Chained;
			if (chainedPropertyMappers is not null)
			{
				for (int i = chainedPropertyMappers.Length - 1; i >= 0; i--)
				{
					foreach (var key in chainedPropertyMappers[i].GetKeys())
					{
						yield return key;
					}
				}
			}

			// Enqueue keys from this mapper.
			foreach (var mapper in _mapper)
			{
				yield return mapper.Key;
			}
		}

		MergedMappers SnapshotMappers()
		{
			var version = ChainVersion;
			var keys = GetKeys().Distinct().ToList();
			var mappers = new List<Action<IElementHandler, IElement>>(keys.Count);
			var cachedMappers = new Dictionary<string, Action<IElementHandler, IElement>?>(keys.Count);

			foreach (var key in keys)
			{
				var mapper = GetPropertyDirect(key)!;
				mappers.Add(mapper);
				cachedMappers[key] = mapper;
			}

			var merged = new MergedMappers(keys, mappers, cachedMappers, version);
			_merged = merged;

			return merged;
		}

		/// <summary>
		/// A built-in framework mapping of a sealed mapper, changed in place by lazy framework remaps.
		/// </summary>
		sealed class FrameworkCell
		{
			volatile Action<IElementHandler, IElement> _current;

			public FrameworkCell(Action<IElementHandler, IElement> mapping)
			{
				_current = mapping;
				Mapping = (handler, view) => _current(handler, view);
			}

			public Action<IElementHandler, IElement> Mapping { get; }

			public void Modify(Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification) =>
				_current = modification(_current);
		}

		/// <summary>
		/// The framework mapping of one key and the app customizations composed on top of it.
		/// </summary>
		/// <remarks>
		/// Framework remaps may run lazily, after app customizations were registered. They therefore modify the
		/// framework layer underneath the recorded customizations, as if they had run first. When the key is not
		/// local, the framework layer is the mapping of the chained mappers the owner had when the record was created.
		/// App customizations of those mappers are only included when they were recorded before the record was
		/// created, which preserves the previously captured mapping semantics. Framework remaps of the chained
		/// mappers, including newly added keys, are always observed because the composition is rebuilt when they
		/// change.
		/// </remarks>
		sealed class MappingCustomization
		{
			readonly PropertyMapper _owner;
			readonly string _key;
			readonly Action<IElementHandler, IElement>? _localMapping;
			readonly bool _hasLocalMapping;
			readonly long _chainedMappingSequence;
			readonly List<Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>>> _frameworkModifications = new();
			readonly List<long> _sequences = new();
			readonly List<Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>>> _customizations = new();
			readonly List<Action<IElementHandler, IElement>?> _registeredMappings = new();
			volatile Compositions? _compositions;
			int _revision;

			public MappingCustomization(PropertyMapper owner, string key, long chainedMappingSequence)
			{
				_owner = owner;
				_key = key;
				// Start from the mapping GetProperty reports, which honors overrides. Only an inherited mapping is
				// resolved through the chained mappers, at the sequence of the record.
				var mapping = owner.GetPropertyDirect(key);
				_hasLocalMapping = owner._mapper.ContainsKey(key)
					|| (mapping is not null && !ReferenceEquals(mapping, owner.GetChainedPropertyDirect(key)));
				_localMapping = _hasLocalMapping ? mapping : null;
				Chained = owner.Chained;
				_chainedMappingSequence = chainedMappingSequence;
			}

			/// <summary>
			/// The chained mappers an inherited mapping is resolved from. Replacing the owner's chain later does not
			/// change the mapping the recorded customizations build on.
			/// </summary>
			public IPropertyMapper[]? Chained { get; }

			public void AddCustomization(
				long sequence,
				Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> customization)
			{
				_sequences.Add(sequence);
				_customizations.Add(customization);
				Interlocked.Increment(ref _revision);
				_compositions = null;
			}

			public void ModifyFrameworkMapping(
				Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification)
			{
				_frameworkModifications.Add(modification);
				Interlocked.Increment(ref _revision);
				_compositions = null;
			}

			/// <summary>
			/// Registers the mapping that includes every customization recorded so far through the owner's
			/// <see cref="SetPropertyCore"/>, so transformations made by an override apply, and remembers what was
			/// registered. A direct replacement that already went through <see cref="SetPropertyCore"/> is
			/// <paramref name="transformed"/> and is stored as is, so the override's transformation applies once. The registered mapping is bound to its position in the composition: it observes later
			/// framework remaps but not later app customizations, which register their own mappings.
			/// </summary>
			public void Register(bool transformed = false)
			{
				var index = _customizations.Count;
				Action<IElementHandler, IElement> mapping = (handler, view) => GetMappingAt(index)?.Invoke(handler, view);
				if (transformed)
					_owner.StoreMapping(_key, mapping);
				else
					_owner.SetCustomizedPropertyCore(_key, mapping);

				while (_registeredMappings.Count <= index)
					_registeredMappings.Add(null);

				_registeredMappings[index] = _owner._mapper.TryGetValue(_key, out var registered) ? registered : null;
			}

			/// <summary>
			/// Gets the mapping as it was after the app customizations recorded up to <paramref name="sequence"/>.
			/// Returns <see langword="false"/> when the key was still inherited unchanged at that point.
			/// </summary>
			public bool TryGetMappingAsOf(long sequence, out Action<IElementHandler, IElement>? mapping)
			{
				var count = 0;
				while (count < _sequences.Count && _sequences[count] <= sequence)
					count++;

				if (count == 0 && _frameworkModifications.Count == 0 && !_hasLocalMapping)
				{
					mapping = null;
					return false;
				}

				mapping = (count < _registeredMappings.Count ? _registeredMappings[count] : null)
					?? GetMappingAt(count);
				return true;
			}

			Action<IElementHandler, IElement>? GetMappingAt(int index)
			{
				var mappings = GetCompositions().Mappings;
				return index < mappings.Length ? mappings[index] : null;
			}

			Compositions GetCompositions()
			{
				var compositions = _compositions;
				var revision = Volatile.Read(ref _revision);
				var version = GetChainVersion(Chained);
				if (compositions is not null && compositions.Version == version && compositions.Revision == revision)
					return compositions;

				// Read the counts once: another thread may record a customization while this one composes, and
				// the revision then makes the next caller compose again.
				var modificationCount = _frameworkModifications.Count;
				var customizationCount = _customizations.Count;
				var mappings = new Action<IElementHandler, IElement>?[customizationCount + 1];
				var mapping = _hasLocalMapping ? _localMapping : GetChainedPropertyAsOf(Chained, _key, _chainedMappingSequence);
				for (var i = 0; i < modificationCount; i++)
				{
					mapping = _frameworkModifications[i](mapping);
				}

				mappings[0] = mapping;
				for (var i = 0; i < customizationCount; i++)
				{
					mapping = _customizations[i](mapping);
					mappings[i + 1] = mapping;
				}

				compositions = new Compositions(mappings, version, revision);
				_compositions = compositions;
				return compositions;
			}

			sealed class Compositions
			{
				public Compositions(Action<IElementHandler, IElement>?[] mappings, long version, int revision)
				{
					Mappings = mappings;
					Version = version;
					Revision = revision;
				}

				/// <summary>
				/// The framework mapping followed by the mapping after each recorded customization.
				/// </summary>
				public Action<IElementHandler, IElement>?[] Mappings { get; }

				public long Version { get; }

				public int Revision { get; }
			}
		}

		/// <summary>
		/// An inherited mapping of a sealed mapper handed to a caller, bound to the state it was read in.
		/// </summary>
		sealed class InheritedCapture
		{
			readonly PropertyMapper _owner;
			readonly string _key;

			public InheritedCapture(PropertyMapper owner, string key, long sequence, IPropertyMapper[]? chained)
			{
				_owner = owner;
				_key = key;
				Sequence = sequence;
				Chained = chained;
				Mapping = Invoke;
			}

			public long Sequence { get; }

			public IPropertyMapper[]? Chained { get; }

			public Action<IElementHandler, IElement> Mapping { get; }

			void Invoke(IElementHandler handler, IElement view) =>
				_owner.GetInheritedPropertyAsOf(_key, Sequence, Chained)?.Invoke(handler, view);
		}

		/// <summary>
		/// The result of merging a mapper with its chain: the keys to enumerate, their mappings in the very
		/// same order, and the key-to-mapping lookup used by single-key updates. Published as one reference
		/// so readers can never observe a mix of two different merges.
		/// </summary>
		sealed class MergedMappers
		{
			public readonly List<string> Keys;

			public readonly List<Action<IElementHandler, IElement>> Mappers;

			/// <summary>
			/// Lazily grown by <see cref="TryUpdatePropertyCore"/> for keys outside <see cref="Keys"/>.
			/// </summary>
			public readonly Dictionary<string, Action<IElementHandler, IElement>?> CachedMappers;

			/// <summary>
			/// The <see cref="ChainVersion"/> this view was merged at.
			/// </summary>
			public readonly long Version;

			public MergedMappers(
				List<string> keys,
				List<Action<IElementHandler, IElement>> mappers,
				Dictionary<string, Action<IElementHandler, IElement>?> cachedMappers,
				long version)
			{
				Keys = keys;
				Mappers = mappers;
				CachedMappers = cachedMappers;
				Version = version;
			}
		}
	}

	/// <summary>
	/// Tracks the bulk <see cref="PropertyMapper.UpdateProperties"/> passes currently in progress on the
	/// calling thread: which handler each one is mapping, and which key of that pass is being mapped right
	/// now. Mapper implementations that redirect one property key's update into another (see
	/// VisualElement's BackgroundColor/BackgroundImageSource/SemanticProperties redirects) use this to tell
	/// their own natural, provably-redundant turn within a bulk pass apart from every other invocation -
	/// an explicit single-key <see cref="PropertyMapper.UpdateProperty"/> call (e.g.
	/// <c>Handler.UpdateValue(name)</c>), a re-entrant call raised by a mapper running later in the same
	/// pass, or a call made when no bulk pass is running at all.
	/// </summary>
	/// <remarks>
	/// Passes are tracked in a per-thread stack of reusable frames rather than in a table keyed by handler.
	/// A bulk pass is a strictly synchronous, lexically scoped construct (<c>BeginPass</c>/<c>EndPass</c>
	/// in a <c>try</c>/<c>finally</c>), so a thread-local stack models it exactly: nested passes - whether
	/// on the same handler or on another one - naturally restore their enclosing pass when they end,
	/// concurrent passes on different threads cannot interfere with (or race against) each other, and no
	/// per-pass allocation is needed once a thread's frames have been created.
	/// </remarks>
	internal static class PropertyMapperPassScope
	{
		[ThreadStatic]
		static PropertyMapperPass[]? t_passes;

		[ThreadStatic]
		static int t_depth;

		/// <summary>
		/// Marks the start of a new bulk mapping pass for <paramref name="handler"/> over
		/// <paramref name="keys"/> (the keys of the mapping snapshot being enumerated, in enumeration
		/// order), and returns the frame describing it. The caller must set
		/// <see cref="PropertyMapperPass.CurrentKeyIndex"/> before invoking each key's mapping, and must
		/// pair this with a call to <see cref="EndPass"/> in a <c>finally</c> block so the pass is always
		/// popped - and the enclosing pass, if any, restored - even if a mapping throws.
		/// </summary>
		public static PropertyMapperPass BeginPass(IElementHandler? handler, List<string>? keys)
		{
			var passes = t_passes;
			var depth = t_depth;

			if (passes is null)
			{
				t_passes = passes = new PropertyMapperPass[4];
			}
			else if (depth == passes.Length)
			{
				Array.Resize(ref passes, depth * 2);
				t_passes = passes;
			}

			// Frames are reused across passes on this thread, so a steady-state pass allocates nothing.
			var pass = passes[depth] ??= new PropertyMapperPass();

			pass.Depth = depth;
			pass.Handler = handler;
			pass.Keys = keys;
			pass.CurrentKeyIndex = -1;

			t_depth = depth + 1;

			return pass;
		}

		/// <summary>
		/// Marks the end of <paramref name="pass"/>, restoring whichever pass enclosed it as the current
		/// one. Clears the frame's references so a finished pass never keeps a handler alive and can never
		/// be matched again.
		/// </summary>
		public static void EndPass(PropertyMapperPass pass)
		{
			pass.Handler = null;
			pass.Keys = null;
			pass.CurrentKeyIndex = -1;

			t_depth = pass.Depth;
		}

		/// <summary>
		/// The innermost bulk mapping pass currently running on this thread, or <see langword="null"/> if
		/// none is.
		/// </summary>
		public static PropertyMapperPass? Current
		{
			get
			{
				var depth = t_depth;
				return depth > 0 ? t_passes![depth - 1] : null;
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> when the caller is running as <paramref name="redirectKey"/>'s
		/// own natural step of a bulk mapping pass for <paramref name="handler"/>, and
		/// <paramref name="canonicalKey"/> has already been mapped earlier in that very same pass - i.e.
		/// when re-dispatching <paramref name="redirectKey"/> into <paramref name="canonicalKey"/> is
		/// provably redundant.
		/// </summary>
		/// <remarks>
		/// This deliberately says nothing about <em>which</em> mapping is registered for
		/// <paramref name="canonicalKey"/>: a derived handler is free to own/override it (ButtonHandler,
		/// PageHandler, ...). All that matters is that the pass already ran whatever mapping that key
		/// resolves to, because that is exactly what re-dispatching would invoke again.
		/// Returns <see langword="false"/> for every other shape of invocation - a standalone
		/// <c>UpdateValue</c>, a re-entrant notification raised by a later mapper in the same pass (the
		/// current step is then that other key, not <paramref name="redirectKey"/>), a pass belonging to a
		/// different handler, or a mapper ordering in which <paramref name="canonicalKey"/> does not
		/// precede <paramref name="redirectKey"/> (or is absent entirely).
		/// </remarks>
		public static bool IsRedundantBulkPassRedirect(IElementHandler? handler, string redirectKey, string canonicalKey)
		{
			if (handler is null)
			{
				return false;
			}

			var pass = Current;
			if (pass is null || !ReferenceEquals(pass.Handler, handler))
			{
				return false;
			}

			var keys = pass.Keys;
			var currentKeyIndex = pass.CurrentKeyIndex;

			// Only the pass's own enumeration step for `redirectKey` is skippable. Anything else reaching
			// this mapping (a re-entrant `UpdateValue` raised while some other key is being mapped, for
			// instance) is a genuine request that must be honored.
			if (keys is null || currentKeyIndex < 0 || currentKeyIndex >= keys.Count ||
				!string.Equals(keys[currentKeyIndex], redirectKey, StringComparison.Ordinal))
			{
				return false;
			}

			// `canonicalKey` must have already had its turn in this same pass. Scanning is bounded by the
			// current step and only ever happens on a redirect key's own turn (a handful of times per
			// pass), so it stays well below the cost of the platform work a redundant redirect would do.
			for (int i = 0; i < currentKeyIndex; i++)
			{
				if (string.Equals(keys[i], canonicalKey, StringComparison.Ordinal))
				{
					return true;
				}
			}

			return false;
		}
	}

	/// <summary>
	/// A single, in-progress bulk <see cref="PropertyMapper.UpdateProperties"/> pass. Instances are pooled
	/// and reused per thread by <see cref="PropertyMapperPassScope"/>; they are only meaningful between the
	/// <c>BeginPass</c> that hands one out and the matching <c>EndPass</c>.
	/// </summary>
	internal sealed class PropertyMapperPass
	{
		/// <summary>This pass's index in its thread's pass stack; restored as the depth when it ends.</summary>
		public int Depth;

		/// <summary>The handler whose properties this pass is mapping.</summary>
		public IElementHandler? Handler;

		/// <summary>The keys being enumerated by this pass, in enumeration order.</summary>
		public List<string>? Keys;

		/// <summary>The index in <see cref="Keys"/> of the key currently being mapped, or -1 if none.</summary>
		public int CurrentKeyIndex;
	}

	public interface IPropertyMapper
	{
		Action<IElementHandler, IElement>? GetProperty(string key);

		IEnumerable<string> GetKeys();

		void UpdateProperties(IElementHandler elementHandler, IElement virtualView);

		void UpdateProperty(IElementHandler elementHandler, IElement virtualView, string property);
	}

	public interface IPropertyMapper<out TVirtualView, out TViewHandler> : IPropertyMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		void Add(string key, Action<TViewHandler, TVirtualView> action);
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper, IPropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(params IPropertyMapper[] chained)
			: base(chained)
		{
		}

		internal PropertyMapper<TVirtualView, TViewHandler> WithFrameworkMappingsSealed()
		{
			SealFrameworkMappings();
			return this;
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var mapping = GetProperty(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");

				// An inherited mapping is used as GetProperty reports it: bound to this mapper when it is sealed, or the
				// chained mapping itself, which a replacement of the key on this mapper never changes.
				if (!_mapper.ContainsKey(key))
					return new Action<TViewHandler, TVirtualView>((h, v) => mapping(h, v));

				// Resolve a local mapping on invocation, as it was after the app customizations recorded so far. A
				// replacement that wraps the returned mapping then still includes framework remaps that run lazily
				// later, as they would have when the remaps ran at startup.
				var sequence = MappingCustomizationSequence.Current;
				return new Action<TViewHandler, TVirtualView>((h, v) => GetPropertyAsOf(key, sequence)?.Invoke(h, v));
			}
			set => Add(key, value);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) =>
			{
				if (v is TVirtualView vv)
				{
					action?.Invoke((TViewHandler)h, vv);
				}
				else
				{
					UpdateChainedProperty(key, h, v);
				}
			});
	}

	public class PropertyMapper<TVirtualView> : PropertyMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(params PropertyMapper[] chained)
			: base(chained)
		{
		}
	}
}