using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		protected Task<string> GetPlatformControlText(ComboBox platformView)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				// these will only work if you've attached the combobox to a window
				var textBlock =
					platformView
						.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("ContentPresenter")
						?.GetFirstDescendant<TextBlock>();

				if (textBlock != null)
					return textBlock.Text;

				return platformView.SelectedItem?.ToString();
			});
		}
	}
}