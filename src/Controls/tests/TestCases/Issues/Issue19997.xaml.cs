using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.ApplicationModel;
using System;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19997, "[iOS] Facing issues with the Entry, DatePicker, and CheckBox components when applying themes", PlatformAffected.iOS)]

public partial class Issue19997 : ContentPage
{
	public Issue19997()
	{
		InitializeComponent();
		Application.Current!.UserAppTheme = AppTheme.Dark;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Application.Current!.UserAppTheme = AppTheme.Light;
	}
}
