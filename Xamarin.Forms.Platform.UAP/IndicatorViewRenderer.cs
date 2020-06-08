using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using WEllipse = Windows.UI.Xaml.Shapes.Ellipse;
using WRectangle = Windows.UI.Xaml.Shapes.Rectangle;
using WShape = Windows.UI.Xaml.Shapes.Shape;

namespace Xamarin.Forms.Platform.UWP
{
	class IndicatorViewRenderer : ViewRenderer<IndicatorView, FrameworkElement>
	{
		const int DefaultPadding = 4;

		SolidColorBrush _selectedColor;
		SolidColorBrush _fillColor;
		ObservableCollection<WShape> _dots;

		public IndicatorViewRenderer()
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					UpdateControl();
				}
				_fillColor = new SolidColorBrush(Element.IndicatorColor.ToWindowsColor());

				_selectedColor = new SolidColorBrush(Element.SelectedIndicatorColor.ToWindowsColor());

				CreateIndicators();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(IndicatorView.IndicatorColorProperty, IndicatorView.SelectedIndicatorColorProperty, IndicatorView.PositionProperty))
				UpdateIndicatorsColor();

			if (e.IsOneOf(IndicatorView.CountProperty,
						  IndicatorView.ItemsSourceProperty,
						  IndicatorView.IndicatorsShapeProperty))
				CreateIndicators();
		}

		void UpdateControl()
		{
			var control = (Element.IndicatorTemplate != null)
				? (FrameworkElement)Element.IndicatorLayout.GetOrCreateRenderer()
				: CreateNativeControl();

			SetNativeControl(control);
		}

		FrameworkElement CreateNativeControl()
		{
			return new ItemsControl
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				ItemsPanel = GetItemsPanelTemplate()
			};
		}

		void UpdateIndicatorsColor()
		{
			if (!(Control is ItemsControl))
				return;
			
			_fillColor = new SolidColorBrush(Element.IndicatorColor.ToWindowsColor());
			_selectedColor = new SolidColorBrush(Element.SelectedIndicatorColor.ToWindowsColor());
			var position = Element.Position;
			int i = 0;
			foreach (var item in (Control as ItemsControl).Items)
			{
				((WShape)item).Fill = i == position ? _selectedColor : _fillColor;
				i++;
			}
		}

		ItemsPanelTemplate GetItemsPanelTemplate()
		{
			var itemsPanelTemplateXaml =
				$@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                        <StackPanel  Orientation='Horizontal'></StackPanel>
			   </ItemsPanelTemplate>";

			return (ItemsPanelTemplate)XamlReader.Load(itemsPanelTemplateXaml);
		}

		void CreateIndicators()
		{
			if (!Element.IsVisible || !(Control is ItemsControl))
				return;

			var position = Element.Position;
			var indicators = new List<WShape>();

			if (Element.ItemsSource != null && Element.Count > 0)
			{
				int i = 0;
				foreach (var item in Element.ItemsSource)
				{
					indicators.Add(CreateIndicator(i, position));
					i++;
				}
			}

			_dots = new ObservableCollection<WShape>(indicators);
			(Control as ItemsControl).ItemsSource = _dots;
		}

		WShape CreateIndicator(int i, int position)
		{
			var indicatorSize = Element.IndicatorSize;
			if (Element.IndicatorsShape == IndicatorShape.Circle)
			{
				return new WEllipse()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = indicatorSize,
					Width = indicatorSize,
					Margin = new Windows.UI.Xaml.Thickness(DefaultPadding, 0, DefaultPadding, 0)
				};
			}
			else
			{
				return new WRectangle()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = indicatorSize,
					Width = indicatorSize,
					Margin = new Windows.UI.Xaml.Thickness(DefaultPadding, 0, DefaultPadding, 0)
				};
			}
		}
	}
}
