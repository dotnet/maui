using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public abstract class MauiView : UIView
	{
		static bool? _respondsToSafeArea;

		WeakReference<IView>? _reference;

		public IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		bool RespondsToSafeArea()
		{
			if (_respondsToSafeArea.HasValue)
				return _respondsToSafeArea.Value;
			return (bool)(_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets")));
		}

		protected CGRect AdjustForSafeArea(CGRect bounds)
		{
			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea || !RespondsToSafeArea())
			{
				return bounds;
			}

#pragma warning disable CA1416 // TODO 'UIView.SafeAreaInsets' is only supported on: 'ios' 11.0 and later, 'maccatalyst' 11.0 and later, 'tvos' 11.0 and later.
			return SafeAreaInsets.InsetRect(bounds);
#pragma warning restore CA1416
		}
	}
}
