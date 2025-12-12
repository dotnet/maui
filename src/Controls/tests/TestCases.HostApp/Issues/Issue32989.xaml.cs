using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32989, "Exception thrown on .NET 10 Windows when calling Permissions.CheckStatusAsync<Permissions.Microphone>()", PlatformAffected.UWP)]
public partial class Issue32989 : ContentPage
{
    Label statusLabel;

    public Issue32989()
    {
        var titleLabel = new Label
        {
            Text = "Test microphone permission in unpackaged apps",
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center
        };

        var checkPermissionButton = new Button
        {
            Text = "Check Microphone Permission",
            AutomationId = "CheckPermissionButton"
        };
        checkPermissionButton.Clicked += OnCheckPermissionClicked;

        statusLabel = new Label
        {
            Text = "Status: Not checked",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            AutomationId = "StatusLabel"
        };

        Content = new StackLayout
        {
            Padding = 20,
            Spacing = 20,
            Children = { titleLabel, checkPermissionButton, statusLabel }
        };
    }

    async void OnCheckPermissionClicked(object sender, EventArgs e)
    {
        statusLabel.Text = "Checking...";
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        statusLabel.Text = "Test Passed";
    }
}