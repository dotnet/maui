namespace Maui.Controls.Sample;

public partial class TitleBarControlPage : ContentPage
{
    private TitleBarViewModel _viewModel;

    public TitleBarControlPage()
    {
        InitializeComponent();

        _viewModel = new TitleBarViewModel();
        BindingContext = _viewModel;

        // Subscribe to property changes to update the TitleBar
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Initial setup after window is created
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        // Setup TitleBar once the page is loaded and the Window is available
        SetupTitleBar();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ensure TitleBar is configured when page appears
        SetupTitleBar();
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update the TitleBar when relevant properties change
        SetupTitleBar();
    }

    private void SetupTitleBar()
    {
        var window = GetParentWindow();
        if (window == null)
            return;

        try
        {
            // Create a new TitleBar with the desired properties
            var titleBar = new Microsoft.Maui.Controls.TitleBar
            {
                Title = _viewModel.Title,
                Subtitle = _viewModel.Subtitle,
                BackgroundColor = _viewModel.Color,
                ForegroundColor = _viewModel.ForegroundColor,
                Icon = _viewModel.Icon,
                IsVisible = _viewModel.IsVisible,
                HeightRequest = 60
            };

            // Set content properties conditionally to avoid null reference issues
            if (_viewModel.TrailingContent != null)
                titleBar.TrailingContent = _viewModel.TrailingContent;

            if (_viewModel.LeadingContent != null)
                titleBar.LeadingContent = _viewModel.LeadingContent;

            if (_viewModel.TitleBarContent != null)
                titleBar.Content = _viewModel.TitleBarContent;

            titleBar.FlowDirection = _viewModel.FlowDirection;

            // Set up visual states for the TitleBar
            VisualStateManager.SetVisualStateGroups(titleBar, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    Name = "TitleActiveStates",
                    States =
                    {
                        new VisualState
                        {
                            Name = "TitleBarTitleActive",
                            Setters = { new Setter { Property = Microsoft.Maui.Controls.TitleBar.ForegroundColorProperty, Value = Colors.White } }
                        },
                        new VisualState
                        {
                            Name = "TitleBarTitleInactive",
                            Setters = { new Setter { Property = Microsoft.Maui.Controls.TitleBar.ForegroundColorProperty, Value = Colors.Black } }
                        }
                    }
                }
            });

            // Assign the new TitleBar to the window
            window.TitleBar = titleBar;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up TitleBar: {ex.Message}");
        }
    }

    private void OnFlowDirectionCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.FlowDirection = FlowDirection.RightToLeft;
        }
        else
        {
            _viewModel.FlowDirection = FlowDirection.LeftToRight;
        }
    }

    private void OnResetButtonClicked(object sender, EventArgs e)
    {
        // Unsubscribe from the old view model
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        // Create a new view model instance and update the field
        _viewModel = new TitleBarViewModel();
        this.BindingContext = _viewModel;

        // Subscribe to the new view model's PropertyChanged event
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Reset the FlowDirection checkbox to unchecked state
        FlowDirectionRTLCheckBox.IsChecked = false;

        // Re-setup the TitleBar after resetting values
        SetupTitleBar();
    }

    private void OnTitleBarContentChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is RadioButton radioButton && e.Value)
        {
            switch (radioButton.Value?.ToString())
            {
                case "SearchBar":
                    _viewModel.SetSearchBarContent();
                    break;

                case "HorizontalStackLayout":
                    _viewModel.SetHorizontalStackLayoutContent();
                    break;

                case "ProgressBar":
                    _viewModel.SetGridWithProgressBar();
                    break;
            }
        }
    }
}
