namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 24354, "[Android] Grid ColumnSpacing affects child's scrollview content size")]
    public class Issue24354 : ContentPage
    {
        public Issue24354()
        {
            var rootGrid = new Grid
            {
                ColumnSpacing = 30,
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(40) }
            },
                RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
            };
            var entry = new Entry();
            entry.Text = "Test Entry";
            entry.AutomationId = "EntryField";
            Grid.SetColumnSpan(entry, 2);
            rootGrid.Children.Add(entry);
            var scrollContent = new ContentView
            {
                Background = Colors.Orange
            };

            var scrollView = new ScrollView
            {
                Background = Colors.Aqua,
                Content = scrollContent
            };
            Grid.SetRow(scrollView, 1);
            Grid.SetColumnSpan(scrollView, 2);
            rootGrid.Children.Add(scrollView);
            Content = rootGrid;
        }
    }
}