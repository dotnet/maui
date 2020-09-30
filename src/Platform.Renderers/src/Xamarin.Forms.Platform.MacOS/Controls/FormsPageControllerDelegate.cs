using System;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class FormsPageControllerDelegate : NSPageControllerDelegate
	{
		readonly Func<NSObject, string> _getIdentifier;
		readonly Func<string, NSViewController> _getViewController;

		public FormsPageControllerDelegate(Func<NSObject, string> getIdentifier,
			Func<string, NSViewController> getViewController)
		{
			_getIdentifier = getIdentifier;
			_getViewController = getViewController;
		}

		public override NSViewController GetViewController(NSPageController pageController, string identifier)
		{
			return _getViewController(identifier);
		}

		public override string GetIdentifier(NSPageController pv, NSObject obj)
		{
			return _getIdentifier(obj);
		}
	}
}