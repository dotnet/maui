// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public interface ISwipeGestureController
	{
		void SendSwipe(Element sender, double totalX, double totalY);
		bool DetectSwipe(View sender, SwipeDirection direction);
	}
}