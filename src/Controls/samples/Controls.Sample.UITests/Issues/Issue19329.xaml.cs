using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19329, "Pointer gestures should work with relative positions correctly", PlatformAffected.iOS)]
	public partial class Issue19329 : ContentPage
	{
		public Issue19329()
		{
			InitializeComponent();
		}

		private void OnTapped(object sender, TappedEventArgs e)
		{
			// Report that the callback was called.
			layout.Children.Add(new Label { Text = "TapAccepted", AutomationId = "TapAccepted" });

			Point? position = e.GetPosition(relativeTo: UpperBox);

			string result;

			if (position is null)
			{
				result = "Error: position is null";
			}
			else if (position.Value.X <= 40 && position.Value.Y <= 40)
			{
				result = "Success";
			}
			else
			{
				result = $"Error: relative position is: X={position.Value.X},Y={position.Value.Y}";
			}

			// Report the result of the test.
			layout.Children.Add(new Label { Text = result, AutomationId = result });
		}
	}
}
