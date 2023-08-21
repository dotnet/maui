// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSStatusBarPage : ContentPage
	{
		public iOSStatusBarPage()
		{
			InitializeComponent();
		}

		void OnPrefersStatusBarHiddenButtonClicked(object sender, EventArgs e)
		{
			switch (On<iOS>().PrefersStatusBarHidden())
			{
				case StatusBarHiddenMode.Default:
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.True);
					break;
				case StatusBarHiddenMode.True:
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.False);
					break;
				case StatusBarHiddenMode.False:
					On<iOS>().SetPrefersStatusBarHidden(StatusBarHiddenMode.Default);
					break;
			}
		}

		void OnPreferredStatusBarUpdateAnimationButtonClicked(object sender, EventArgs e)
		{
			switch (On<iOS>().PreferredStatusBarUpdateAnimation())
			{
				case UIStatusBarAnimation.None:
					On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Fade);
					break;
				case UIStatusBarAnimation.Fade:
					On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.Slide);
					break;
				case UIStatusBarAnimation.Slide:
					On<iOS>().SetPreferredStatusBarUpdateAnimation(UIStatusBarAnimation.None);
					break;
			}
		}
	}
}
