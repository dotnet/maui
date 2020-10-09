using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WRect = Windows.Foundation.Rect;

namespace Xamarin.Forms.Platform.UWP
{
	public class ShellFlyoutItemRenderer : ContentControl
	{
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected), typeof(bool), typeof(ShellFlyoutItemRenderer),
			new PropertyMetadata(default(bool), IsSelectedChanged));

		View _content;
		FrameworkElement FrameworkElement { get; set; }

		public ShellFlyoutItemRenderer()
		{
			this.DataContextChanged += OnDataContextChanged;
		}

		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		void OnDataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
		{
			if (_content != null)
			{
				if(_content.BindingContext is INotifyPropertyChanged inpc)
					inpc.PropertyChanged -= ShellElementPropertyChanged;

				_content.Cleanup();
				_content.MeasureInvalidated -= OnMeasureInvalidated;
				_content.BindingContext = null;
				_content.Parent = null;
				_content = null;
			}

			var bo = (BindableObject)args.NewValue;
			var element = bo as Element;
			var shell = element?.FindParent<Shell>();
			DataTemplate dataTemplate = (shell as IShellController)?.GetFlyoutItemDataTemplate(bo);

			if(bo != null)
				bo.PropertyChanged += ShellElementPropertyChanged;

			if(dataTemplate != null)
			{
				_content = (View)dataTemplate.CreateContent();
				_content.BindingContext = bo;
				_content.Parent = shell;
				_content.MeasureInvalidated += OnMeasureInvalidated;
				IVisualElementRenderer renderer = Platform.CreateRenderer(_content);
				Platform.SetRenderer(_content, renderer);

				Content = renderer.ContainerElement;
				FrameworkElement = renderer.ContainerElement;

				// make sure we re-measure once the template is applied
				if (FrameworkElement != null)
				{
					FrameworkElement.Loaded += OnFrameworkElementLoaded;

					void OnFrameworkElementLoaded(object _, RoutedEventArgs __)
					{
						OnMeasureInvalidated();
						FrameworkElement.Loaded -= OnFrameworkElementLoaded;
					}
				}

				UpdateVisualState();
				OnMeasureInvalidated();

				if (renderer.ContainerElement != null)
					renderer.ContainerElement.SetAutomationPropertiesAutomationId(element.AutomationId);
			}
		}

		void ShellElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(BaseShellItem.IsCheckedProperty))
				UpdateVisualState();
			
		}

		void OnMeasureInvalidated(object sender, EventArgs e)
		{
			OnMeasureInvalidated();
		}

		void OnMeasureInvalidated()
		{
			Size request = _content.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins).Request;

			var minSize = (double)Windows.UI.Xaml.Application.Current.Resources["NavigationViewItemOnLeftMinHeight"];

			if (request.Height < minSize)
			{
				request.Height = minSize;
			}

			if (this.ActualWidth > request.Width)
				request.Width = this.ActualWidth;

			Layout.LayoutChildIntoBoundingRegion(_content, new Rectangle(0, 0, request.Width, request.Height));
		}

		void UpdateVisualState()
		{
			if (_content?.BindingContext is BaseShellItem baseShellItem && baseShellItem != null)
			{
				if (baseShellItem.IsChecked)
					VisualStateManager.GoToState(_content, "Selected");
				else
					VisualStateManager.GoToState(_content, "Normal");
			}
		}

		static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ShellFlyoutItemRenderer)d).UpdateVisualState();
		}
	}
}
