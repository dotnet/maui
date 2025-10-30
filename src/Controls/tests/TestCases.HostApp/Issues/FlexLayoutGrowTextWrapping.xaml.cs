using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 0, "FlexLayout with Grow=1 text should wrap instead of cutting off on Windows",
		PlatformAffected.Default)]
	public partial class FlexLayoutGrowTextWrapping : ContentPage
	{
		public FlexLayoutGrowTextWrapping()
		{
			InitializeComponent();
		}
	}
}
