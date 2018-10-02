using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using WList = System.Windows.Controls.ListView;

namespace Xamarin.Forms.Platform.WPF
{
	public class ListViewRenderer : ViewRenderer<ListView, WList>
	{
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				e.OldElement.ItemSelected -= OnElementItemSelected;
				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;
				templatedItems.CollectionChanged -= TemplatedItems_GroupedCollectionChanged;
				templatedItems.GroupedCollectionChanged -= TemplatedItems_GroupedCollectionChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ItemSelected += OnElementItemSelected;

				if (Control == null) // Construct and SetNativeControl and suscribe control event
				{
					var listView = new WList
					{
						DataContext = Element,
						ItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["CellTemplate"],
						Style = (System.Windows.Style)System.Windows.Application.Current.Resources["ListViewTemplate"]
					};

					SetNativeControl(listView);

					Control.MouseUp += OnNativeMouseUp;
					Control.KeyUp += OnNativeKeyUp;
					Control.TouchUp += OnNativeTouchUp;
					Control.StylusUp += OnNativeStylusUp;
				}

				// Update control property 
				UpdateItemSource();

				// Suscribe element event
				TemplatedItemsView.TemplatedItems.CollectionChanged += TemplatedItems_GroupedCollectionChanged;
				TemplatedItemsView.TemplatedItems.GroupedCollectionChanged += TemplatedItems_GroupedCollectionChanged;

				if (Element.SelectedItem != null)
					OnElementItemSelected(null, new SelectedItemChangedEventArgs(Element.SelectedItem));
			}

			base.OnElementChanged(e);
		}

		private void TemplatedItems_GroupedCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateItemSource();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}

		void UpdateItemSource()
		{
			List<object> items = new List<object>();
			if (Element.IsGroupingEnabled)
			{
				int index = 0;
				foreach (var groupItem in TemplatedItemsView.TemplatedItems)
				{
					var group = TemplatedItemsView.TemplatedItems.GetGroup(index);

					if (group.Count != 0)
					{
						items.Add(group.HeaderContent);

						items.AddRange(group);
					}

					index++;
				}
				Control.ItemsSource = items;
			}
			else
			{
				Control.ItemsSource = Element.TemplatedItems;
			}
		}

		void OnNativeKeyUp(object sender, KeyEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeMouseUp(object sender, MouseButtonEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeTouchUp(object sender, TouchEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		void OnNativeStylusUp(object sender, StylusEventArgs e)
			=> Element.NotifyRowTapped(Control.SelectedIndex, cell: null);

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.MouseUp -= OnNativeMouseUp;
					Control.KeyUp -= OnNativeKeyUp;
					Control.TouchUp -= OnNativeTouchUp;
					Control.StylusUp -= OnNativeStylusUp;
				}

				if (Element != null)
				{
					TemplatedItemsView.TemplatedItems.CollectionChanged -= TemplatedItems_GroupedCollectionChanged;
					TemplatedItemsView.TemplatedItems.GroupedCollectionChanged -= TemplatedItems_GroupedCollectionChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		void OnElementItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (e.SelectedItem == null)
			{
				Control.SelectedIndex = -1;
				return;
			}

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var index = 0;

			if (Element.IsGroupingEnabled)
			{
				int selectedItemIndex = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
				var leftOver = 0;
				int groupIndex = templatedItems.GetGroupIndexFromGlobal(selectedItemIndex, out leftOver);

				index = selectedItemIndex - (groupIndex + 1);
			}
			else
			{
				index = templatedItems.GetGlobalIndexOfItem(e.SelectedItem);
			}

			Control.SelectedIndex = index;
		}
	}
}
