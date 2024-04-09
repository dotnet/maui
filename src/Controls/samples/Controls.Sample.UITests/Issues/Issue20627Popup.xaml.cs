using System;
using Microsoft.Maui.Controls;
using Mopups.Pages;
using Mopups.Services;

namespace Maui.Controls.Sample.Issues;

public partial class Issue20627Popup : PopupPage
{
	public Issue20627Popup()
	{
		InitializeComponent();
	}

    void OnCounterClicked(Object sender, EventArgs e)
    {
		MopupService.Instance.PopAsync();
    }
}