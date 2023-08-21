// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public interface ISwipeViewController
	{
		bool IsOpen { get; set; }
		void SendSwipeStarted(SwipeStartedEventArgs args);
		void SendSwipeChanging(SwipeChangingEventArgs args);
		void SendSwipeEnded(SwipeEndedEventArgs args);
	}
}