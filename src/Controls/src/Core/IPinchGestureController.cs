// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IPinchGestureController
	{
		bool IsPinching { get; set; }

		void SendPinch(Element sender, double scale, Point currentScalePoint);

		void SendPinchCanceled(Element sender);

		void SendPinchEnded(Element sender);

		void SendPinchStarted(Element sender, Point intialScalePoint); //TODO: intial should be initial, but this is a breaking ABI change. Consider changing for .NET 8
	}
}