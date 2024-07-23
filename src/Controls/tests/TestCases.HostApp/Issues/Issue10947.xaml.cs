using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 10947, "CollectionView Header and Footer Scrolling", PlatformAffected.iOS)]

public partial class Issue10947 : ContentPage
{
	public Issue10947()
	{
		InitializeComponent();
	}
}
