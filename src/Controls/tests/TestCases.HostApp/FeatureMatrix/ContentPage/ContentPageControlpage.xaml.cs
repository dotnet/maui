namespace Maui.Controls.Sample;

public class ContentPageControlPage : NavigationPage
{
	private ContentPageViewModel _viewModel;

	public ContentPageControlPage()
	{
		_viewModel = new ContentPageViewModel();
		PushAsync(new ContentPageControlMainPage(_viewModel));
	}
}

public partial class ContentPageControlMainPage : ContentPage
{
	private ContentPageViewModel _viewModel;

	public ContentPageControlMainPage(ContentPageViewModel viewModel)
	{
		InitializeComponent();
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void TogglePadding_Clicked(object sender, EventArgs e)
	{
		// Toggle between different padding values
		_viewModel.Padding = _viewModel.Padding.Equals(new Thickness(10))
			? new Thickness(30, 20, 30, 20)
			: new Thickness(10);
	}

	private void SetIcon_Clicked(object sender, EventArgs e)
	{
		// Set an icon for the page (using a system icon or emoji)
		_viewModel.IconImageSource = ImageSource.FromFile("dotnet_bot.png"); // You can use any available icon
	}

	private void SetGradientBackground_Clicked(object sender, EventArgs e)
	{
		// Set a gradient background
		_viewModel.BackgroundImageSource = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1),
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.LightBlue, Offset = 0.0f },
				new GradientStop { Color = Colors.LightPink, Offset = 1.0f }
			}
		};
	}

	private void ChangeContent_Clicked(object sender, EventArgs e)
	{
		var currentContent = this.Content;
		this.Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
	{
	 new Label
	 {
	  Text = "New Content",
	  FontSize = 24,
	  FontAttributes = FontAttributes.Bold,
	  AutomationId = "NewContentLabel",
	  TextColor = Colors.DarkBlue,
	  HorizontalOptions = LayoutOptions.Center
	 },
	 new Label
	 {
	  Text = "This demonstrates a Content property changed in runtime",
	  FontSize = 16,
	  TextColor = Colors.Gray,
	  HorizontalOptions = LayoutOptions.Center,
	  Margin = new Thickness(20, 0)
	 },
	 new Button
	 {
	  Text = "ResetContent",
	  BackgroundColor = Colors.DodgerBlue,
	  TextColor = Colors.White,
	  FontAttributes = FontAttributes.Bold,
	  FontSize = 16,
	  AutomationId = "ResetContentButton",
	  Margin = new Thickness(0, 20, 0, 0),
	  Command = new Command(() => {      
       // Reset the original page's ViewModel properties
       _viewModel.ResetAllProperties();
	   this.Content = currentContent;
	  })
	 }
	}
		};
	}
	private void ResetAll_Clicked(object sender, EventArgs e)
	{
		// Reset all properties to their initial state
		_viewModel.ResetAllProperties();
	}

	private void ToggleFlowDirection_Clicked(object sender, EventArgs e)
	{
		// Toggle between LTR and RTL flow directions
		_viewModel.FlowDirection = _viewModel.FlowDirection == FlowDirection.LeftToRight
			? FlowDirection.RightToLeft
			: FlowDirection.LeftToRight;
	}
}