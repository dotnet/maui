using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Maui.Platform.WPF.Interfaces;

namespace System.Maui.Platform.WPF.Controls
{
	public class SelectionChangedEventArgs : EventArgs
	{
		public SelectionChangedEventArgs(object oldElement, object newElement)
		{
			OldElement = oldElement;
			NewElement = newElement;
		}

		public object NewElement { get; private set; }

		public object OldElement { get; private set; }
	}

	[System.Windows.Markup.ContentProperty("ItemsSource")]
	public class FormsMultiPage : FormsPage
	{
		public FormsTransitioningContentControl FormsContentControl { get; private set; }

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		public static readonly DependencyProperty ContentLoaderProperty = DependencyProperty.Register("ContentLoader", typeof(IContentLoader), typeof(FormsMultiPage), new PropertyMetadata(new DefaultContentLoader()));
		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<object>), typeof(FormsMultiPage));
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(FormsMultiPage), new PropertyMetadata(OnSelectedItemChanged));
		public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(FormsMultiPage), new PropertyMetadata(0));

		public IContentLoader ContentLoader
		{
			get { return (IContentLoader)GetValue(ContentLoaderProperty); }
			set { SetValue(ContentLoaderProperty, value); }
		}

		public ObservableCollection<object> ItemsSource
		{
			get { return (ObservableCollection<object>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public object SelectedItem
		{
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		private static void OnSelectedItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue == e.NewValue) return;
			((FormsMultiPage)o).OnSelectedItemChanged(e.OldValue, e.NewValue);
		}

		private void OnSelectedItemChanged(object oldValue, object newValue)
		{
			if (ItemsSource == null) return;
			SelectedIndex = ItemsSource.Cast<object>().ToList().IndexOf(newValue);
			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldValue, newValue));
		}

		public FormsMultiPage()
		{
			SetValue(FormsMultiPage.ItemsSourceProperty, new ObservableCollection<object>());
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			FormsContentControl = Template.FindName("PART_Multi_Content", this) as FormsTransitioningContentControl;
		}

		public override bool GetHasNavigationBar()
		{
			if (FormsContentControl != null && FormsContentControl.Content is FormsPage page)
			{
				return page.GetHasNavigationBar();
			}
			return false;
		}

		public override IEnumerable<FrameworkElement> GetPrimaryTopBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.PrimaryTopBarCommands);

			if (FormsContentControl != null && FormsContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetPrimaryTopBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetSecondaryTopBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.SecondaryTopBarCommands);

			if (FormsContentControl != null && FormsContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetSecondaryTopBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetPrimaryBottomBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.PrimaryBottomBarCommands);

			if (FormsContentControl != null && FormsContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetPrimaryBottomBarCommands());
			}

			return frameworkElements;
		}

		public override IEnumerable<FrameworkElement> GetSecondaryBottomBarCommands()
		{
			List<FrameworkElement> frameworkElements = new List<FrameworkElement>();
			frameworkElements.AddRange(this.SecondaryBottomBarCommands);

			if (FormsContentControl != null && FormsContentControl.Content is FormsPage page)
			{
				frameworkElements.AddRange(page.GetSecondaryBottomBarCommands());
			}

			return frameworkElements;
		}
	}
}
