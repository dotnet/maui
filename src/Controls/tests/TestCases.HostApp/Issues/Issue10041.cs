namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 10041, "Throws exception when shell item is null", PlatformAffected.iOS)]
    public class Issue10041 : TestShell
    {
        protected override void Init()
        {
            // FlyoutBehavior = FlyoutBehavior.Locked;
            // FlyoutBackgroundColor = Colors.Red;
            // FlyoutIsPresented = true;
            // FlyoutContentTemplate = new DataTemplate(() =>
            // {
            //     var stackLayout = new StackLayout();

            //     for (int i = 1; i <= 3; i++)
            //     {
            //         var label = new Label
            //         {
            //             Text = $"Page {i}",
            //             FontSize = 18,
            //             TextColor = Colors.Black,
            //             AutomationId = $"FlyoutPage{i}"
            //         };
            //         stackLayout.Children.Add(label);
            //     }
            //     stackLayout.Children.Add(new Button
            //     {
            //         Text = "Close",
            //         FontSize = 18,
            //         TextColor = Colors.Black,
            //         AutomationId = "CloseButton"
            //     });

            //     return stackLayout;
            // });
        }
    }
}
