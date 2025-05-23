using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WEllipse = Microsoft.UI.Xaml.Shapes.Ellipse;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;
using WShape = Microsoft.UI.Xaml.Shapes.Shape;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : ItemsControl
	{
		IIndicatorView? _indicatorView;
		const int DefaultPadding = 4;
		WBrush? _selectedColor;
		WBrush? _fillColor;
		ObservableCollection<WShape>? _dots;

		internal bool UseShapeIndicator => _indicatorView == null || (_indicatorView is ITemplatedIndicatorView templatedView && templatedView.IndicatorsLayoutOverride != null);

		public MauiPageControl()
		{
			HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
			VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
			ItemsPanel = GetItemsPanelTemplate();
		}
		public void SetIndicatorView(IIndicatorView indicatorView)
		{
			_indicatorView = indicatorView;

			if (indicatorView == null)
				Items.Clear();
		}

		internal void UpdateIndicatorsColor()
		{
			if (UseShapeIndicator)
			{
				return;
			}

			if (_indicatorView?.IndicatorColor is SolidPaint solidPaint)
				_fillColor = solidPaint?.ToPlatform();
			if (_indicatorView?.SelectedIndicatorColor is SolidPaint selectedSolidPaint)
				_selectedColor = selectedSolidPaint.ToPlatform();
			var position = _indicatorView?.Position;
			int i = 0;
			foreach (var item in Items)
			{
				((WShape)item).Fill = i == position ? _selectedColor : _fillColor;
				i++;
			}
		}

		internal void CreateIndicators()
		{
			if (UseShapeIndicator)
			{
				return;
			}

			var position = GetIndexFromPosition();
			var indicators = new List<WShape>();

			var indicatorCount = _indicatorView?.GetMaximumVisible();
			if (indicatorCount > 0)
			{
				for (int i = 0; i < indicatorCount; i++)
				{
					var shape = CreateIndicator(i, position);

					if (shape != null)
					{
						indicators.Add(shape);
					}
				}
			}

			_dots = new ObservableCollection<WShape>(indicators);
			ItemsSource = _dots;
		}

		static ItemsPanelTemplate GetItemsPanelTemplate()
		{
			var itemsPanelTemplateXaml =
				$@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                        <StackPanel  Orientation='Horizontal'></StackPanel>
			   </ItemsPanelTemplate>";

			return (ItemsPanelTemplate)XamlReader.Load(itemsPanelTemplateXaml);
		}

		WShape? CreateIndicator(int i, int position)
		{
			if (_indicatorView == null)
				return null;

			var indicatorSize = _indicatorView.IndicatorSize;
			WShape? shape = null;
			if (_indicatorView.IsCircleShape())
			{
				shape = new WEllipse()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = indicatorSize,
					Width = indicatorSize,
					Margin = WinUIHelpers.CreateThickness(DefaultPadding, 0, DefaultPadding, 0)
				};
			}
			else
			{
				shape = new WRectangle()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = indicatorSize,
					Width = indicatorSize,
					Margin = WinUIHelpers.CreateThickness(DefaultPadding, 0, DefaultPadding, 0)
				};
			}
			shape.Tag = i;
			shape.PointerPressed += (s, e) =>
			{
				if (_indicatorView == null)
					return;

				_indicatorView.Position = (int)((WShape)s).Tag;
			};
			return shape;
		}

		int GetIndexFromPosition()
		{
			if (_indicatorView == null)
				return 0;

			var maxVisible = _indicatorView.GetMaximumVisible();
			var position = _indicatorView.Position;
			return Math.Max(0, position >= maxVisible ? maxVisible - 1 : position);
		}
	}
}
