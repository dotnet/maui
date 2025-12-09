#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UwpApp = Microsoft.UI.Xaml.Application;
using UwpControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsListView : Microsoft.UI.Xaml.Controls.ListView, IEmptyView
	{
		ContentControl _emptyViewContentControl;
		ScrollViewer _scrollViewer;
		FrameworkElement _emptyView;
		View _formsEmptyView;

		public FormsListView()
		{
			Template = (UwpControlTemplate)UwpApp.Current.Resources["FormsListViewTemplate"];

			ScrollViewer.SetHorizontalScrollBarVisibility(this, UwpScrollBarVisibility.Disabled);
			ScrollViewer.SetVerticalScrollBarVisibility(this, UwpScrollBarVisibility.Auto);
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(FormsListView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FormsListView listView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				listView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
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

		public void SetEmptyView(FrameworkElement emptyView, View formsEmptyView)
		{
			_emptyView = emptyView;
			_formsEmptyView = formsEmptyView;

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

			_scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;

			if (_emptyView != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			_formsEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return base.ArrangeOverride(finalSize);
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			// Adjust the ScrollViewer's hit test visibility if it exists
			if (_scrollViewer is not null)
			{
				// When the empty view is visible, disable hit testing for the ScrollViewer.
				// This ensures that interactions are directed to the empty view instead of the ScrollViewer.
				// In the template, the empty view is placed below the ScrollViewer in the visual tree.
				_scrollViewer.IsHitTestVisible = visibility != WVisibility.Visible;
			}

			_emptyViewContentControl.Visibility = visibility;
		}
	}
}