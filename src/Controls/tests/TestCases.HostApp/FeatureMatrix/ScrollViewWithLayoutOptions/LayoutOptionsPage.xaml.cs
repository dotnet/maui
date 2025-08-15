namespace Maui.Controls.Sample;

public partial class LayoutOptionsPage : ContentPage
{
	private readonly LayoutViewModel _viewModel;

	public LayoutOptionsPage(LayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}


	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void OnHorizontalOptionClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "Fill":
				_viewModel.HorizontalOptions = LayoutOptions.Fill;
				break;
			case "Start":
				_viewModel.HorizontalOptions = LayoutOptions.Start;
				break;
			case "Center":
				_viewModel.HorizontalOptions = LayoutOptions.Center;
				break;
			case "End":
				_viewModel.HorizontalOptions = LayoutOptions.End;
				break;
		}
	}

	private void OnVerticalOptionClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "Fill":
				_viewModel.VerticalOptions = LayoutOptions.Fill;
				break;
			case "Start":
				_viewModel.VerticalOptions = LayoutOptions.Start;
				break;
			case "Center":
				_viewModel.VerticalOptions = LayoutOptions.Center;
				break;
			case "End":
				_viewModel.VerticalOptions = LayoutOptions.End;
				break;
		}
	}

	private void OnWidthRequestClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "No Width":
				_viewModel.WidthRequest = -1;
				break;
			case "350":
				_viewModel.WidthRequest = 350;
				break;
		}
	}

	private void OnHeightRequestClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "No Height":
				_viewModel.HeightRequest = -1;
				break;
			case "300":
				_viewModel.HeightRequest = 300;
				break;
		}
	}

	private void OnOrientationClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "Vertical":
				_viewModel.Orientation = ScrollOrientation.Vertical;
				break;
			case "Horizontal":
				_viewModel.Orientation = ScrollOrientation.Horizontal;
				break;
		}
	}
}