using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;


namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24516, "Rendering issue in WinUI when setting Label.FormattedText", PlatformAffected.UWP)]
	public partial class Issue24516 : ContentPage
	{
		int count = 0;
		public Issue24516()
		{
			InitializeComponent();
		}

		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			if (count % 2 == 0)
			{
				label.Text = "Hello, World!";
			}
			else
			{
				var formattedString = new FormattedString();
				formattedString.Spans.Add(new Span { Text = "Hello", TextColor = Color.FromRgb(0xB0, 0x0F, 0x50) });
				formattedString.Spans.Add(new Span { Text = ", World!" });
				label.FormattedText = formattedString;
			}
		}
	}
}