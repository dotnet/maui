#nullable disable
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WApp = Microsoft.UI.Xaml.Application;
using WControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class MauiItemsView : Microsoft.UI.Xaml.Controls.ItemsView, IEmptyView
	{
		ContentControl _emptyViewContentControl;
		FrameworkElement _emptyView;
		View _mauiEmptyView;

		public MauiItemsView()
		{
			Template = (WControlTemplate)WApp.Current.Resources["MauiItemsViewTemplate"];
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(MauiItemsView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MauiItemsView itemsView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				itemsView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get
			{
				return (WVisibility)GetValue(EmptyViewVisibilityProperty);
			}
			set
			{
				SetValue(EmptyViewVisibilityProperty, value);
			}
		}

		public void SetEmptyView(FrameworkElement emptyView, View mauiEmptyView)
		{
			_emptyView = emptyView;
			_mauiEmptyView = mauiEmptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			if (_emptyView != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (_mauiEmptyView != null)
			{
				_mauiEmptyView.Layout(new Rect(0, 0, finalSize.Width, finalSize.Height));
			}

			return base.ArrangeOverride(finalSize);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl == null)
			{
				return;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}
