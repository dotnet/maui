﻿using System;
using System.Drawing;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSModalPagePresentationStyle : ContentPage
	{
		//bool childPage = false;
		public Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle presentationStyle = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIModalPresentationStyle.Automatic;
		public iOSModalPagePresentationStyle()
		{
			InitializeComponent();
		}
		public iOSModalPagePresentationStyle(UIModalPresentationStyle presentationStyle, View? modalOrigin=null, Rectangle? rectangle = null)
		{
			InitializeComponent();
			if(modalOrigin != null){
				On<iOS>().SetModalPopoverView(modalOrigin);
			}
			if(rectangle != null){
				On<iOS>().SetModalPopoverRect(rectangle.Value);
			}
			On<iOS>().SetModalPresentationStyle(presentationStyle);
		}

		async void OnPushFormSheetClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.FormSheet);
			await Navigation.PushModalAsync(pushMe);
		}
		async void OnPushPopoverClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.Popover, originButton);
			await Navigation.PushModalAsync(pushMe);
		}
		async void OnPushPopoverOffsetClicked(object sender, EventArgs e)
		{
			var offset = new System.Drawing.Rectangle(0,0,100,10);
			Microsoft.Maui.Controls.Page pushMe = new iOSModalPagePresentationStyle(UIModalPresentationStyle.Popover, originButton2, offset);
			await Navigation.PushModalAsync(pushMe);
		}

		async void OnReturnButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}
