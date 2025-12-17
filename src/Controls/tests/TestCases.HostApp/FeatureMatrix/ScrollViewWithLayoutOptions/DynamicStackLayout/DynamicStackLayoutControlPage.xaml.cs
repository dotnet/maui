namespace Maui.Controls.Sample;

public partial class DynamicStackLayoutControlPage : ContentPage
{
	private LayoutViewModel _viewModel;
	public DynamicStackLayoutControlPage()
	{
		InitializeComponent();
		_viewModel = new LayoutViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		BuildDynamicStack();
	}

	private void BuildDynamicStack()
	{
		DynamicStack.Children.Clear();

		if (_viewModel.Orientation == ScrollOrientation.Horizontal)
		{
			DynamicStack.Orientation = StackOrientation.Horizontal;
			DynamicStack.BackgroundColor = Colors.Pink;
			DynamicStack.HorizontalOptions = LayoutOptions.Start;
			DynamicStack.VerticalOptions = LayoutOptions.Fill;
			MyScrollView.HorizontalOptions = LayoutOptions.Start;
			MyScrollView.VerticalOptions = LayoutOptions.Fill;
		}
		else
		{
			DynamicStack.Orientation = StackOrientation.Vertical;
			DynamicStack.BackgroundColor = Colors.Yellow;
			DynamicStack.HorizontalOptions = LayoutOptions.Fill;
			DynamicStack.VerticalOptions = LayoutOptions.Start;
			MyScrollView.HorizontalOptions = LayoutOptions.Fill;
			MyScrollView.VerticalOptions = LayoutOptions.Start;
		}

		for (int i = 1; i <= _viewModel.LabelCount; i++)
		{
			DynamicStack.Children.Add(CreateLabel(i));
		}

	}

	private Label CreateLabel(int index)
	{
		return new Label
		{
			Text = $"Label {index}",
			FontSize = 18,
			Padding = new Thickness(10)
		};
	}
	private void OnAddChildClicked(object sender, EventArgs e)
	{
		if (DynamicStack == null)
			return;
		_viewModel.LabelCount++;
		DynamicStack.Children.Add(CreateLabel(_viewModel.LabelCount));
	}

	private void OnRemoveChildClicked(object sender, EventArgs e)
	{
		if (DynamicStack == null)
			return;

		if (DynamicStack.Children.Count > 0)
		{
			DynamicStack.Children.RemoveAt(DynamicStack.Children.Count - 1);
			_viewModel.LabelCount--;
		}
	}

	private void OnOrientationClicked(object sender, CheckedChangedEventArgs e)
	{
		if (VerticalStack.IsChecked)
		{
			_viewModel.Orientation = ScrollOrientation.Vertical;
		}
		else
		{
			_viewModel.Orientation = ScrollOrientation.Horizontal;
		}

		BuildDynamicStack();
	}
}