using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22528, "Prevent the label text from being cut off from the top when the specified LineHeight",
		PlatformAffected.iOS)]
	public partial class Issue22528 : ContentPage
	{
		public Issue22528()
		{
			InitializeComponent();

		}
	}
}