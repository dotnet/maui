using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Graphics;


namespace Microsoft.Maui.Platform
{
	public static class NavigationViewExtensions
	{
		public static void UpdateTopNavAreaBackground(this MauiNavigationView navigationView, Paint? paint)
		{
			// Background property is set via {ThemeResource NavigationViewTopPaneBackground} in the Control Template
			// AFAICT you can't modify properties set by ThemeResource at runtime so we have to just update this value directly
			if (paint != null)
				navigationView.TopNavArea?.UpdateBackground(paint, null);
		}
	}
}
