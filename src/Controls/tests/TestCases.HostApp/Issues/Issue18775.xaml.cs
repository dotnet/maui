using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18775, "[regression/8.0.3] Cannot control unselected text color of tabs within TabbedPage", PlatformAffected.iOS)]
	public partial class Issue18775 : TabbedPage
	{
		public Issue18775()
		{
			InitializeComponent();
		}

	}

	public partial class Issue18775NavPage : ContentPage
	{
		public Issue18775NavPage()
		{
			this.Content = new Label() { Text = "Label", AutomationId = "MauiLabel" };
		}
	}
}