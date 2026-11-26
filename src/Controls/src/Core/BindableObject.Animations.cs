#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	// The animation layer: SetterSpecificity.Animation lets an animation show a value without owning the property.
	//
	// While a run holds a property, its frames are written at Animation rank. Every lower write (manual, style,
	// binding, dynamic resource, visual state, trigger) is stored beneath the hold without a notification, and
	// shows when the hold is released. A handler write (user input) ends the hold and stands. Animated values
	// never reach a binding source.
	//
	// Everything here runs only for animation writes or for properties that currently hold an Animation entry;
	// every other write takes the unchanged path at the cost of one specificity compare.
	public abstract partial class BindableObject
	{
		/// <summary>
		/// Writes an animated value. The property's own value sources are left untouched.
		/// </summary>
		/// <remarks>
		/// <para>The first write by an owner takes the property over in place from any previous owner, which
		/// is told it lost the hold. No notification is raised at the switch unless the value changes.</para>
		/// <para>Returns <see cref="AnimationWriteResult.Refused"/> while the property is being set (the write
		/// would be queued and could land after the run released it), and
		/// <see cref="AnimationWriteResult.Excluded"/> for properties the platform or a TwoWay /
		/// OneWayToSource binding writes, which would lose data under a hold.</para>
		/// </remarks>
		internal AnimationWriteResult SetAnimationValue(BindableProperty property, object value, IAnimationHoldOwner owner)
		{
			if (property is null)
				throw new ArgumentNullException(nameof(property));
			if (owner is null)
				throw new ArgumentNullException(nameof(owner));
			if (property.IsReadOnly)
				return AnimationWriteResult.Excluded;

			// Refused before conversion and coercion: coercion has side effects (IsEnabled, InputTransparent) that must
			// not happen for a write that won't land. Checked again afterwards, for reentrancy during those callbacks.
			var context = GetOrCreateContext(property);
			if ((context.Attributes & BindableContextAttributes.IsBeingSet) != 0)
				return AnimationWriteResult.Refused;

			if (!property.TryConvert(ref value))
				throw new ArgumentException($"Cannot convert {value} to type '{property.ReturnType}'", nameof(value));
			if (property.ValidateValue != null && !property.ValidateValue(this, value))
				throw new ArgumentException($"Value is an invalid value for {property.PropertyName}", nameof(value));
			if (property.CoerceValue != null)
				value = property.CoerceValue(this, value);

			if ((context.Attributes & BindableContextAttributes.IsBeingSet) != 0)
				return AnimationWriteResult.Refused;

			var top = context.Values.GetSpecificityAndValue();
			if (top.Key != SetterSpecificity.Animation && IsExcludedFromAnimation(context, top.Key))
				return AnimationWriteResult.Excluded;

			var previousOwner = context.AnimationOwner;
			context.AnimationOwner = owner;

			if (IsSameValue(property, value, top.Value))
				context.Values.SetValue(SetterSpecificity.Animation, value);
			else
				RaiseAnimationChange(context, top.Value, value, AnimationMutation.Store, owner);

			// Either way, a previous owner has lost the property.
			if (previousOwner is not null && !ReferenceEquals(previousOwner, owner))
				previousOwner.OnHoldLost(this, property, AnimationHoldLossReason.TakenOver);

			// The owner's own handler may have ended the hold it just stored; leave nothing behind.
			return ReferenceEquals(context.AnimationOwner, owner) ? AnimationWriteResult.Written : AnimationWriteResult.Lost;
		}

		/// <summary>
		/// Withdraws the animated value. The highest remaining value shows, with one change notification if it
		/// differs from the animated one.
		/// </summary>
		internal void ClearAnimationValue(BindableProperty property, IAnimationHoldOwner owner)
		{
			var context = GetContext(property);
			if (context is null || !ReferenceEquals(context.AnimationOwner, owner))
				return;

			context.AnimationOwner = null;
			if (context.Values.GetSpecificity() != SetterSpecificity.Animation)
				return;

			var animated = context.Values.GetValue();
			var shown = context.Values.GetClearedValue();
			try
			{
				//there's some side effect implemented in CoerceValue (see IsEnabled) that we need to trigger here
				property.CoerceValue?.Invoke(this, shown);

				if (IsSameValue(property, shown, animated))
					context.Values.Remove(SetterSpecificity.Animation);
				else
					RaiseAnimationChange(context, animated, shown, AnimationMutation.Remove, null);
			}
			finally
			{
				// Exception-safe: a throwing coercion or PropertyChanging callback must not leave an animated value that no
				// owner can release (it would hide every later write). A run that claimed the property meanwhile keeps its
				// ownership; the released value is still removed.
				if (context.Values.GetSpecificity() == SetterSpecificity.Animation && ReferenceEquals(context.Values.GetValue(), animated))
					context.Values.Remove(SetterSpecificity.Animation);
			}
		}

		/// <summary>
		/// Keeps <paramref name="value"/> after the run ends without taking the property over: the value is
		/// written in the slot of the property's current base value (binding, resource, style, visual state,
		/// trigger, manual or default), then the hold is released. The value source stays attached and wins
		/// again on its next update. No notification is raised when the committed value is what is shown.
		/// </summary>
		internal void CommitAnimationValue(BindableProperty property, object value, IAnimationHoldOwner owner)
		{
			var context = GetContext(property);
			if (context is null || !ReferenceEquals(context.AnimationOwner, owner))
				return;

			if (!property.TryConvert(ref value))
				throw new ArgumentException($"Cannot convert {value} to type '{property.ReturnType}'", nameof(value));
			if (property.ValidateValue != null && !property.ValidateValue(this, value))
				throw new ArgumentException($"Value is an invalid value for {property.PropertyName}", nameof(value));

			var held = context.Values.GetSpecificity() == SetterSpecificity.Animation;

			// Never promote: the commit replaces the base value at its own rank (default stays default).
			var baseSpecificity = held ? context.Values.GetClearedSpecificity() : context.Values.GetSpecificity();

			if ((context.Attributes & BindableContextAttributes.IsBeingSet) != 0 || !held)
			{
				// Not shown at animation rank (a claim without a value), or the property is being set (rule 13): the hold,
				// if any, is released at once and the value is written at the base value's rank as an ordinary write, which
				// queues behind the property's current write when there is one. That ordinary path also coerces.
				var outerWrite = (context.Attributes & BindableContextAttributes.IsBeingSet) == 0;
				if (held)
					ClearAnimationValue(property, owner);
				else
					context.AnimationOwner = null;

				try
				{
					SetValueCore(property, value, SetValueFlags.None, SetValuePrivateFlags.Converted, baseSpecificity);
				}
				catch when (outerWrite)
				{
					// Rule 14: for its own writes the engine leaves the property usable when a handler throws (the stock
					// path would leave it marked as being set, queueing every later write).
					context.Attributes &= ~BindableContextAttributes.IsBeingSet;
					context.DelayedSetters = null;
					throw;
				}
				return;
			}

			if (property.CoerceValue != null)
				value = property.CoerceValue(this, value);

			context.Values.SetValue(baseSpecificity, value);
			ClearAnimationValue(property, owner);
		}

		/// <summary>
		/// Takes ownership of the property without showing a value (a run in its begin delay, or seeked back before
		/// it starts). A previous owner is told it lost the property; a value it held is overwritten in place by
		/// this owner's first write, never released and re-applied.
		/// </summary>
		internal bool ClaimAnimation(BindableProperty property, IAnimationHoldOwner owner)
		{
			if (property.IsReadOnly)
				return false;

			var context = GetOrCreateContext(property);
			if (context.Values.GetSpecificity() != SetterSpecificity.Animation && IsExcludedFromAnimation(context, context.Values.GetSpecificity()))
				return false;

			var previousOwner = context.AnimationOwner;
			if (ReferenceEquals(previousOwner, owner))
				return true;

			context.AnimationOwner = owner;
			previousOwner?.OnHoldLost(this, property, AnimationHoldLossReason.TakenOver);
			return true;
		}

		/// <summary>
		/// The dispatcher animation runs must use: the one captured when this object was created, else one found in its
		/// hierarchy or on the current thread. Unlike <see cref="Dispatcher"/>, returns null instead of throwing.
		/// </summary>
		internal IDispatcher GetAnimationDispatcher() => _dispatcher ?? this.TryFindDispatcher();

		/// <summary>
		/// A TwoWay or OneWayToSource binding attached while a run owns the property without showing a value (its begin
		/// delay): the run gives the property up, as it would if it were showing one.
		/// </summary>
		void RelinquishClaimForBinding(BindablePropertyContext context, BindingBase binding)
		{
			var mode = binding?.GetRealizedMode(context.Property) ?? BindingMode.Default;
			if (mode != BindingMode.TwoWay && mode != BindingMode.OneWayToSource)
				return;

			var owner = context.AnimationOwner;
			context.AnimationOwner = null;
			owner?.OnHoldLost(this, context.Property, AnimationHoldLossReason.Excluded);
		}

		/// <summary>The value the property would have without the animation (WPF's GetAnimationBaseValue).</summary>
		internal object GetAnimationBaseValue(BindableProperty property)
		{
			var context = property.DefaultValueCreator != null ? GetOrCreateContext(property) : GetContext(property);
			if (context is null)
				return property.DefaultValue;

			return context.Values.GetSpecificity() == SetterSpecificity.Animation
				? context.Values.GetClearedValue()
				: context.Values.GetValue();
		}

		/// <summary>Whether an animation currently shows a value on the property.</summary>
		internal bool HasAnimationValue(BindableProperty property)
		{
			var context = GetContext(property);
			return context is not null && context.Values.GetSpecificity() == SetterSpecificity.Animation;
		}

		/// <summary>The run that holds the property, if any.</summary>
		internal IAnimationHoldOwner GetAnimationOwner(BindableProperty property)
			=> GetContext(property)?.AnimationOwner;

		/// <summary>
		/// Whether a hold would lose data: a value the platform wrote (user input, or a TwoWay binding's value,
		/// both at handler rank) or an active TwoWay / OneWayToSource binding. These are never interpolated.
		/// </summary>
		internal bool IsExcludedFromAnimation(BindableProperty property)
		{
			var context = GetContext(property);
			return context is not null && IsExcludedFromAnimation(context, context.Values.GetSpecificity());
		}

		static bool IsExcludedFromAnimation(BindablePropertyContext context, SetterSpecificity top)
		{
			if (top == SetterSpecificity.FromHandler)
				return true;

			var binding = context.Bindings.GetValue();
			if (binding is null)
				return false;

			var mode = binding.GetRealizedMode(context.Property);
			return mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource;
		}

		// A write at a lower rank while the property is held: stored, not displayed, no notification. Its side
		// effects are evaluated against the value beneath the hold, so that after release the property shows
		// exactly what the same writes produce without an animation.
		void SetValueBeneathAnimation(BindablePropertyContext context, object value, bool currentlyApplying, SetValueFlags attributes, SetterSpecificity specificity)
		{
			var property = context.Property;
			var baseSpecificity = context.Values.GetClearedSpecificity();

			if (specificity == SetterSpecificity.DynamicResourceSetter
				&& (context.Attributes & BindableContextAttributes.IsDynamicResource) != 0
				&& baseSpecificity == SetterSpecificity.ManualValueSetter)
			{
				context.Values.Remove(SetterSpecificity.ManualValueSetter);
				baseSpecificity = context.Values.GetClearedSpecificity();
			}

			context.Values.SetValue(specificity, value);

			if (specificity < baseSpecificity)
				return;

			context.Attributes &= ~BindableContextAttributes.IsDefaultValueCreated;

			bool clearDynamicResources = (attributes & SetValueFlags.ClearDynamicResource) != 0;
			if ((context.Attributes & BindableContextAttributes.IsDynamicResource) != 0
				&& clearDynamicResources
				&& (specificity == SetterSpecificity.ManualValueSetter || (!currentlyApplying && specificity.IsBinding)))
				RemoveDynamicResource(property);

			context.AnimationOwner?.OnBaseValueChanged(this, property);
		}

		// SetBinding on a held property operates on the highest non-Animation entry and keeps the hold, except
		// for a TwoWay or OneWayToSource binding, which ends it first so its source never sees an animated value.
		SetterSpecificity PrepareBindingUnderAnimation(BindablePropertyContext context, BindingBase binding)
		{
			var mode = binding?.GetRealizedMode(context.Property) ?? BindingMode.Default;
			if (mode != BindingMode.TwoWay && mode != BindingMode.OneWayToSource)
				return context.Values.GetClearedSpecificity();

			var owner = context.AnimationOwner;
			try
			{
				ClearAnimationValue(context.Property, owner);
			}
			finally
			{
				owner?.OnHoldLost(this, context.Property, AnimationHoldLossReason.Excluded);
			}
			return context.Values.GetSpecificity();
		}

		void OnAnimationHoldLost(BindablePropertyContext context, AnimationHoldLossReason reason)
		{
			var owner = context.AnimationOwner;
			context.AnimationOwner = null;
			owner?.OnHoldLost(this, context.Property, reason);
		}

		enum AnimationMutation
		{
			Store,
			Remove,
		}

		// Raises a change made by the animation layer. The store is mutated between PropertyChanging and
		// PropertyChanged, so observers of PropertyChanging still read the old value. The property is marked as being
		// set while its handlers run, so writes they make are queued and applied afterwards, as for any write, and the
		// queue stays attached while it drains so writes queued by the drain itself are applied too. The mark is
		// cleared even when a handler throws, so the property stays usable (the stock path leaves it set).
		void RaiseAnimationChange(BindablePropertyContext context, object oldValue, object newValue, AnimationMutation mutation, IAnimationHoldOwner owner)
		{
			var property = context.Property;

			// Nested inside the property's own write (a commit refused under rule 13 releases its hold here): that write
			// stays marked as being set and drains the queue itself.
			var nested = (context.Attributes & BindableContextAttributes.IsBeingSet) != 0;
			context.Attributes |= BindableContextAttributes.IsBeingSet;
			try
			{
				property.PropertyChanging?.Invoke(this, oldValue, newValue);
				NotifyPropertyChanging(property);

				if (mutation == AnimationMutation.Remove)
				{
					context.Values.Remove(SetterSpecificity.Animation);
				}
				else if (ReferenceEquals(context.AnimationOwner, owner))
				{
					context.Values.SetValue(SetterSpecificity.Animation, newValue);
				}
				else
				{
					// A PropertyChanging callback stopped (or replaced) the run before its value was stored: store nothing,
					// so no ownerless value hides later writes. A value the run was taking over from a previous owner is
					// now owned by no one either, so it goes too, and the base value shows. Writes the callbacks queued
					// are still applied below.
					newValue = oldValue;
					if (context.AnimationOwner is null && context.Values.GetSpecificity() == SetterSpecificity.Animation)
					{
						context.Values.Remove(SetterSpecificity.Animation);
						newValue = context.Values.GetValue();
					}
				}

				// Always raised, so PropertyChanging is never left without its PropertyChanged.
				OnBindablePropertySet(property, oldValue, newValue, !IsSameValue(property, newValue, oldValue), true);

				Queue<SetValueArgs> delayQueue = nested ? null : context.DelayedSetters;
				if (delayQueue != null)
				{
					while (delayQueue.Count > 0)
					{
						SetValueArgs s = delayQueue.Dequeue();
						if (s != null)
							SetValueActual(s.Property, s.Context, s.Value, s.CurrentlyApplying, s.Attributes, s.Specificity);
					}

					context.DelayedSetters = null;
				}
			}
			catch
			{
				// Writes queued behind a throwing handler would otherwise land, stale, after some later write.
				if (!nested)
					context.DelayedSetters = null;
				throw;
			}
			finally
			{
				if (!nested)
					context.Attributes &= ~BindableContextAttributes.IsBeingSet;
			}
		}
	}
}
