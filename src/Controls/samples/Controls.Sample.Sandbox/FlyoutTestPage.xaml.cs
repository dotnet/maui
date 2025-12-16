using Microsoft.Maui.Controls;
#pragma warning disable IDE0031 // Use null propagation
namespace Maui.Controls.Sample
{
    public partial class FlyoutTestPage : ContentPage
    {
        public FlyoutTestPage()
        {
            InitializeComponent();
            UpdateFlyoutState();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Shell.Current is not null)
            {
                Shell.Current.PropertyChanged += OnShellPropertyChanged;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Shell.Current is not null)
            {
                Shell.Current.PropertyChanged -= OnShellPropertyChanged;
            }
        }

        private void OnShellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shell.FlyoutIsPresented))
            {
                UpdateFlyoutState();
            }
        }

        private void UpdateFlyoutState()
        {
            if (Shell.Current != null)
            {
                FlyoutStateLabel.Text = $"Flyout State: {(Shell.Current.FlyoutIsPresented ? "Open" : "Closed")}";
            }
        }

        private void OnOpenFlyoutClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
            StatusLabel.Text = "Status: Flyout opened";
        }

        private void OnCloseFlyoutClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;
            StatusLabel.Text = "Status: Flyout closed";
        }

        private void OnToggleFlyoutClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
            StatusLabel.Text = $"Status: Flyout toggled to {(Shell.Current.FlyoutIsPresented ? "Open" : "Closed")}";
        }

        private void OnSetBehaviorFlyout(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
            StatusLabel.Text = "Status: FlyoutBehavior = Flyout (swipeable)";
        }

        private void OnSetBehaviorLocked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Locked;
            StatusLabel.Text = "Status: FlyoutBehavior = Locked (always visible)";
        }

        private void OnSetBehaviorDisabled(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
            StatusLabel.Text = "Status: FlyoutBehavior = Disabled (hidden)";
        }

        private void OnChangeFlyoutBackgroundRed(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackgroundColor = Colors.Red;
            StatusLabel.Text = "Status: Flyout background = Red";
        }

        private void OnChangeFlyoutBackgroundBlue(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackgroundColor = Colors.Blue;
            StatusLabel.Text = "Status: Flyout background = Blue";
        }

        private void OnResetFlyoutBackground(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackgroundColor = null;
            StatusLabel.Text = "Status: Flyout background reset to default";
        }

        private void OnSetWidth200(object sender, EventArgs e)
        {
            Shell.Current.FlyoutWidth = 200;
            StatusLabel.Text = "Status: Flyout width = 200";
        }

        private void OnSetWidth300(object sender, EventArgs e)
        {
            Shell.Current.FlyoutWidth = 300;
            StatusLabel.Text = "Status: Flyout width = 300";
        }

        private void OnResetWidth(object sender, EventArgs e)
        {
            Shell.Current.FlyoutWidth = -1; // Default
            StatusLabel.Text = "Status: Flyout width reset to default";
        }

        // Header/Footer Tests
        private void OnAddHeaderClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeader = new Label
            {
                Text = "Flyout Header",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                BackgroundColor = Colors.Blue,
                Padding = new Thickness(10),
                HorizontalOptions = LayoutOptions.Fill
            };
            StatusLabel.Text = "Status: Header added (Label)";
        }

        private void OnChangeHeaderClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeader = new Button
            {
                Text = "Header Button",
                BackgroundColor = Colors.Green,
                TextColor = Colors.White,
                Margin = new Thickness(10)
            };
            StatusLabel.Text = "Status: Header changed (Button)";
        }

        private void OnRemoveHeaderClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeader = null;
            StatusLabel.Text = "Status: Header removed";
        }

        private void OnAddFooterClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutFooter = new Label
            {
                Text = "Flyout Footer - Version 1.0",
                FontSize = 12,
                TextColor = Colors.Gray,
                BackgroundColor = Colors.LightGray,
                Padding = new Thickness(10),
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center
            };
            StatusLabel.Text = "Status: Footer added";
        }

        private void OnRemoveFooterClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutFooter = null;
            StatusLabel.Text = "Status: Footer removed";
        }

        // Header Behavior Tests
        private void OnHeaderBehaviorDefault(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeaderBehavior = FlyoutHeaderBehavior.Default;
            StatusLabel.Text = "Status: Header behavior = Default";
        }

        private void OnHeaderBehaviorScroll(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeaderBehavior = FlyoutHeaderBehavior.Scroll;
            StatusLabel.Text = "Status: Header behavior = Scroll";
        }

        private void OnHeaderBehaviorCollapse(object sender, EventArgs e)
        {
            Shell.Current.FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;
            StatusLabel.Text = "Status: Header behavior = CollapseOnScroll";
        }

        // Flyout Content Tests
        private void OnSetCustomContentClicked(object sender, EventArgs e)
        {
            var customContent = new CollectionView
            {
                ItemsSource = new[] { "Custom Item 1", "Custom Item 2", "Custom Item 3" },
                BackgroundColor = Colors.LightYellow
            };
            Shell.Current.FlyoutContent = customContent;
            StatusLabel.Text = "Status: Custom content set (ListView)";
        }

        private void OnResetContentClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutContent = null;
            StatusLabel.Text = "Status: Content reset to default";
        }

        // Flyout Backdrop Tests
        private void OnRedBackdropClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackdrop = new SolidColorBrush(Colors.Red);
            StatusLabel.Text = "Status: Backdrop = Red";
        }

        private void OnBlackBackdropClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackdrop = new SolidColorBrush(Colors.Black.WithAlpha(0.5f));
            StatusLabel.Text = "Status: Backdrop = Black semi-transparent";
        }

        private void OnRemoveBackdropClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutBackdrop = Brush.Transparent;
            StatusLabel.Text = "Status: Backdrop removed (transparent)";
        }

        // Flow Direction Tests
        private void OnFlowDirectionLTRClicked(object sender, EventArgs e)
        {
            Shell.Current.FlowDirection = FlowDirection.LeftToRight;
            StatusLabel.Text = "Status: Flow direction = LeftToRight";
        }

        private void OnFlowDirectionRTLClicked(object sender, EventArgs e)
        {
            Shell.Current.FlowDirection = FlowDirection.RightToLeft;
            StatusLabel.Text = "Status: Flow direction = RightToLeft";
        }
    }
}
