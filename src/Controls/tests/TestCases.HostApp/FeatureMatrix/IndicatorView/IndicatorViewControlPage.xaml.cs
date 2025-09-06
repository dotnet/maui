using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class IndicatorViewControlPage : NavigationPage
{
	private IndicatorViewViewModel _viewModel;
	public IndicatorViewControlPage()
	{
		_viewModel = new IndicatorViewViewModel();
		PushAsync(new IndicatorViewControlMainPage(_viewModel));
	}
}

public partial class IndicatorViewControlMainPage : ContentPage
{
	private IndicatorViewViewModel _viewModel;
	private readonly Color[] _fixedColors = new Color[]
	{
			Colors.Red,
			Colors.Green,
			Colors.Blue,
			Colors.Orange,
			Colors.Purple,
			Colors.Teal,
			Colors.Brown
	};

	public IndicatorViewControlMainPage(IndicatorViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel = new IndicatorViewViewModel();
		BindingContext = _viewModel;
		_viewModel.Position = 0;
		_viewModel.CurrentItem = _viewModel.CarouselItems.FirstOrDefault();
		await Navigation.PushAsync(new IndicatorViewOptionsPage(_viewModel));
	}

	private void OnAddItemClicked(object sender, EventArgs e)
	{
		var index = _viewModel.CarouselItems.Count;
		var color = _fixedColors[index % _fixedColors.Length];

		_viewModel.CarouselItems.Add(new IndicatorViewCarouselItem
		{
			Title = $"Item {index + 1}",
			Description = $"Description for item {index + 1}",
			Color = color
		});

		_viewModel.Count = _viewModel.CarouselItems.Count;
	}

	private void OnRemoveItemClicked(object sender, EventArgs e)
	{
		if (_viewModel.CarouselItems.Count > 0)
		{
			_viewModel.CarouselItems.RemoveAt(_viewModel.CarouselItems.Count - 1);
			_viewModel.Count = _viewModel.CarouselItems.Count;

			if (_viewModel.Position >= _viewModel.CarouselItems.Count)
				_viewModel.Position = Math.Max(0, _viewModel.CarouselItems.Count - 1);
		}
	}
}