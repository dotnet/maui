using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : HandlerTestBase
	{
		protected Task<string> GetPlatformControlText(ComboBox platformView)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return platformView.SelectedItem?.ToString();
			});
		}
	}
}