using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSSearchBarPage : ContentPage
	{
		public iOSSearchBarPage()
		{
			InitializeComponent();
		}

		void OnSearchBarStyleButtonClicked(object? sender, EventArgs e)
		{
			switch (searchBar.On<iOS>().GetSearchBarStyle())
			{
				case UISearchBarStyle.Default:
					searchBar.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Minimal);
					break;
				case UISearchBarStyle.Minimal:
					searchBar.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Prominent);
					break;
				case UISearchBarStyle.Prominent:
					searchBar.On<iOS>().SetSearchBarStyle(UISearchBarStyle.Default);
					break;
			}
		}

		void OnToggleBackgroundButtonClicked(object? sender, EventArgs e)
		{
			searchBar.BackgroundColor = (searchBar.BackgroundColor == Colors.Teal) ? Colors.Black : Colors.Teal;
		}
	}
}
