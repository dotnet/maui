
using System;
using System.Linq;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample
{
    public partial class ShellNavigationControlPage : Shell
    {
        readonly ShellViewModel _viewModel;
        string _previousPageTitle = "null";
        public ShellNavigationControlPage()
        {
            _viewModel = new ShellViewModel();
            BindingContext = _viewModel;
            InitializeComponent();
            Routing.RegisterRoute("detail1", typeof(DetailPage1));
            Routing.RegisterRoute("detail2", typeof(DetailPage2));
            Routing.RegisterRoute("detail1/subdetail", typeof(SubDetailPage));
            Routing.RegisterRoute("detail2/subdetail", typeof(SubDetailPage));

            // Register navigation test pages
            Routing.RegisterRoute("navtest1", typeof(NavigationTestPage1));
            Routing.RegisterRoute("navtest2", typeof(NavigationTestPage2));
            Routing.RegisterRoute("navtest3", typeof(NavigationTestPage3));
            this.Navigating += OnShellNavigating;
            this.Navigated += OnShellNavigated;

        }
        public ShellViewModel ViewModel => _viewModel;
        void UpdateCurrentState()
        {
            var shell = Shell.Current;
            if (shell != null)
            {
                _viewModel.CurrentState = shell.CurrentState?.Location?.ToString() ?? "Not Set";
                _viewModel.CurrentPage = shell.CurrentPage?.Title ?? "Not Set";
                _viewModel.CurrentItem = shell.CurrentItem?.Title ?? "Not Set";
                _viewModel.ShellCurrent = shell.GetType().Name;
            }
        }
        void UpdatePage2State()
        {
            UpdatePageLabels(Page2CurrentStateLabel, Page2CurrentPageLabel, Page2CurrentItemLabel, Page2ShellCurrentLabel, Page2ContentPage);

            // Update Tab.Stack info
            var shell = Shell.Current;
            var section = shell?.CurrentItem?.CurrentItem;
            if (section != null)
            {
                var stack = section.Stack;
                _viewModel.TabStackInfo = $"Count={stack.Count}: {string.Join(", ", stack.Select(p => p?.Title ?? "null"))}";
            }
        }
        void UpdatePage2A2State()
        {
            UpdatePageLabels(Page2A2CurrentStateLabel, Page2A2CurrentPageLabel, Page2A2CurrentItemLabel, Page2A2ShellCurrentLabel, Page2ContentA2Page);
        }
        void UpdatePage2TabBState()
        {
            UpdatePageLabels(Page2TabBCurrentStateLabel, Page2TabBCurrentPageLabel, Page2TabBCurrentItemLabel, Page2TabBShellCurrentLabel, Page2TabBPage);
        }
        void UpdatePageLabels(Label stateLabel, Label pageLabel, Label itemLabel, Label shellLabel, ContentPage page)
        {
            var shell = Shell.Current;
            if (shell != null)
            {
                stateLabel.Text = shell.CurrentState?.Location?.ToString() ?? "Not Set";
                pageLabel.Text = shell.CurrentPage?.Title ?? "Not Set";
                itemLabel.Text = shell.CurrentItem?.Title ?? "Not Set";
                shellLabel.Text = shell.GetType().Name;
                page.BindingContext = _viewModel;
            }
        }
        public void OnIconOverrideClicked(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Text == "None")
                    _viewModel.IconOverride = string.Empty;
                else
                    _viewModel.IconOverride = btn.Text;
            }
        }
        void OnToggleIsEnabled(object sender, EventArgs e)
        {
            _viewModel.IsEnabled = !_viewModel.IsEnabled;
        }
        void OnToggleIsVisible(object sender, EventArgs e)
        {
            _viewModel.IsVisible = !_viewModel.IsVisible;
        }
        async void OnNavigateToDetail1Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("detail1");
        }
        async void OnNavigateToDetail2Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("detail2");
        }
        async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ShellNavigationOptionsPage(_viewModel));
        }
        void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            _previousPageTitle = Shell.Current?.CurrentPage?.Title ?? "null";
            _viewModel.NavigatingCurrent = Shell.Current?.CurrentPage?.Title ?? "null";
            _viewModel.NavigatingSource = e.Source.ToString();
            _viewModel.NavigatingTarget = e.Target?.Location?.ToString() ?? "null";
            _viewModel.NavigatingCanCancel = e.CanCancel.ToString();
            _viewModel.NavigatingCancelled = e.Cancelled.ToString();

            if (_viewModel.CancelNavigation && e.CanCancel)
            {
                e.Cancel();
                _viewModel.NavigatingCancelled = e.Cancelled.ToString();
                _viewModel.DeferralStatus = "Navigation cancelled";
                return;
            }

            if (_viewModel.EnableDeferral && e.CanCancel)
            {
                var deferral = e.GetDeferral();
                _viewModel.DeferralStatus = "Deferring...";
                HandleDeferralAsync(deferral);
            }
        }

        async void HandleDeferralAsync(ShellNavigatingDeferral deferral)
        {
            try
            {
                await Task.Delay(2000);
                _viewModel.DeferralStatus = "Deferral completed";
                deferral.Complete();
            }
            catch (Exception ex)
            {
                _viewModel.DeferralStatus = $"Deferral error";
                System.Diagnostics.Debug.WriteLine($"Deferral failed: {ex.Message}");
            }
        }
        void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            _viewModel.NavigatedCurrent = Shell.Current?.CurrentPage?.Title ?? "null";
            _viewModel.NavigatedPrevious = _previousPageTitle;
            _viewModel.NavigatedSource = e.Source.ToString();
            UpdateCurrentState();
            UpdatePage2State();
            UpdatePage2A2State();
            UpdatePage2TabBState();
        }
        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            _viewModel.OverrideNavigatingStatus = $"Source={args.Source}, Target={args.Target?.Location}";
        }
        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            _viewModel.OverrideNavigatedStatus = $"Source={args.Source}, Previous={args.Previous?.Location}";
        }
        bool _routeRegistered = true;
        async void OnToggleRouteClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (_routeRegistered)
            {
                Routing.UnRegisterRoute("detail2");
                try
                { await Shell.Current.GoToAsync("detail2"); _viewModel.RouteStatus = "Still works"; }
                catch (Exception ex) { _viewModel.RouteStatus = "Unregistered"; System.Diagnostics.Debug.WriteLine($"Expected: {ex.Message}"); }
                btn.Text = "Register Route";
                _routeRegistered = false;
            }
            else
            {
                Routing.RegisterRoute("detail2", typeof(DetailPage2));
                try
                { await Shell.Current.GoToAsync("detail2"); _viewModel.RouteStatus = "Registered"; }
                catch (Exception ex) { _viewModel.RouteStatus = $"{ex.Message}"; }
                btn.Text = "Unregister Route";
                _routeRegistered = true;
            }
        }
        void OnResetClicked(object sender, EventArgs e)
        {
            _viewModel.TextOverride = string.Empty;
            _viewModel.IconOverride = string.Empty;
            _viewModel.IsEnabled = true;
            _viewModel.IsVisible = true;
            _viewModel.CommandParameter = string.Empty;
            _viewModel.CommandExecuted = string.Empty;
            _viewModel.CurrentState = "Not Set";
            _viewModel.CurrentPage = "Not Set";
            _viewModel.CurrentItem = "Not Set";
            _viewModel.ShellCurrent = "Not Set";
            _viewModel.NavigatingCurrent = string.Empty;
            _viewModel.NavigatingSource = string.Empty;
            _viewModel.NavigatingTarget = string.Empty;
            _viewModel.NavigatingCanCancel = string.Empty;
            _viewModel.NavigatingCancelled = string.Empty;
            _viewModel.NavigatedCurrent = string.Empty;
            _viewModel.NavigatedPrevious = string.Empty;
            _viewModel.NavigatedSource = string.Empty;
            _viewModel.RouteStatus = string.Empty;
            _viewModel.CancelNavigation = false;
            _viewModel.EnableDeferral = false;
            _viewModel.DeferralStatus = string.Empty;
            _viewModel.OverrideNavigatingStatus = string.Empty;
            _viewModel.OverrideNavigatedStatus = string.Empty;
            _viewModel.TabStackInfo = string.Empty;
        }
        void OnToggleCancelNavigation(object sender, EventArgs e)
        {
            _viewModel.CancelNavigation = !_viewModel.CancelNavigation;
        }
        void OnToggleEnableDeferral(object sender, EventArgs e)
        {
            _viewModel.EnableDeferral = !_viewModel.EnableDeferral;
        }
    }
    public class ShellDetailBasePage : ContentPage
    {
        readonly string _prefix;
        Label _currentStateLabel;
        Label _currentPageLabel;
        Label _currentItemLabel;
        Label _shellCurrentLabel;
        Label _commandExecutedLabel;
        public ShellDetailBasePage(string title, string prefix)
        {
            Title = title;
            AutomationId = $"{prefix}Page";
            _prefix = prefix;
            var behavior = new BackButtonBehavior();
            behavior.SetBinding(BackButtonBehavior.TextOverrideProperty, "TextOverride");
            behavior.SetBinding(BackButtonBehavior.IconOverrideProperty, "IconOverride");
            behavior.SetBinding(BackButtonBehavior.IsEnabledProperty, "IsEnabled");
            behavior.SetBinding(BackButtonBehavior.IsVisibleProperty, "IsVisible");
            behavior.SetBinding(BackButtonBehavior.CommandProperty, "Command");
            behavior.SetBinding(BackButtonBehavior.CommandParameterProperty, "CommandParameter");
            Shell.SetBackButtonBehavior(this, behavior);
            BuildUI();
            this.Appearing += OnPageAppearing;
        }
        void BuildUI()
        {
            _currentStateLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentStateLabel" };
            _currentPageLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentPageLabel" };
            _currentItemLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentItemLabel" };
            _shellCurrentLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}ShellCurrentLabel" };
            _commandExecutedLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CommandExecutedLabel" };
            var goBackButton = ShellNavHelper.CreateNavButton("Go Back", "..", $"{_prefix}GoBackButton");
            var contextualNavButton = ShellNavHelper.CreateNavButton("Navigate SubDetail", "subdetail", $"{_prefix}ContextualNavButton");
            var grid = new Grid
            {
                Padding = 10,
                RowSpacing = 4,
                ColumnSpacing = 10,
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
            };
            AddRow(grid, 0, "CurrentState:", _currentStateLabel);
            AddRow(grid, 1, "CurrentPage:", _currentPageLabel);
            AddRow(grid, 2, "CurrentItem:", _currentItemLabel);
            AddRow(grid, 3, "Shell.Current:", _shellCurrentLabel);
            AddRow(grid, 4, "CommandExecuted:", _commandExecutedLabel);
            Grid.SetRow(goBackButton, 5);
            Grid.SetColumn(goBackButton, 0);
            Grid.SetColumnSpan(goBackButton, 2);
            grid.Children.Add(goBackButton);
            Grid.SetRow(contextualNavButton, 6);
            Grid.SetColumn(contextualNavButton, 0);
            Grid.SetColumnSpan(contextualNavButton, 2);
            grid.Children.Add(contextualNavButton);
            Content = new ScrollView { Content = grid };
        }
        static void AddRow(Grid grid, int row, string labelText, Label valueLabel)
        {
            var label = new Label { Text = labelText, FontSize = 12 };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            Grid.SetRow(valueLabel, row);
            Grid.SetColumn(valueLabel, 1);
            grid.Children.Add(label);
            grid.Children.Add(valueLabel);
        }
        void OnPageAppearing(object sender, EventArgs e)
        {
            UpdateState();
            if (Shell.Current != null)
                Shell.Current.Navigated += OnShellNavigatedUpdateState;
        }

        void OnShellNavigatedUpdateState(object sender, ShellNavigatedEventArgs e)
        {
            if (Shell.Current?.CurrentPage == this)
                UpdateState();
        }

        void UpdateState()
        {
            var shell = Shell.Current;
            if (shell != null)
            {
                _currentStateLabel.Text = shell.CurrentState?.Location?.ToString() ?? "Not Set";
                _currentPageLabel.Text = shell.CurrentPage?.Title ?? "Not Set";
                _currentItemLabel.Text = shell.CurrentItem?.Title ?? "Not Set";
                _shellCurrentLabel.Text = shell.GetType().Name;
                if (shell is ShellNavigationControlPage controlPage)
                {
                    var vm = controlPage.ViewModel;
                    _commandExecutedLabel.Text = vm.CommandExecuted;
                    BindingContext = vm;
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Shell.Current != null)
                Shell.Current.Navigated -= OnShellNavigatedUpdateState;
        }
    }
    public class DetailPage1 : ShellDetailBasePage
    {
        public DetailPage1() : base("DetailPage1", "Detail1")
        {
            var grid = (Content as ScrollView)?.Content as Grid;
            if (grid == null)
                return;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var absBtn = ShellNavHelper.CreateNavButton("Absolute to Page2", "//page2", "Detail1AbsoluteButton");
            Grid.SetRow(absBtn, 7);
            Grid.SetColumnSpan(absBtn, 2);
            grid.Children.Add(absBtn);
            var relBtn = ShellNavHelper.CreateNavButton("Relative to NavTest1", "navtest1", "Detail1RelativeButton");
            Grid.SetRow(relBtn, 8);
            Grid.SetColumnSpan(relBtn, 2);
            grid.Children.Add(relBtn);
        }
    }

    public class DetailPage2 : ShellDetailBasePage
    {
        public DetailPage2() : base("DetailPage2", "Detail2")
        {
            var grid = (Content as ScrollView)?.Content as Grid;
            if (grid == null)
                return;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var absToPage2Btn = ShellNavHelper.CreateNavButton("Absolute to Page2", "//page2", "Detail2AbsoluteButton");
            Grid.SetRow(absToPage2Btn, 7);
            Grid.SetColumnSpan(absToPage2Btn, 2);
            grid.Children.Add(absToPage2Btn);
            var backBtn = ShellNavHelper.CreateNavButton("Go Back", "..", "Detail2BackButton");
            Grid.SetRow(backBtn, 8);
            Grid.SetColumnSpan(backBtn, 2);
            grid.Children.Add(backBtn);
        }
    }
    public class SubDetailPage : ContentPage
    {
        public SubDetailPage()
        {
            Title = "SubDetailPage";
            AutomationId = "SubDetailPage";
            var currentRouteLabel = new Label { FontSize = 12, AutomationId = "SubDetailCurrentRouteLabel" };
            var sourceContextLabel = new Label { FontSize = 12, AutomationId = "SubDetailSourceContextLabel" };
            var goBackButton = ShellNavHelper.CreateNavButton("Go Back", "..", "SubDetailGoBackButton");
            this.Appearing += (s, e) =>
            {
                UpdateLabels(currentRouteLabel, sourceContextLabel);
                if (Shell.Current != null)
                    Shell.Current.Navigated += OnNavigated;
            };
            this.Disappearing += (s, e) =>
            {
                if (Shell.Current != null)
                    Shell.Current.Navigated -= OnNavigated;
            };

            void OnNavigated(object sender, ShellNavigatedEventArgs e)
            {
                if (Shell.Current?.CurrentPage == this)
                    UpdateLabels(currentRouteLabel, sourceContextLabel);
            }

            void UpdateLabels(Label routeLabel, Label contextLabel)
            {
                var shell = Shell.Current;
                var location = shell?.CurrentState?.Location?.ToString() ?? "unknown";
                routeLabel.Text = location;
                contextLabel.Text = location.Contains("detail1", StringComparison.Ordinal) ? "Contextual from: detail1"
                    : location.Contains("detail2", StringComparison.Ordinal) ? "Contextual from: detail2"
                    : "Unknown context";
            }
            Content = new ScrollView
            {
                Content = new Grid
                {
                    Padding = 10,
                    RowSpacing = 4,
                    ColumnSpacing = 10,
                    RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                    Children =
                    {
                        CreateLabel("Current Route:", 0, 0),
                        SetGrid(currentRouteLabel, 0, 1),
                        CreateLabel("Source Context:", 1, 0),
                        SetGrid(sourceContextLabel, 1, 1),
                        SetGrid(goBackButton, 2, 0, 2)
                    }
                }
            };
        }
        static Label CreateLabel(string text, int row, int col, int colSpan = 1, bool bold = false)
        {
            var label = new Label { Text = text, FontSize = 12 };
            if (bold)
                label.FontAttributes = FontAttributes.Bold;
            Grid.SetRow(label, row);
            Grid.SetColumn(label, col);
            if (colSpan > 1)
                Grid.SetColumnSpan(label, colSpan);
            return label;
        }
        static View SetGrid(View view, int row, int col, int colSpan = 1)
        {
            Grid.SetRow(view, row);
            Grid.SetColumn(view, col);
            if (colSpan > 1)
                Grid.SetColumnSpan(view, colSpan);
            return view;
        }
    }
    public class NavigationTestPage1 : ContentPage
    {
        public NavigationTestPage1()
        {
            Title = "NavTest1";
            AutomationId = "NavigationTestPage1";
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 10,
                    Spacing = 10,
                    Children =
                    {
                        ShellNavHelper.CreateNavButton("Go Back", "..", "NavTest1BackButton"),
                        ShellNavHelper.CreateNavButton("Navigate to NavTest2", "navtest2", "NavTest1ToNavTest2Button")
                    }
                }
            };
        }
    }
    public class NavigationTestPage2 : ContentPage
    {
        public NavigationTestPage2()
        {
            Title = "NavTest2";
            AutomationId = "NavigationTestPage2";
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 10,
                    Spacing = 10,
                    Children =
                    {
                        ShellNavHelper.CreateNavButton("Back and Forward", "../navtest3", "NavTest2BackForwardButton"),
                        ShellNavHelper.CreateNavButton("Simple Back", "..", "NavTest2BackButton")
                    }
                }
            };
        }
    }
    public class NavigationTestPage3 : ContentPage
    {
        public NavigationTestPage3()
        {
            Title = "NavTest3";
            AutomationId = "NavigationTestPage3";
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 10,
                    Spacing = 10,
                    Children =
                    {
                        ShellNavHelper.CreateNavButton("Back 2 Levels", "../..", "NavTest3MultiBackButton")
                    }
                }
            };
        }
    }
    static class ShellNavHelper
    {
        public static Button CreateNavButton(string text, string route, string automationId)
        {
            var btn = new Button { Text = text, FontSize = 12, HeightRequest = 35, AutomationId = automationId };
            btn.Clicked += async (s, e) =>
            {
                try
                { await Shell.Current.GoToAsync(route); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}"); }
            };
            return btn;
        }
    }
}
