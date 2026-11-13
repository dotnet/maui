using System.Linq;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample
{
    public partial class ShellNavigationOptionsPage : ContentPage
    {
        readonly ShellViewModel _viewModel;
        readonly List<ContentPage> _insertedPages = new();
        int _insertedPageCount;
        public ShellNavigationOptionsPage(ShellViewModel viewModel, List<ContentPage> existingInsertedPages = null, int existingInsertedPageCount = 0)
        {
            _viewModel = viewModel;
            if (existingInsertedPages != null)
            {
                _insertedPages.AddRange(existingInsertedPages);
                _insertedPageCount = existingInsertedPageCount;
            }
            BindingContext = _viewModel;
            InitializeComponent();
            this.Appearing += OnPageAppearing;
        }
        void OnPageAppearing(object sender, System.EventArgs e)
        {
            UpdateStateLabels();
            Shell.Current?.Navigated += OnShellNavigated;
        }
        void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            if (Shell.Current?.CurrentPage == this)
                UpdateStateLabels();
        }
        void UpdateStateLabels()
        {
            var shell = Shell.Current;
            if (shell != null)
            {
                _viewModel.CurrentState = shell.CurrentState?.Location?.ToString() ?? "Not Set";
                _viewModel.CurrentPage = shell.CurrentPage?.Title ?? "Not Set";
                _viewModel.CurrentItem = shell.CurrentItem?.Title ?? "Not Set";
                _viewModel.ShellCurrent = shell.GetType().Name;
            }
            UpdateNavigationStackDisplay();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Shell.Current?.Navigated -= OnShellNavigated;
        }
        void UpdateNavigationStackDisplay()
        {
            var shell = Shell.Current;
            var section = shell?.CurrentItem?.CurrentItem;
            if (section != null)
            {
                // section.Stack[0] is null in Shell (root placeholder).
                // Resolve it to the root ContentPage title via ShellContent.Content.
                var rootTitle = (section.CurrentItem?.Content as Page)?.Title ?? "Root";
                var stack = section.Stack;
                TabStackLabel.Text = $"Count={stack.Count}: {string.Join(", ", stack.Select(p => p?.Title ?? rootTitle))}";
                var navStack = Navigation.NavigationStack;
                GetNavStackLabel.Text = $"Count={navStack.Count}: {string.Join(", ", navStack.Select(p => p?.Title ?? rootTitle))}";
            }
            else
            {
                TabStackLabel.Text = "N/A";
                GetNavStackLabel.Text = "N/A";
            }
        }
        async void OnPushClicked(object sender, System.EventArgs e)
        {
            var stack = Navigation.NavigationStack;
            int optionsIndex = -1;
            for (int i = 0; i < stack.Count; i++)
                if (ReferenceEquals(stack[i], this)) { optionsIndex = i; break; }
            var subPageNumber = optionsIndex >= 0 ? stack.Count - optionsIndex : 1;
            var pageTitle = $"SubPage{subPageNumber}";
            var subPage = new ContentPage
            {
                Title = pageTitle,
                AutomationId = $"OptionsSubPage{subPageNumber}",
            };
            var subTabStackLabel = new Label { FontSize = 11, AutomationId = $"SubPage{subPageNumber}TabStackLabel" };
            var subNavStackLabel = new Label { FontSize = 11, AutomationId = $"SubPage{subPageNumber}NavStackLabel" };
            subPage.Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 10,
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = $"Sub Page {subPageNumber}", FontSize = 14, FontAttributes = FontAttributes.Bold, AutomationId = $"OptionsSubPage{subPageNumber}IdentityLabel" },
                        CreateEventDisplay(),
                        CreateStackDisplay(subTabStackLabel, subNavStackLabel),
                        CreateSubPageButtons()
                    }
                }
            };
            subPage.Appearing += (s2, e2) => UpdateStackLabels(subPage, subTabStackLabel, subNavStackLabel);
            subPage.BindingContext = _viewModel;
            await Navigation.PushAsync(subPage);
            // Update the new SubPage's stack labels immediately after push so they
            // reflect the correct count before Appearing fires (fixes "not updated on push").
            UpdateStackLabels(subPage, subTabStackLabel, subNavStackLabel);
            UpdateNavigationStackDisplay();
        }
        async void OnPopClicked(object sender, System.EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
        async void OnPopToRootClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
        void OnInsertClicked(object sender, System.EventArgs e)
        {
            _insertedPageCount++;
            var pageTitle = $"InsertedPage{_insertedPageCount}";
            var goBackBtn = new Button
            {
                Text = "Go Back",
                FontSize = 12,
                HeightRequest = 35,
                AutomationId = $"InsertedPage{_insertedPageCount}GoBackButton"
            };
            goBackBtn.Clicked += async (s, ev) => await Shell.Current.GoToAsync("..");
            var goToOptionsBtn = new Button
            {
                Text = "Go to Options Page",
                FontSize = 12,
                HeightRequest = 35,
                AutomationId = $"InsertedPage{_insertedPageCount}GoToOptionsButton"
            };
            var insTabStackLabel = new Label { FontSize = 11, AutomationId = $"InsertedPage{_insertedPageCount}TabStackLabel" };
            var insNavStackLabel = new Label { FontSize = 11, AutomationId = $"InsertedPage{_insertedPageCount}NavStackLabel" };
            var insertedPage = new ContentPage
            {
                Title = pageTitle,
                AutomationId = $"InsertedPage{_insertedPageCount}",
                Content = new VerticalStackLayout
                {
                    Padding = 20,
                    Spacing = 10,
                    Children =
                    {
                        new Label { Text = $"Inserted Page {_insertedPageCount}", FontSize = 14, FontAttributes = FontAttributes.Bold, AutomationId = $"InsertedPage{_insertedPageCount}IdentityLabel" },
                        CreateStackDisplay(insTabStackLabel, insNavStackLabel),
                        goToOptionsBtn,
                        goBackBtn
                    }
                }
            };
            insertedPage.Appearing += (s2, e2) => UpdateStackLabels(insertedPage, insTabStackLabel, insNavStackLabel);
            goToOptionsBtn.Clicked += async (s, ev) => await insertedPage.Navigation.PushAsync(new ShellNavigationOptionsPage(_viewModel, _insertedPages, _insertedPageCount));
            _insertedPages.Add(insertedPage);
            Navigation.InsertPageBefore(insertedPage, this);
            UpdateNavigationStackDisplay();
        }
        void OnRemoveClicked(object sender, System.EventArgs e)
        {
            for (int i = _insertedPages.Count - 1; i >= 0; i--)
            {
                if (Navigation.NavigationStack.Contains(_insertedPages[i]))
                {
                    Navigation.RemovePage(_insertedPages[i]);
                    _insertedPages.RemoveAt(i);
                    break;
                }
            }
            if (_insertedPages.Count == 0)
                _insertedPageCount = 0;
            UpdateNavigationStackDisplay();
        }
        VerticalStackLayout CreateEventDisplay()
        {
            var layout = new VerticalStackLayout { Spacing = 3 };
            layout.Children.Add(new Label { Text = "NAVIGATING EVENT", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkBlue });
            AddBoundLabel(layout, "Current:", "NavigatingCurrent", "SubNavigatingCurrentLabel");
            AddBoundLabel(layout, "Source:", "NavigatingSource", "SubNavigatingSourceLabel");
            AddBoundLabel(layout, "Target:", "NavigatingTarget", "SubNavigatingTargetLabel");
            AddBoundLabel(layout, "CanCancel:", "NavigatingCanCancel", "SubNavigatingCanCancelLabel");
            AddBoundLabel(layout, "Cancelled:", "NavigatingCancelled", "SubNavigatingCancelledLabel");
            layout.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 4) });
            layout.Children.Add(new Label { Text = "NAVIGATED EVENT", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkGreen });
            AddBoundLabel(layout, "Current:", "NavigatedCurrent", "SubNavigatedCurrentLabel");
            AddBoundLabel(layout, "Previous:", "NavigatedPrevious", "SubNavigatedPreviousLabel");
            AddBoundLabel(layout, "Source:", "NavigatedSource", "SubNavigatedSourceLabel");
            return layout;
        }
        static void AddBoundLabel(VerticalStackLayout layout, string title, string bindingPath, string automationId)
        {
            var row = new HorizontalStackLayout { Spacing = 5 };
            row.Children.Add(new Label { Text = title, FontSize = 11, WidthRequest = 80 });
            var valueLabel = new Label { FontSize = 11, AutomationId = automationId };
            valueLabel.SetBinding(Label.TextProperty, bindingPath);
            row.Children.Add(valueLabel);
            layout.Children.Add(row);
        }
        Grid CreateSubPageButtons()
        {
            var pushBtn = new Button { Text = "Push Deeper", FontSize = 11, HeightRequest = 35, AutomationId = "SubPushDeeperButton" };
            pushBtn.Clicked += OnPushClicked;
            var popBtn = new Button { Text = "Pop (Go Back)", FontSize = 11, HeightRequest = 35, AutomationId = "SubPopButton" };
            popBtn.Clicked += async (s, e) => await Navigation.PopAsync();
            var popToRootBtn = new Button { Text = "PopToRoot", FontSize = 11, HeightRequest = 35, AutomationId = "SubPopToRootButton" };
            popToRootBtn.Clicked += async (s, e) => await Navigation.PopToRootAsync();
            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                RowSpacing = 4,
                ColumnSpacing = 4
            };
            Grid.SetRow(pushBtn, 0);
            Grid.SetColumn(pushBtn, 0);
            Grid.SetRow(popBtn, 0);
            Grid.SetColumn(popBtn, 1);
            Grid.SetRow(popToRootBtn, 1);
            Grid.SetColumn(popToRootBtn, 0);
            Grid.SetColumnSpan(popToRootBtn, 2);
            grid.Children.Add(pushBtn);
            grid.Children.Add(popBtn);
            grid.Children.Add(popToRootBtn);
            return grid;
        }
        static Grid CreateStackDisplay(Label tabStackLabel, Label navStackLabel)
        {
            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(new GridLength(100)), new ColumnDefinition(GridLength.Star) },
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                RowSpacing = 2,
                ColumnSpacing = 8
            };
            var tabLabel = new Label { Text = "TabStack:", FontSize = 12 };
            Grid.SetRow(tabLabel, 0);
            Grid.SetColumn(tabLabel, 0);
            Grid.SetRow(tabStackLabel, 0);
            Grid.SetColumn(tabStackLabel, 1);
            var navLabel = new Label { Text = "NavStack:", FontSize = 12 };
            Grid.SetRow(navLabel, 1);
            Grid.SetColumn(navLabel, 0);
            Grid.SetRow(navStackLabel, 1);
            Grid.SetColumn(navStackLabel, 1);
            grid.Children.Add(tabLabel);
            grid.Children.Add(tabStackLabel);
            grid.Children.Add(navLabel);
            grid.Children.Add(navStackLabel);
            return grid;
        }
        void UpdateStackLabels(Page page, Label tabStackLabel, Label navStackLabel)
        {
            var shell = Shell.Current;
            var section = shell?.CurrentItem?.CurrentItem;
            if (section != null)
            {
                var rootTitle = (section.CurrentItem?.Content as Page)?.Title ?? "Root";
                var stack = section.Stack;
                tabStackLabel.Text = $"Count={stack.Count}: {string.Join(", ", stack.Select(p => p?.Title ?? rootTitle))}";
                var navStack = page.Navigation.NavigationStack;
                navStackLabel.Text = $"Count={navStack.Count}: {string.Join(", ", navStack.Select(p => p?.Title ?? rootTitle))}";
            }
            else
            {
                tabStackLabel.Text = "N/A";
                navStackLabel.Text = "N/A";
            }
        }
    }
}
