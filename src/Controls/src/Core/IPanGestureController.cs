// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public interface IPanGestureController
	{
		void SendPan(Element sender, double totalX, double totalY, int gestureId);

		void SendPanCanceled(Element sender, int gestureId);

		void SendPanCompleted(Element sender, int gestureId);

		void SendPanStarted(Element sender, int gestureId);
	}
}