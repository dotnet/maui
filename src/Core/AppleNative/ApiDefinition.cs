using System;
using CoreAnimation;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	[Category]
	[Internal]
	[BaseType(typeof(CALayer))]
	internal interface CALayer_MauiAutoSizeToSuperLayer
	{
		[Export("mauiAutoSizeToSuperLayer")]
		bool GetMauiAutoSizeToSuperLayer();

		[Export("setMauiAutoSizeToSuperLayer:")]
		void SetMauiAutoSizeToSuperLayer(bool value);
	}
}