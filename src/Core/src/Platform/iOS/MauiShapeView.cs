// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		public MauiShapeView()
		{
			BackgroundColor = UIColor.Clear;
		}
	}
}