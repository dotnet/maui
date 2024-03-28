using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18420, "ViewExtensions RotateYTo and RotateXTo with length 0 crashes on Windows", PlatformAffected.UWP)]
	public partial class Issue18420 : ContentPage
	{
		int _count = 0;

		public Issue18420()
		{
			InitializeComponent();
		}

		private void OnCounterClicked(object sender, EventArgs e)
		{
			_count++;

			if (_count == 1)
				CounterLbl.Text = $"Clicked {_count} time";
			else
				CounterLbl.Text = $"Clicked {_count} times";

			CounterBtn.RotateYTo(10 * (_count + 1), 0);
		}
	}
}