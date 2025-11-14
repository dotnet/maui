using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Maui.Controls.Sample;

public partial class BindableLayoutControlPage : NavigationPage
{
	private BindableLayoutViewModel _viewModel;
	public BindableLayoutControlPage()
	{
		_viewModel = new BindableLayoutViewModel();
		PushAsync(new BindableLayoutControlMainPage(_viewModel));
	}
}

public partial class BindableLayoutControlMainPage : ContentPage
{
	private BindableLayoutViewModel _viewModel;

	public BindableLayoutControlMainPage(BindableLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BindableLayoutViewModel();
		await Navigation.PushAsync(new BindableLayoutOptionsPage(_viewModel));
	}

	private void RemoveItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.RemoveLastItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.RemoveItemAtIndex(index);
		}

		IndexEntry.Text = string.Empty;
	}

	private void AddItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.AddSequentialItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.AddItemAtIndex(index);
		}

		IndexEntry.Text = string.Empty;
	}

	public void OnGridLoaded(object sender, EventArgs e)
	{
		if (sender is not Grid grid)
			return;

		grid.ChildAdded += (_, _) => ArrangeGridItems(grid);
		grid.ChildRemoved += (_, _) => ArrangeGridItems(grid);

		ArrangeGridItems(grid);
	}

	private void ArrangeGridItems(Grid grid)
	{
		const int columns = 2;

		var children = grid.Children.OfType<View>().ToList();

		grid.RowDefinitions.Clear();
		grid.ColumnDefinitions.Clear();

		for (int i = 0; i < columns; i++)
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

		int totalRows = (int)Math.Ceiling((double)children.Count / columns);
		for (int i = 0; i < totalRows; i++)
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		for (int index = 0; index < children.Count; index++)
		{
			int row = index / columns;
			int column = index % columns;
			Microsoft.Maui.Controls.Grid.SetRow(children[index], row);
			Microsoft.Maui.Controls.Grid.SetColumn(children[index], column);
		}
	}
	private void ReplaceItems_Clicked(object sender, EventArgs e)
	{
		var text = IndexEntry.Text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			_viewModel.ReplaceItem();
		}
		else if (int.TryParse(text, out int index))
		{
			_viewModel.ReplaceItemAtIndex(index);
		}
		IndexEntry.Text = string.Empty;
	}

}