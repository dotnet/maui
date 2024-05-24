using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22606, "Border doesnt expand on Content size changed", PlatformAffected.Android)]
	public partial class Issue22606 : ContentPage
	{
		public Issue22606()
		{
			InitializeComponent();		
		}

		void OnSetHeightTo200Clicked(object sender, EventArgs e)
		{
			content.HeightRequest = 200;
		}

		void OnSetHeightTo500Clicked(object sender, EventArgs e)
		{
			content.HeightRequest = 500;
		}
	}
}