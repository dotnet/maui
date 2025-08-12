using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
    public class ContentViewControlPage : ContentPage
    {
        private ContentViewFirstCustomPage _firstCustomView;
        private ContentViewSecondCustomPage _secondCustomView;
        RadioButton _firstPageRadioButton;
        RadioButton _secondPageRadioButton;
        RadioButton _controlTemplateYesRadioButton;
        RadioButton _controlTemplateNoRadioButton;
        View _currentCustomView;
        ContentView _dynamicContentHost;

        public ContentViewControlPage()
        {
            _firstCustomView = new ContentViewFirstCustomPage
            {
                CardTitle = "ContenView",
                CardDescription = "Use ContentViewPage as the content, binding all card properties to the ViewModel",
                IconImageSource = "dotnet_bot.png",
                IconBackgroundColor = Colors.LightGray,
                BorderColor = Colors.Pink,
                CardColor = Colors.SkyBlue
            };

            _secondCustomView = new ContentViewSecondCustomPage
            {
                SecondCustomViewTitle = "Second Custom Title",
                SecondCustomViewDescription = "This is the description for the second custom view.",
                SecondCustomViewText = "This is the SECOND custom view",
                FrameBackgroundColor = Colors.LightGray
            };

            _firstPageRadioButton = new RadioButton
            {
                Content = "First Page",
                GroupName = "PageType",
                IsChecked = true
            };
            _secondPageRadioButton = new RadioButton
            {
                Content = "Second Page",
                GroupName = "PageType"
            };
            _firstPageRadioButton.CheckedChanged += OnCustomPageRadioCheckedChanged;
            _secondPageRadioButton.CheckedChanged += OnCustomPageRadioCheckedChanged;

            _controlTemplateYesRadioButton = new RadioButton
            {
                Content = "Yes",
                GroupName = "TemplateType"
            };
            _controlTemplateNoRadioButton = new RadioButton
            {
                Content = "No",
                GroupName = "TemplateType",
                IsChecked = true
            };
            _controlTemplateYesRadioButton.CheckedChanged += OnControlTemplateRadioCheckedChanged;
            _controlTemplateNoRadioButton.CheckedChanged += OnControlTemplateRadioCheckedChanged;

            _dynamicContentHost = new ContentView();

            var boxSpacer = new BoxView { HeightRequest = 150, Opacity = 0 };
            var customPagesLabel = new Label { Text = "Custom Pages", FontAttributes = FontAttributes.Bold, FontSize = 16 };
            var customPagesRadioLayout = new HorizontalStackLayout
            {
                Children = { _firstPageRadioButton, _secondPageRadioButton }
            };
            var controlTemplateLabel = new Label { Text = "Control Template", FontAttributes = FontAttributes.Bold, FontSize = 16 };
            var controlTemplateRadioLayout = new HorizontalStackLayout
            {
                Children = { _controlTemplateYesRadioButton, _controlTemplateNoRadioButton }
            };

            var mainLayout = new VerticalStackLayout
            {
                Spacing = 2,
                Padding = new Thickness(5),
                Children =
                {
                    _dynamicContentHost,
                    boxSpacer,
                    customPagesLabel,
                    customPagesRadioLayout,
                    controlTemplateLabel,
                    controlTemplateRadioLayout
                }
            };

            Content = mainLayout;

            _firstPageRadioButton.IsChecked = true;
            _currentCustomView = _firstCustomView;
            UpdateContentViews();
        }

        private void OnCustomPageRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_firstPageRadioButton.IsChecked)
            {
                _currentCustomView = _firstCustomView;
            }
            else if (_secondPageRadioButton.IsChecked)
            {
                _currentCustomView = _secondCustomView;
            }
            UpdateContentViews();
        }

        private void OnControlTemplateRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateContentViews();
        }

        private void UpdateContentViews()
        {
            var titleLabel = new Label
            {
                Text = _currentCustomView is ContentViewFirstCustomPage
                    ? "First Custom View"
                    : "Second Custom View",
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center
            };

            var contentCopy = CreateNewContentView(_currentCustomView);

            if (contentCopy is ContentViewFirstCustomPage firstCopy)
            {
                firstCopy.ControlTemplate = _controlTemplateYesRadioButton.IsChecked
                    ? (ControlTemplate)firstCopy.Resources["AlternateCardTemplate"]
                    : (ControlTemplate)firstCopy.Resources["DefaultFirstTemplate"];
            }
            else if (contentCopy is ContentViewSecondCustomPage secondCopy)
            {
                secondCopy.ControlTemplate = _controlTemplateYesRadioButton.IsChecked
                    ? (ControlTemplate)secondCopy.Resources["AlternateSecondTemplate"]
                    : (ControlTemplate)secondCopy.Resources["DefaultSecondTemplate"];
            }

            var stack = new VerticalStackLayout();
            stack.Children.Add(titleLabel);
            stack.Children.Add(contentCopy);

            _dynamicContentHost.Content = stack;
        }

        private View CreateNewContentView(View view)
        {
            if (view is ContentViewFirstCustomPage)
                return new ContentViewFirstCustomPage
                {
                    CardTitle = _firstCustomView.CardTitle,
                    CardDescription = _firstCustomView.CardDescription,
                    IconImageSource = _firstCustomView.IconImageSource,
                    IconBackgroundColor = _firstCustomView.IconBackgroundColor,
                    BorderColor = _firstCustomView.BorderColor,
                    CardColor = _firstCustomView.CardColor
                };

            if (view is ContentViewSecondCustomPage)
                return new ContentViewSecondCustomPage
                {
                    SecondCustomViewTitle = _secondCustomView.SecondCustomViewTitle,
                    SecondCustomViewDescription = _secondCustomView.SecondCustomViewDescription,
                    SecondCustomViewText = _secondCustomView.SecondCustomViewText,
                    FrameBackgroundColor = _secondCustomView.FrameBackgroundColor
                };

            return view;
        }
    }
}