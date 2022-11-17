using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		protected Task<string> GetPlatformControlText(MauiPicker platformView)
		{
			return InvokeOnMainThreadAsync(() => platformView.Text);
		}
	}
}