#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Why a run lost a <c>(target, property)</c> track.</summary>
	internal enum TrackLossReason
	{
		/// <summary>Another run took the track over in place.</summary>
		TakenOver,

		/// <summary>The platform wrote a new value, for example user input; that value stands.</summary>
		HandlerWrite,
	}

	/// <summary>A run that owns animation tracks.</summary>
	internal interface IAnimationTrackOwner
	{
		void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason);
	}

	/// <summary>
	/// The animation layer (spec Part A). Values are shown at <see cref="SetterSpecificity.Animation"/>
	/// rank: above styles, triggers, visual states and manual values, below handler writes. Everything
	/// beneath a hold keeps updating silently and surfaces when the hold is released.
	/// </summary>
	internal static class AnimationLayer
	{
		sealed class Owners
		{
			public readonly Dictionary<BindableProperty, IAnimationTrackOwner> Map = new();
		}

		static readonly ConditionalWeakTable<BindableObject, Owners> s_owners = new();

		public static IAnimationTrackOwner? GetOwner(BindableObject target, BindableProperty property)
			=> s_owners.TryGetValue(target, out var owners) && owners.Map.TryGetValue(property, out var owner) ? owner : null;

		/// <summary>
		/// Takes ownership of <paramref name="property"/>. A previous owner is told it lost the track. Its
		/// animation entry stays in place, so the takeover neither jumps nor notifies (rule 4).
		/// </summary>
		public static void Acquire(BindableObject target, BindableProperty property, IAnimationTrackOwner owner)
		{
			var owners = s_owners.GetValue(target, static _ => new Owners());
			if (owners.Map.TryGetValue(property, out var previous) && ReferenceEquals(previous, owner))
				return;

			owners.Map[property] = owner;
			previous?.OnTrackLost(target, property, TrackLossReason.TakenOver);
		}

		public static bool Owns(BindableObject target, BindableProperty property, IAnimationTrackOwner owner)
			=> ReferenceEquals(GetOwner(target, property), owner);

		/// <summary>
		/// Shows <paramref name="value"/> at animation rank (rule 1). The caller must own the track. Returns
		/// <see langword="false"/>, writing nothing, while the property is in the middle of being set.
		/// </summary>
		public static bool Write(BindableObject target, BindableProperty property, object? value)
			=> target.SetAnimatedValue(property, value!);

		/// <summary>Removes the hold but keeps ownership: the base value shows until the owner writes again.</summary>
		public static void Clear(BindableObject target, BindableProperty property, IAnimationTrackOwner owner)
		{
			if (Owns(target, property, owner))
				target.ClearAnimatedValue(property);
		}

		/// <summary>Removes the hold; the highest remaining value is promoted (rule 2).</summary>
		public static void Release(BindableObject target, BindableProperty property, IAnimationTrackOwner owner)
		{
			if (!Forget(target, property, owner))
				return;

			target.ClearAnimatedValue(property);
		}

		/// <summary>
		/// Keeps <paramref name="value"/> after the run without taking ownership of the property (rule 6).
		/// The value is written at the rank of the current base value while the hold is still active, then
		/// the hold is released. Bindings, resources, styles and visual states stay attached and win again
		/// on their next update. No notification is raised when the hold already shows the value.
		/// </summary>
		public static void CommitAndRelease(BindableObject target, BindableProperty property, object? value, IAnimationTrackOwner owner)
		{
			if (!Owns(target, property, owner))
				return;

			target.GetAnimationBaseValue(property, out var baseSpecificity);
			target.SetValueForAnimation(property, value, baseSpecificity);
			Release(target, property, owner);
		}

		/// <summary>The value the property shows without the hold (rule 3).</summary>
		public static object? GetBaseValue(BindableObject target, BindableProperty property)
			=> target.GetAnimationBaseValue(property, out _);

		/// <summary>TwoWay or OneWayToSource bound properties and platform-written values are never interpolated (rule 8).</summary>
		public static bool IsExcluded(BindableObject target, BindableProperty property)
			=> target.IsExcludedFromAnimation(property);

		/// <summary>
		/// Called by <see cref="BindableObject"/> after a hold ended from outside the animation layer: a handler
		/// wrote a new value (rule 9), or a TwoWay or OneWayToSource binding was attached (rule 8).
		/// </summary>
		internal static void OnHandlerWrite(BindableObject target, BindableProperty property)
		{
			if (!s_owners.TryGetValue(target, out var owners) || !owners.Map.TryGetValue(property, out var owner))
				return;

			owners.Map.Remove(property);
			owner.OnTrackLost(target, property, TrackLossReason.HandlerWrite);
		}

		static bool Forget(BindableObject target, BindableProperty property, IAnimationTrackOwner owner)
		{
			if (!s_owners.TryGetValue(target, out var owners)
				|| !owners.Map.TryGetValue(property, out var current)
				|| !ReferenceEquals(current, owner))
				return false;

			owners.Map.Remove(property);
			return true;
		}
	}
}
