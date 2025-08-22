using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24520, "Change the LineStackingStrategy to BlockLineHeight for Labels on Windows", PlatformAffected.UWP)]
public partial class Issue24520 : ContentPage
{
	public Issue24520()
	{
		InitializeComponent();
	}
}