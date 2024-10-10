using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20736, "Editor control does not scroll properly on iOS when enclosed in a Border control", PlatformAffected.iOS)]
public partial class Issue20736 : ContentPage
{
	public Issue20736()
    {
        InitializeComponent();
	}
}