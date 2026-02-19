using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32994, "Shell TabBarIsVisible binding not working on ShellContent", PlatformAffected.All)]
public class Issue32994 : Shell
{
    public Issue32994()
    {
        var viewModel = new Issue32994ViewModel();
        BindingContext = viewModel;

        var tab1 = new Tab
        {
            Title = "Tab1",
            AutomationId = "Tab1"
        };

        var page1Content = new ShellContent
        {
            Title = "Page1",
            AutomationId = "Page1Tab"
        };
        page1Content.Content = new Issue32994Page1(page1Content, viewModel);
        tab1.Items.Add(page1Content);

        var page2Content = new ShellContent
        {
            Title = "Page2",
            AutomationId = "Page2Tab"
        };
        page2Content.SetBinding(Shell.TabBarIsVisibleProperty, nameof(Issue32994ViewModel.TabBarIsVisible));
        page2Content.Content = new Issue32994Page2();
        tab1.Items.Add(page2Content);

        Items.Add(tab1);

        var tab2 = new Tab
        {
            Title = "Tab2",
            AutomationId = "Tab2"
        };
        tab2.Items.Add(new ShellContent
        {
            Title = "Tab2Content",
            Content = new ContentPage
            {
                Content = new Label
                {
                    Text = "Welcome to Tab 2",
                    AutomationId = "Tab2Label"
                }
            }
        });
        Items.Add(tab2);
    }
}

public class Issue32994Page1 : ContentPage
{
    private readonly ShellContent _page1Content;
    private readonly Issue32994ViewModel _viewModel;

    public Issue32994Page1(ShellContent page1Content, Issue32994ViewModel viewModel)
    {
        _page1Content = page1Content;
        _viewModel = viewModel;
        Title = "Page1";

        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 15
        };

        layout.Children.Add(new Label
        {
            Text = "This is Page1",
            FontAttributes = FontAttributes.Bold
        });

        layout.Children.Add(new Label
        {
            Text = "Control Page1 Tab (Direct SetTabBarIsVisible):",
            Margin = new Thickness(0, 10, 0, 0)
        });

        var page1ButtonLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };

        var showPage1Button = new Button
        {
            Text = "Show TabBar",
            WidthRequest = 120,
            AutomationId = "ShowPage1TabBar"
        };
        showPage1Button.Clicked += (s, e) => Shell.SetTabBarIsVisible(_page1Content, true);
        page1ButtonLayout.Children.Add(showPage1Button);

        var hidePage1Button = new Button
        {
            Text = "Hide TabBar",
            WidthRequest = 120,
            AutomationId = "HidePage1TabBar"
        };
        hidePage1Button.Clicked += (s, e) => Shell.SetTabBarIsVisible(_page1Content, false);
        page1ButtonLayout.Children.Add(hidePage1Button);

        layout.Children.Add(page1ButtonLayout);

        layout.Children.Add(new Label
        {
            Text = "Control Page2 Tab (via Binding):",
            Margin = new Thickness(0, 10, 0, 0)
        });

        var page2ButtonLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };

        var showPage2Button = new Button
        {
            Text = "Show TabBar",
            WidthRequest = 120,
            AutomationId = "ShowPage2TabBar"
        };
        showPage2Button.Clicked += (s, e) => _viewModel.TabBarIsVisible = true;
        page2ButtonLayout.Children.Add(showPage2Button);

        var hidePage2Button = new Button
        {
            Text = "Hide TabBar",
            WidthRequest = 120,
            AutomationId = "HidePage2TabBar"
        };
        hidePage2Button.Clicked += (s, e) => _viewModel.TabBarIsVisible = false;
        page2ButtonLayout.Children.Add(hidePage2Button);

        layout.Children.Add(page2ButtonLayout);

        Content = layout;
    }
}

public class Issue32994Page2 : ContentPage
{
    public Issue32994Page2()
    {
        Title = "Page2";

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Children =
                {
                    new Label
                    {
                        Text = "This is Page2",
                        FontAttributes = FontAttributes.Bold,
                        AutomationId = "Page2Label"
                    }
                }
        };
    }
}

public class Issue32994ViewModel : INotifyPropertyChanged
{
    bool _tabBarIsVisible = true;

    public bool TabBarIsVisible
    {
        get => _tabBarIsVisible;
        set
        {
            if (_tabBarIsVisible != value)
            {
                _tabBarIsVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
