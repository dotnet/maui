namespace maui;

public partial class Tabs : TabbedPage
{
    public Tabs()
    {
        InitializeComponent();

        Children.Add(new NavigationPage(new MainPage()) { Title = "Tab 1" });
        Children.Add(new NavigationPage(new MainPage()) { Title = "Tab 2" });
        Children.Add(new NavigationPage(new MainPage()) { Title = "Tab 3" });
    }
}