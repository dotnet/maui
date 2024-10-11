﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 8845, "Picker on windows shows \"Microsoft.Maui.Controls.Picker\" if ItemsSource has an empty string", PlatformAffected.UWP)]
	public partial class Issue8845 : ContentPage
	{
		public Issue8845()
		{
			InitializeComponent();
			var items = new List<object>
				{
					new { DisplayName = (string)null!},
					new { DisplayName = "Not null"},
				};
			picker1.ItemsSource = items;
			picker2.ItemsSource = items;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			picker1.SelectedIndex = 0;
			picker2.SelectedIndex = 1;
		}

	}
}