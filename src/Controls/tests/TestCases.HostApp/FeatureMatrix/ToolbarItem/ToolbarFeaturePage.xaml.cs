using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ToolbarFeaturePage : NavigationPage
{
    public ToolbarFeaturePage()
    {
        PushAsync(new ToolbarFeatureMainPage());
    }
}
public partial class ToolbarFeatureMainPage : ContentPage
{
    ToolbarItem _cachedSecondary3;
    public Command ClickedCommand { get; }
    public ToolbarFeatureMainPage()
    {
        InitializeComponent();
        ClickedCommand = new Command(() =>
        {
            menuLabel.Text = "You clicked on ToolbarItem: Test Secondary (3)";
        });
        BindingContext = this;
    }
    void ItemClicked(object sender, EventArgs e)
    {
        if (sender is ToolbarItem tbi)
        {
            menuLabel.Text = $"You clicked on ToolbarItem: {tbi.Text}";
        }
    }
    void Button_Clicked(object sender, EventArgs e)
    {
        secondary4.IsEnabled = !secondary4.IsEnabled;
    }
    void Button_Clicked1(object sender, EventArgs e)
    {
        primary1.IsEnabled = !primary1.IsEnabled;
    }
    void Button_Clicked2(object sender, EventArgs e)
    {
        // 1 second delay so you can have the menu open and see the change.
        // However, the menu will close if change happens. There is no way around this.
        Task.Delay(1000).ContinueWith(t =>
        {
            Dispatcher.Dispatch(() =>
            {
                secondary2.IsEnabled = !secondary2.IsEnabled;
            });
        });
    }
    void Button_Clicked3(object sender, EventArgs e)
    {
        secondary1.Text = secondary1.Text == "Test Secondary (1)" ? "Changed Text" : "Test Secondary (1)";
    }
    void Button_Clicked4(object sender, EventArgs e)
    {
        _cachedSecondary3 ??= secondary3;
        if (ToolbarItems.Contains(_cachedSecondary3))
        {
            ToolbarItems.Remove(_cachedSecondary3);
        }
        else
        {
            ToolbarItems.Add(_cachedSecondary3);
        }
    }
    void Button_Clicked5(object sender, EventArgs e)
    {
        secondary3.Command = new Command(() =>
        {
            menuLabel.Text = "You clicked on ToolbarItem: Test Secondary (3) with changed Command";
        });
    }
}
