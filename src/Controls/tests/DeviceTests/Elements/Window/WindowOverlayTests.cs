// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory;

[Category(TestCategory.WindowOverlay)]
public class WindowOverlayTests : ControlsHandlerTestBase
{
	[Fact("Does Not Leak")]
	public async Task DoesNotLeak()
	{
		WeakReference viewReference = null;

		{
			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, _ =>
			{
				var overlay = new WindowOverlay(window);
				viewReference = new(overlay);
				window.AddOverlay(overlay);
				window.RemoveOverlay(overlay);
			});
		}

		await AssertionExtensions.WaitForGC(viewReference);
		Assert.False(viewReference.IsAlive, "WindowOverlay should not be alive!");
	}
}

