// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Content;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class MauiShapeView : PlatformGraphicsView
	{
		public MauiShapeView(Context? context) : base(context)
		{
			ClipToOutline = true;
		}
	}
}