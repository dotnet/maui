using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Graphics;


namespace Microsoft.Maui.Platform
{
	public static class NavigationViewExtensions
	{
		public static void UpdateTopNavAreaBackground(this NavigationView navigationView, Paint? paint)
		{
			navigationView.UpdateThemeDictionaries("NavigationViewTopPaneBackground", paint?.ToNative());
		}
	}
}
