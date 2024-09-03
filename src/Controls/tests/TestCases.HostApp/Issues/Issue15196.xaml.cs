using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 15196, "Nested Entry View In A Frame Causes Crash", PlatformAffected.Android)]
	public partial class Issue15196 : ContentPage
	{
		public Issue15196()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			if (stackLayout.Children.Contains(frame))
			{
				// Remove the frame from the stackLayout
				stackLayout.Children.Remove(frame);
			}
			
		frame?.Handler?.DisconnectHandler();
		entry?.Handler?.DisconnectHandler();
		}
	}
}