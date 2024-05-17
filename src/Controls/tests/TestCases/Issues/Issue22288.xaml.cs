namespace Maui.Controls.Sample.Issues;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

[Issue(IssueTracker.Github, 22288, "Top Button Content Causes Infinite Layout", PlatformAffected.iOS)]
public partial class Issue22288 : ContentPage
{
	public Issue22288()
	{
		InitializeComponent();
	}
}