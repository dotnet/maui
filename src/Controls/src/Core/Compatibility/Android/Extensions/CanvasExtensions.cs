// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Android.Content;
using Android.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class CanvasExtensions
	{
		public static void ClipShape(this Canvas canvas, Context context, VisualElement element)
		{
			if (canvas == null || element == null)
				return;

			var geometry = element.Clip;

			if (geometry == null)
				return;

			var path = geometry.ToAPath(context);
			canvas.ClipPath(path);
		}
	}
}