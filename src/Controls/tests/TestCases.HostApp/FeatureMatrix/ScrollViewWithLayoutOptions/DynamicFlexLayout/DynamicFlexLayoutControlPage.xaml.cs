using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;

public partial class DynamicFlexLayoutControlPage : ContentPage
{
	private LayoutViewModel _viewModel;

	public DynamicFlexLayoutControlPage()
	{
		InitializeComponent();
		_viewModel = new LayoutViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		BuildDynamicFlex();
	}

	private void BuildDynamicFlex()
	{
		DynamicFlex.Children.Clear();

		if (_viewModel.FlexDirection == FlexDirection.Row)
		{
			DynamicFlex.Direction = FlexDirection.Row;
			DynamicFlex.BackgroundColor = Colors.Pink;
			DynamicFlex.HorizontalOptions = LayoutOptions.Fill;
			DynamicFlex.VerticalOptions = LayoutOptions.Start;
			MyScrollView.HorizontalOptions = LayoutOptions.Fill;
			MyScrollView.VerticalOptions = LayoutOptions.Start;
			MyScrollView.Orientation = ScrollOrientation.Vertical;
		}
		else
		{
			DynamicFlex.Direction = FlexDirection.Column;
			DynamicFlex.BackgroundColor = Colors.Yellow;
			DynamicFlex.HorizontalOptions = LayoutOptions.Start;
			DynamicFlex.VerticalOptions = LayoutOptions.Fill;
			MyScrollView.HorizontalOptions = LayoutOptions.Start;
			MyScrollView.VerticalOptions = LayoutOptions.Fill;
			MyScrollView.Orientation = ScrollOrientation.Horizontal;
		}

		for (int i = 1; i <= _viewModel.LabelCount; i++)
		{
			DynamicFlex.Children.Add(CreateLabel(i));
		}
	}

	private Label CreateLabel(int index)
	{
		return new Label
		{
			Text = $"Label {index}",
			FontSize = 18,
			Padding = new Thickness(10),
			BackgroundColor = Colors.LightGray,
			Margin = new Thickness(5)
		};
	}

	private void OnAddChildClicked(object sender, EventArgs e)
	{
		if (DynamicFlex == null)
			return;

		_viewModel.LabelCount++;
		DynamicFlex.Children.Add(CreateLabel(_viewModel.LabelCount));
	}

	private void OnRemoveChildClicked(object sender, EventArgs e)
	{
		if (DynamicFlex == null)
			return;

		if (DynamicFlex.Children.Count > 0)
		{
			DynamicFlex.Children.RemoveAt(DynamicFlex.Children.Count - 1);
			_viewModel.LabelCount--;
		}
	}

	private void OnFlexDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ColumnFlex.IsChecked)
			_viewModel.FlexDirection = FlexDirection.Column;
		else
			_viewModel.FlexDirection = FlexDirection.Row;

		BuildDynamicFlex();
	}
}