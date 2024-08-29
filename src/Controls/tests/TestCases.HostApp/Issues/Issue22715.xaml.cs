using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22715, "Page should not scroll when focusing element above keyboard", PlatformAffected.iOS)]
public partial class Issue22715 : ContentPage
{
	public Issue22715()
	{
		InitializeComponent();
	}

	private void OnPageLoaded(object sender, EventArgs e)
	{
		EntNumber.Focus();
	}
}