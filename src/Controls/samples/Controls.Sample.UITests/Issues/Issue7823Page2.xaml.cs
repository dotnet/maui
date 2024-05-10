using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue7823Page2 : ContentPage
	{
		public Issue7823Page2()
		{
			InitializeComponent();
		}

		async void OnToolbarItemClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}