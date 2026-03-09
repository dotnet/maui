using System;
using System.Drawing;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSModalPagePresentationStyle : ContentPage
	{
		bool isChildPage;

		public Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle presentationStyle =
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.Automatic;

		public iOSModalPagePresentationStyle()
		{
			InitializeComponent();
		}

		public iOSModalPagePresentationStyle(UIModalPresentationStyle presentationStyle, bool isChildPage = true,
			View? modalOrigin = null, Rectangle? rectangle = null, Microsoft.Maui.Graphics.Size? popoverSize = null,
			UIPopoverArrowDirection? arrowDirections = null)
		{
			InitializeComponent();

			if (modalOrigin is not null)
			{
				On<iOS>().SetModalPopoverView(modalOrigin);
			}

			if (rectangle is not null)
			{
				On<iOS>().SetModalPopoverRect(rectangle.Value);
			}

			if (popoverSize is not null)
			{
				On<iOS>().SetModalPopoverContentSize(popoverSize.Value);
			}

			if (arrowDirections is not null)
			{
				On<iOS>().SetModalPopoverArrowDirections(arrowDirections.Value);
			}

			On<iOS>().SetModalPresentationStyle(presentationStyle);

			this.isChildPage = isChildPage;
		}

		async void OnPushFormSheetClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.FormSheet, true);
			await Navigation.PushModalAsync(pushMe);
		}

		async void OnPushPopoverClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.Popover, true, originButton);
			await Navigation.PushModalAsync(pushMe);
		}

		async void OnPushPopoverOffsetClicked(object sender, EventArgs e)
		{
			var offset = new System.Drawing.Rectangle(0, 0, 100, 10);
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.Popover, true, originButton2, offset);
			await Navigation.PushModalAsync(pushMe);
		}

		async void OnPushPopoverDirectionSizeClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.Popover, true, originButton3, null, new Microsoft.Maui.Graphics.Size(240, 240), UIPopoverArrowDirection.Any);
			await Navigation.PushModalAsync(pushMe);
		}

		async void OnReturnButtonClicked(object sender, EventArgs e)
		{
			if (isChildPage)
			{
				await Navigation.PopModalAsync();
			}
			else
			{
				await Navigation.PopAsync();
			}
		}
	}
}
