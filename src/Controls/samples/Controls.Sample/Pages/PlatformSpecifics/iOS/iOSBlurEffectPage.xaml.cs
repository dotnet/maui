// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSBlurEffectPage : ContentPage
	{
		public iOSBlurEffectPage()
		{
			InitializeComponent();
		}

		void OnNoBlurButtonClicked(object sender, EventArgs e)
		{
			image.On<iOS>().UseBlurEffect(BlurEffectStyle.None);
		}

		void OnExtraLightBlurButtonClicked(object sender, EventArgs e)
		{
			image.On<iOS>().UseBlurEffect(BlurEffectStyle.ExtraLight);
		}

		void OnLightBlurButtonClicked(object sender, EventArgs e)
		{
			image.On<iOS>().UseBlurEffect(BlurEffectStyle.Light);
		}

		void OnDarkBlurButtonClicked(object sender, EventArgs e)
		{
			image.On<iOS>().UseBlurEffect(BlurEffectStyle.Dark);
		}
	}
}
