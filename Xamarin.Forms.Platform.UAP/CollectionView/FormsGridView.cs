using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.Platform.UWP
{
	internal class FormsGridView : GridView, IEmptyView
	{
		int _maximumRowsOrColumns;
		ItemsWrapGrid _wrapGrid;
		ContentControl _emptyViewContentControl;
		FrameworkElement _emptyView;

		public FormsGridView()
		{
			// TODO hartez 2018/06/06 09:52:16 Do we need to clean this up? If so, where?	
			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);
			Loaded += OnLoaded;
		}

		public int MaximumRowsOrColumns
		{
			get => _maximumRowsOrColumns;
			set
			{
				_maximumRowsOrColumns = value;
				if (_wrapGrid != null)
				{
					_wrapGrid.MaximumRowsOrColumns = MaximumRowsOrColumns;
				}
			}
		}

		public Visibility EmptyViewVisibility
		{
			get { return (Visibility)GetValue(EmptyViewVisibilityProperty); }
			set { SetValue(EmptyViewVisibilityProperty, value); }
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility), 
				typeof(FormsGridView), new PropertyMetadata(Visibility.Collapsed));

		// TODO hartez 2018/06/06 10:01:32 Probably should just create a local enum for this?	
		public void UseHorizontalItemsPanel()
		{
			ItemsPanel =
				(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["HorizontalGridItemsPanel"];
		}

		public void UseVerticalItemsPanel()
		{
			ItemsPanel =
				(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["VerticalGridItemsPanel"];
		}

		void FindItemsWrapGrid()
		{
			_wrapGrid = this.GetFirstDescendant<ItemsWrapGrid>();

			if (_wrapGrid == null)
			{
				return;
			}

			_wrapGrid.MaximumRowsOrColumns = MaximumRowsOrColumns;
		}

		void ItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
		{
			FindItemsWrapGrid();
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			FindItemsWrapGrid();
		}

		public void SetEmptyView(FrameworkElement emptyView)
		{
			_emptyView = emptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			if (_emptyView != null)
			{
				_emptyViewContentControl.Content = _emptyView;
			}
		}
	}
}