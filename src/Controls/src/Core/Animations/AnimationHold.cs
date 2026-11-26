#nullable enable
namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>The owner of an animation hold on a (target, property): a storyboard player, or another front end.</summary>
	internal interface IAnimationHoldOwner
	{
		/// <summary>The owner no longer owns the property. Raised after the change that caused it; the owner must
		/// not take the property straight back from here.</summary>
		void OnHoldLost(BindableObject target, BindableProperty property, AnimationHoldLossReason reason);

		/// <summary>A write beneath the hold changed the property's base value.</summary>
		void OnBaseValueChanged(BindableObject target, BindableProperty property);
	}

	internal enum AnimationHoldLossReason
	{
		/// <summary>Another run took the property over, in place.</summary>
		TakenOver,

		/// <summary>The platform wrote the property (user input), and its value stands.</summary>
		HandlerWrite,

		/// <summary>A TwoWay or OneWayToSource binding was attached; such properties are never interpolated.</summary>
		Excluded,
	}

	internal enum AnimationWriteResult
	{
		Written,

		/// <summary>The property is being set; retry on the next frame.</summary>
		Refused,

		/// <summary>The property is read-only, platform-written or TwoWay/OneWayToSource-bound.</summary>
		Excluded,

		/// <summary>The write's own change notification ended the hold.</summary>
		Lost,
	}
}
