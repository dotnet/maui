using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 24414, "Shadows not rendering as expected on Android and iOS", PlatformAffected.Android | PlatformAffected.iOS)]

public partial class Issue24414 : ContentPage
{
	public Issue24414()
	{
		InitializeComponent();
	}
}
