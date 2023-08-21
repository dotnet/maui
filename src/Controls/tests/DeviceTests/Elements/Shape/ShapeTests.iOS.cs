// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeTests
	{
		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler<ButtonHandler>(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}

		UIButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;
	}
}