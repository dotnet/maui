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

		void OnToolbarItemClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}