using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 20685, "MenuBarItem Commands not working on Mac Catalyst",
        PlatformAffected.All)]
    public class Issue20685 : TestShell
    {
        protected override void Init()
        {
            // Create a label to display results
            var resultLabel = new Label
            {
                Text = "No action performed yet",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                AutomationId = "ResultLabel"
            };

            // Set up MenuBarItems with both Clicked and Command handlers
            var menuBarItem1 = new MenuBarItem { Text = "Clicked Events" };
            var menuBarItem2 = new MenuBarItem { Text = "Commands" };
            var menuBarItem3 = new MenuBarItem { Text = "Command With Param" };
            var menuBarItem4 = new MenuBarItem { Text = "Disabled" };

            // Add an item that uses the Clicked event
            var clickedItem = new MenuFlyoutItem
            {
                Text = "Clicked Event Item",
                AutomationId = "ClickedEventItem"
            };
            clickedItem.Clicked += (s, e) =>
            {
                resultLabel.Text = "Clicked event handler executed";
            };
            menuBarItem1.Add(clickedItem);

            // Add an item that uses a Command
            var commandItem = new MenuFlyoutItem
            {
                Text = "Command Item",
                AutomationId = "CommandItem",
                Command = new Command(() =>
                {
                    resultLabel.Text = "Command executed";
                })
            };
            menuBarItem2.Add(commandItem);

            // Add an item that uses a Command with parameter
            var commandWithParamItem = new MenuFlyoutItem
            {
                Text = "Command with Parameter",
                AutomationId = "CommandWithParamItem",
                Command = new Command<string>((param) =>
                {
                    resultLabel.Text = $"Command executed with parameter: {param}";
                }),
                CommandParameter = "Test Parameter"
            };
            menuBarItem3.Add(commandWithParamItem);

            // Add a disabled item to test the disabled state
            var disabledItem = new MenuFlyoutItem
            {
                Text = "Disabled Item",
                AutomationId = "DisabledItem",
                IsEnabled = false
            };
            menuBarItem4.Add(disabledItem);

            // Add the menu bar items to the Shell
            MenuBarItems.Add(menuBarItem1);
            MenuBarItems.Add(menuBarItem2);
            MenuBarItems.Add(menuBarItem3);
            MenuBarItems.Add(menuBarItem4);

            // Create a content page with a simple layout
            var contentPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 20,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label 
                        {
                            Text = "MenuFlyoutItem Delegate Test",
                            FontSize = 24,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        resultLabel,
                        new Label
                        {
                            Text = "Use the menu bar items at the top of the screen to test delegate implementations",
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    }
                }
            };
            
            // Add the content page to the Shell
            AddContentPage(contentPage);
        }
    }
}
