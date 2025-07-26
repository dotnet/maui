using System;
using CoreAnimation;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	[Category]
	[Internal]
	[BaseType(typeof(CALayer))]
	internal interface CALayer_AutoSizeToSuperLayer
	{
		[Export("autoSizeToSuperLayer")]
		bool GetAutoSizeToSuperLayer();

		[Export("setAutoSizeToSuperLayer:")]
		void SetAutoSizeToSuperLayer(bool value);
	}
}