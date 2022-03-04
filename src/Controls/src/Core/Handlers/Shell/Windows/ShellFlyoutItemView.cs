using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WRect = Windows.Foundation.Rect;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellFlyoutItemView : ContentControl
	{
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected), typeof(bool), typeof(ShellFlyoutItemView),
			new PropertyMetadata(default(bool), IsSelectedChanged));

		View _content;
		object _previousDataContext;
		FrameworkElement FrameworkElement { get; set; }
		Shell _shell;
		public ShellFlyoutItemView()
		{
			this.DataContextChanged += OnDataContextChanged;
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);
		}

		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		void OnDataContextChanged(Microsoft.UI.Xaml.FrameworkElement sender, Microsoft.UI.Xaml.DataContextChangedEventArgs args)
		{
			if (_previousDataContext == args.NewValue)
				return;

			_previousDataContext = args.NewValue;
			if (_content != null)
			{
				if (_content.BindingContext is INotifyPropertyChanged inpc)
					inpc.PropertyChanged -= ShellElementPropertyChanged;

				_shell?.RemoveLogicalChild(_content);
				_content.Cleanup();
				_content.BindingContext = null;
				_content.Parent = null;
				_content = null;
			}

			var bo = (BindableObject)args.NewValue;
			var element = bo as Element;
			_shell = element?.FindParentOfType<Shell>();
			DataTemplate dataTemplate = (_shell as IShellController)?.GetFlyoutItemDataTemplate(bo);

			if (bo != null)
				bo.PropertyChanged += ShellElementPropertyChanged;

			if (dataTemplate != null)
			{
				_content = (View)dataTemplate.CreateContent();
				_content.BindingContext = bo;
				_shell.AddLogicalChild(_content);

				var renderer = _content.ToPlatform(_shell.Handler.MauiContext);

				Content = renderer;
				FrameworkElement = renderer;

				UpdateVisualState();
			}
		}

		void ShellElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(BaseShellItem.IsCheckedProperty))
				UpdateVisualState();

		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (this.ActualWidth > 0 && _content is IView view)
			{
				if (Parent is FrameworkElement fe)
				{
					if (!_content.IsVisible)
					{
						fe.Visibility = WVisibility.Collapsed;
					}
					else
					{
						fe.Visibility = WVisibility.Visible;
					}
				}

				var request = view.Handler.GetDesiredSizeFromHandler(availableSize.Width, availableSize.Height);
				view.Frame = new Rect(0, 0, request.Width, request.Height);
				Clip = new RectangleGeometry { Rect = new WRect(0, 0, request.Width, request.Height) };
				return request.ToPlatform();
			}

			return base.MeasureOverride(availableSize);
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (this.ActualWidth > 0 && _content is IView view)
			{
				view.Handler.PlatformArrangeHandler(new Rect(0, 0, finalSize.Width, finalSize.Height));
				return finalSize;
			}

			return base.ArrangeOverride(finalSize);
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
			((ShellFlyoutItemView)d).UpdateVisualState();
		}
	}
}
