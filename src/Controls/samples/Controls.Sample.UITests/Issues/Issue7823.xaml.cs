using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 7823, "In a ToolbarItems, if an item has no icon but just text, MAUI uses the icon from the previous page in the Navigation", PlatformAffected.Android)]
	public partial class Issue7823 : ContentPage
	{
		public Issue7823()
		{
			InitializeComponent();
		}
		
		void OnToolbarItemClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue7823Page2());
		}
		
		void OnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue7823Page2());
		}
	}
}