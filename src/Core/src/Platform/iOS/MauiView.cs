using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public abstract class MauiView : UIView 
	{
		static bool? _respondsToSafeArea;

		public IView? View { get; set; }

		bool RespondsToSafeArea()
		{
			if (_respondsToSafeArea.HasValue)
				return _respondsToSafeArea.Value;
			return (bool)(_respondsToSafeArea = RespondsToSelector(new Selector("safeAreaInsets")));
		}

		protected CGRect AdjustForSafeArea(CGRect frame)
		{
			if (View is not ISafeAreaView sav || sav.IgnoreSafeArea || !RespondsToSafeArea())
			{
				return frame;
			}
			
			return SafeAreaInsets.InsetRect(frame);
		}
	}
}