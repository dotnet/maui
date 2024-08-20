using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17801, "ScrollView always has a scroll bar on iOS", PlatformAffected.iOS)]
	public partial class Issue17801 : ContentPage
	{
		int _count = 0;

		public Issue17801()
		{
			InitializeComponent();
		}
		
		void OnCounterClicked(object sender, EventArgs e)
		{
			_count++;

			if (_count == 1)
				CounterBtn.Text = $"Clicked {_count} time";
			else
				CounterBtn.Text = $"Clicked {_count} times";
		}
	}
}