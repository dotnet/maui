using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms.Internals;
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
				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;
				templatedItems.CollectionChanged -= TemplatedItems_GroupedCollectionChanged;
				templatedItems.GroupedCollectionChanged -= TemplatedItems_GroupedCollectionChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					var listView = new WList
					{
						DataContext = Element,
						ItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["CellTemplate"],
						Style = (System.Windows.Style)System.Windows.Application.Current.Resources["ListViewTemplate"]
					};
					SetNativeControl(listView);
					Control.SelectionChanged += OnNativeSelectionChanged;
				}
				
				// Update control property 
				UpdateItemSource();

				//UpdateHeader();
				//UpdateFooter();
				//UpdateJumpList();

				// Suscribe element event
				TemplatedItemsView.TemplatedItems.CollectionChanged += TemplatedItems_GroupedCollectionChanged;
				TemplatedItemsView.TemplatedItems.GroupedCollectionChanged += TemplatedItems_GroupedCollectionChanged;
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

			if (e.PropertyName == ListView.IsGroupingEnabledProperty.PropertyName)
			{
				//UpdateGrouping();
			}


			/*if (e.PropertyName == ListView.SelectedItemProperty.PropertyName)
				OnItemSelected(Element.SelectedItem);
			else if (e.PropertyName == "HeaderElement")
				UpdateHeader();
			else if (e.PropertyName == "FooterElement")
				UpdateFooter();
			else if ((e.PropertyName == ListView.IsRefreshingProperty.PropertyName) || (e.PropertyName == ListView.IsPullToRefreshEnabledProperty.PropertyName) || (e.PropertyName == "CanRefresh"))
				UpdateIsRefreshing();
			else if (e.PropertyName == "GroupShortNameBinding")
				UpdateJumpList();*/
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

						/*if (HasHeader(group))
							_cells.Add(GetCell(group.HeaderContent));
						else
							_cells.Add(CreateEmptyHeader());*/

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


		
		void OnNativeSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Element.NotifyRowTapped(Control.SelectedIndex, cell:null);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.SelectionChanged -= OnNativeSelectionChanged;
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
	}
}
