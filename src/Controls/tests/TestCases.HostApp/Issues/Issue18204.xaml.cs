using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 18204, "[iOS] Drawing of Borders lags behind other elements creating bizarre overlaps and glitches", PlatformAffected.iOS)]

public partial class Issue18204 : ContentPage
{
	public Issue18204()
	{
		InitializeComponent();
	}

	private void ButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		button.CancelAnimations();
		var targetHeight = button.HeightRequest == 200.0 ? 500.0 : 200.0;
		button.Animate("Height", new Animation(v => button.HeightRequest = v, button.Height, targetHeight, Easing.Linear));
	}
}
