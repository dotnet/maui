namespace Maui.Controls.Sample;

public partial class ContentViewControlPage : NavigationPage
{
	public ContentViewControlPage()
	{
		PushAsync(new ContentViewControlMainPage());
	}
}

public partial class ContentViewControlMainPage : ContentPage
{
	private readonly ContentViewFirstCustomPage _firstCustomView;
	private readonly ContentViewSecondCustomPage _secondCustomView;
	private readonly RadioButton _firstPageRadioButton;
	private readonly RadioButton _secondPageRadioButton;
	private readonly RadioButton _controlTemplateYesRadioButton;
	private readonly RadioButton _controlTemplateNoRadioButton;
	private View _currentCustomView;

	public ContentViewControlMainPage()
	{
		InitializeComponent();

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
			FrameBackgroundColor = Colors.LightGray
		};


		_firstPageRadioButton = new RadioButton
		{
			Content = "First Page",
			GroupName = "PageType",
			IsChecked = true,
		};
		_secondPageRadioButton = new RadioButton
		{
			Content = "Second Page",
			GroupName = "PageType"
		};
		_firstPageRadioButton.CheckedChanged += OnCustomPageRadioCheckedChanged;
		_secondPageRadioButton.CheckedChanged += OnCustomPageRadioCheckedChanged;

		var customPageRadioLayout = new HorizontalStackLayout
		{
			Spacing = 20,
			Children = { _firstPageRadioButton, _secondPageRadioButton }
		};

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

		var controlTemplateRadioLayout = new HorizontalStackLayout
		{
			Children = { _controlTemplateYesRadioButton, _controlTemplateNoRadioButton }
		};

		// Main layout as per requested order
		var mainLayout = new VerticalStackLayout
		{
			Spacing = 2,
			Padding = new Thickness(5),
			Children =
			{
				new ContentView { Content = DynamicContentHost },
				new BoxView { HeightRequest = 150, Opacity = 0 },
				new Label { Text = "Custom Pages", FontAttributes = FontAttributes.Bold, FontSize = 16 },
				customPageRadioLayout,
				new Label { Text = "Control Template", FontAttributes = FontAttributes.Bold, FontSize = 16 },
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
		   // Set ControlTemplate to default or alternate template as appropriate
		   if (_currentCustomView is ContentViewFirstCustomPage first)
		   {
			   first.ControlTemplate = _controlTemplateYesRadioButton.IsChecked
				   ? (ControlTemplate)first.Resources["AlternateCardTemplate"]
				   : (ControlTemplate)first.Resources["DefaultFirstTemplate"];
		   }
		   else if (_currentCustomView is ContentViewSecondCustomPage second)
		   {
			   second.ControlTemplate = _controlTemplateYesRadioButton.IsChecked
				   ? (ControlTemplate)second.Resources["AlternateSecondTemplate"]
				   : (ControlTemplate)second.Resources["DefaultSecondTemplate"];
		   }

		   var stack = new VerticalStackLayout();
		   stack.Children.Add(_currentCustomView);
		   DynamicContentHost.Content = stack;
	   }
}

