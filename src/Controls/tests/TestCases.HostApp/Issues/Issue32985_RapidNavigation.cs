namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32985, "Shell rapid overlapping navigations should not drop pages", PlatformAffected.Android)]
public class Issue32985_RapidNavigation : TestShell
{
    protected override void Init()
    {
        var statusLabel = new Label
        {
            AutomationId = "StatusLabel",
            Text = "Ready"
        };

        var stackDepthLabel = new Label
        {
            AutomationId = "StackDepthLabel",
            Text = "Stack: 1"
        };

        var rapidPushButton = new Button
        {
            Text = "Rapid Push Two Pages",
            AutomationId = "RapidPushButton",
            Command = new Command(async () =>
            {
                statusLabel.Text = "Pushing...";

                // Fire two rapid PushAsync calls without awaiting the first.
                // StackNavigationManager should queue the second instead of throwing.
                var push1 = Navigation.PushAsync(new ContentPage
                {
                    Title = "Page 2",
                    Content = new VerticalStackLayout
                    {
                        Spacing = 15,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "Page 2 (first rapid push)",
                                AutomationId = "Page2Label",
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Button
                            {
                                Text = "Go Back from Page 2",
                                AutomationId = "GoBackFromPage2Button",
                                Command = new Command(async () => await Navigation.PopAsync())
                            }
                        }
                    }
                });

                var push2 = Navigation.PushAsync(new ContentPage
                {
                    Title = "Page 3",
                    Content = new VerticalStackLayout
                    {
                        Spacing = 15,
                        Padding = new Thickness(20),
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "Page 3 (second rapid push)",
                                AutomationId = "Page3Label",
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                AutomationId = "FinalStackDepth",
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Button
                            {
                                Text = "Check Stack Depth",
                                AutomationId = "CheckStackButton",
                                Command = new Command(() =>
                                {
                                    var depth = Navigation.NavigationStack.Count;
                                    if (Shell.Current?.CurrentPage is ContentPage cp &&
                                        cp.Content is VerticalStackLayout vsl)
                                    {
                                        foreach (var child in vsl.Children)
                                        {
                                            if (child is Label l && l.AutomationId == "FinalStackDepth")
                                            {
                                                l.Text = $"Depth: {depth}";
                                                break;
                                            }
                                        }
                                    }
                                })
                            },
                            new Button
                            {
                                Text = "Go Back",
                                AutomationId = "GoBackButton",
                                Command = new Command(async () => await Navigation.PopAsync())
                            }
                        }
                    }
                });

                await Task.WhenAll(push1, push2);
                statusLabel.Text = "Done";
                stackDepthLabel.Text = $"Stack: {Navigation.NavigationStack.Count}";
            })
        };

        var page = new ContentPage
        {
            Title = "Rapid Navigation Test",
            Content = new VerticalStackLayout
            {
                Spacing = 15,
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Rapid Overlapping Navigation Test",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Tests that two rapid PushAsync calls both complete without dropping pages.",
                        FontSize = 13,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    rapidPushButton,
                    statusLabel,
                    stackDepthLabel
                }
            }
        };

        AddContentPage(page);
    }
}
