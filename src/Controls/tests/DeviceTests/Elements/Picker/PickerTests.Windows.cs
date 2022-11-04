using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : HandlerTestBase
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