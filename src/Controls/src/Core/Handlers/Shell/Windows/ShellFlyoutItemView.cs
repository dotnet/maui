using System.ComponentModel;
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
		Shell _shell;
		ShellView ShellView => _shell.Handler?.PlatformView as ShellView;

		public ShellFlyoutItemView()
		{
			this.DataContextChanged += OnDataContextChanged;
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

				if (_content.Parent is BaseShellItem bsi)
					bsi.RemoveLogicalChild(_content);
				else
					_shell?.RemoveLogicalChild(_content);

				_content.Cleanup();
				_content.BindingContext = null;
				_content.Parent = null;
				_content = null;
			}

			BindableObject bo = null;

			if (args.NewValue is NavigationViewItemViewModel vm && vm.Data is BindableObject bindableObject)
				bo = bindableObject;
			else
				bo = (BindableObject)args.NewValue;

			var element = bo as Element;
			_shell = element?.FindParentOfType<Shell>();
			DataTemplate dataTemplate = (_shell as IShellController)?.GetFlyoutItemDataTemplate(bo);

			if (bo != null)
				bo.PropertyChanged += ShellElementPropertyChanged;

			if (dataTemplate != null)
			{
				_content = (View)dataTemplate.CreateContent();

				// Set binding context before calling AddLogicalChild so parent binding context doesn't propagate to view
				_content.BindingContext = bo;

				if (bo is BaseShellItem bsi)
					bsi.AddLogicalChild(_content);
				else
					_shell.AddLogicalChild(_content);


				var platformView = _content.ToPlatform(_shell.Handler.MauiContext);

				Content = platformView;
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
			if (ShellView == null)
				return base.MeasureOverride(availableSize);

			if (!ShellView.IsPaneOpen)
				return base.MeasureOverride(availableSize);

			if (ShellView.OpenPaneLength < availableSize.Width)
				return base.MeasureOverride(availableSize);

			if (_content is IView view)
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

				var request = view.Measure(availableSize.Width, availableSize.Height);
				Clip = new RectangleGeometry { Rect = new WRect(0, 0, request.Width, request.Height) };
				return request.ToPlatform();
			}

			return base.MeasureOverride(availableSize);
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (this.ActualWidth > 0 && _content is IView view)
			{
				view.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
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
