﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22937, "[Android] ToolbarItem font color not updating properly after changing the available state at runtime", PlatformAffected.Android)]
public partial class Issue22937 : ContentPage
{

    public Issue22937()
    {
        InitializeComponent();
    }

	private void ChangeStateClicked(object sender, EventArgs e)
	{
		SaveButton.IsEnabled = !SaveButton.IsEnabled;
	}
}
