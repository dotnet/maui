using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal class IndicatorStackLayout : StackLayout
	{
		readonly IndicatorView _indicatorView;
		public IndicatorStackLayout(IndicatorView indicatorView)
		{
			_indicatorView = indicatorView;
			Orientation = StackOrientation.Horizontal;
			_indicatorView.PropertyChanged += IndicatorViewPropertyChanged;
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is View view)
			{
				var tapGestureRecognizer = new TapGestureRecognizer
				{
					Command = new Command(sender => _indicatorView.Position = Children.IndexOf(sender)),
					CommandParameter = view
				};
				view.GestureRecognizers.Add(tapGestureRecognizer);
			}
		}

		void IndicatorViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == IndicatorView.IndicatorsShapeProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorTemplateProperty.PropertyName)
			{
				ResetIndicators();
			}
			if (e.PropertyName == IndicatorView.MaximumVisibleProperty.PropertyName
				|| e.PropertyName == IndicatorView.PositionProperty.PropertyName
				|| e.PropertyName == IndicatorView.HideSingleProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorColorProperty.PropertyName
				|| e.PropertyName == IndicatorView.SelectedIndicatorColorProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorSizeProperty.PropertyName)
			{
				ResetIndicatorStyles();
			}
		}

		void ResetIndicatorStyles()
		{
			try
			{
				BatchBegin();
				ResetIndicatorStylesNonBatch();
			}
			finally
			{
				BatchCommit();
			}
		}

		internal void ResetIndicators()
		{
			try
			{
				BatchBegin();
				BindIndicatorItems();
			}
			finally
			{
				ResetIndicatorStylesNonBatch();
				BatchCommit();
			}
		}

		internal void ResetIndicatorCount(int oldCount)
		{
			try
			{
				BatchBegin();
				if (oldCount < 0)
				{
					oldCount = 0;
				}

				if (oldCount > _indicatorView.Count)
				{
					return;
				}

				BindIndicatorItems();
			}
			finally
			{
				ResetIndicatorStylesNonBatch();
				BatchCommit();
			}
		}

		protected override void OnInsert(int index, IView view)
		{
			base.OnInsert(index, view);
			ResetIndicatorStylesNonBatch();
		}

		protected override void OnRemove(int index, IView view)
		{
			base.OnRemove(index, view);
			ResetIndicatorStylesNonBatch();
		}

		void ResetIndicatorStylesNonBatch()
		{
			var indicatorCount = _indicatorView.Count;
			var childrenCount = Children.Count;
			var maxVisible = _indicatorView.MaximumVisible;
			var position = _indicatorView.Position;
			var selectedIndex = position >= maxVisible ? maxVisible - 1 : position;

			for (int index = 0; index < childrenCount; index++)
			{
				bool isSelected = index == selectedIndex;
				if (Children[index] is not VisualElement visualElement)
				{
					return;
				}

				visualElement.BackgroundColor = isSelected
					? GetColorOrDefault(_indicatorView.SelectedIndicatorColor, Colors.Gray)
					: GetColorOrDefault(_indicatorView.IndicatorColor, Colors.Silver);


				VisualStateManager.GoToState(visualElement, isSelected
					? VisualStateManager.CommonStates.Selected
					: VisualStateManager.CommonStates.Normal);

			}

			IsVisible = indicatorCount > 1 || !_indicatorView.HideSingle;
		}

		Color GetColorOrDefault(Color? color, Color defaultColor) => color ?? defaultColor;

		void BindIndicatorItems()
		{
			var indicatorSize = _indicatorView.IndicatorSize > 0 ? _indicatorView.IndicatorSize : 10;
			var indicatorTemplate = _indicatorView.IndicatorTemplate ??= new DataTemplate(() => new Border
			{
				Padding = 0,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = indicatorSize,
				HeightRequest = indicatorSize,
				StrokeShape = new RoundRectangle()
				{
					CornerRadius = _indicatorView.IndicatorsShape == IndicatorShape.Circle
						? (float)indicatorSize / 2
						: 0,
					Stroke = Colors.Transparent
				}
			});

			// Get the filtered items source based on MaximumVisible
			var itemsSource = GetFilteredItemsSource();
			BindableLayout.SetItemsSource(this, itemsSource);

			BindableLayout.SetItemTemplate(this, indicatorTemplate);
		}

		IEnumerable? GetFilteredItemsSource()
		{
			if (_indicatorView.ItemsSource is null || _indicatorView.MaximumVisible <= 0)
			{
				return null;
			}

			if (_indicatorView.ItemsSource is IEnumerable enumerable)
			{
				int totalCount = enumerable.Cast<object>().Count();
				int maxVisible = Math.Min(totalCount, _indicatorView.MaximumVisible);
				if (totalCount <= maxVisible)
				{
					return _indicatorView.ItemsSource;
				}

				var filteredItems = new List<object>(maxVisible);
				int count = 0;
				foreach (var item in enumerable)
				{
					if (count++ >= maxVisible)
					{
						break;
					}

					filteredItems.Add(item);
				}	

				return filteredItems;
			}

			return _indicatorView.ItemsSource;
		}

		public void Remove()
		{
			_indicatorView.PropertyChanged -= IndicatorViewPropertyChanged;
		}
	}
}
