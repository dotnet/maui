using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10660, "Inconsistent toolbar text color on interaction", PlatformAffected.Android)]
public partial class Issue10660 : ContentPage
{

    public Issue10660()
    {
        InitializeComponent();
    }

	private void ChangeStateClicked(object sender, EventArgs e)
	{
		ChangeState.Text = ChangeState.Text == "Close" ? "Open" : "Close";
	}
}
