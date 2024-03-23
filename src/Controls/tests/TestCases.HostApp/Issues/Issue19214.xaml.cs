using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19214, "iOS Keyboard Scrolling ContentInset Tests", PlatformAffected.iOS)]
public partial class Issue19214 : ContentPage
{
	public Issue19214()
	{
		InitializeComponent();
	}

	async void PushMultipleScrollViews(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue19214_MultipleScrollViews());
	}

	async void PushFullScrollView(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Issue19214_FullScrollView());
	}
}