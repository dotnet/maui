using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.UWP
{
	public class TableViewRenderer : ViewRenderer<TableView, Windows.UI.Xaml.Controls.ListView>
	{
		bool _ignoreSelectionEvent;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.ModelChanged -= OnModelChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new Windows.UI.Xaml.Controls.ListView
					{
						ItemContainerStyle = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["FormsListViewItem"],
						ItemTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["CellTemplate"],
						GroupStyle = { new GroupStyle { HidesIfEmpty = false, HeaderTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["TableSection"] } },
						HeaderTemplate = (Windows.UI.Xaml.DataTemplate)Windows.UI.Xaml.Application.Current.Resources["TableRoot"],
						SelectionMode = ListViewSelectionMode.Single
					});

					// You can't set ItemsSource directly to a CollectionViewSource, it crashes.
					Control.SetBinding(ItemsControl.ItemsSourceProperty, "");
					Control.SelectionChanged += OnSelectionChanged;
				}

				e.NewElement.ModelChanged += OnModelChanged;
				OnModelChanged(e.NewElement, EventArgs.Empty);
			}

			base.OnElementChanged(e);
		}

		void OnModelChanged(object sender, EventArgs e)
		{
			Control.Header = Element.Root;

			// This auto-selects the first item in the new DataContext, so we just null it and ignore the selection
			// as this selection isn't driven by user input
			_ignoreSelectionEvent = true;
			Control.DataContext = new CollectionViewSource { Source = Element.Root, IsSourceGrouped = true };
			_ignoreSelectionEvent = false;
		}

		void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_ignoreSelectionEvent)
			{
				foreach (object item in e.AddedItems)
				{
					var cell = item as Cell;
					if (cell != null)
					{
						if (cell.IsEnabled)
							Element.Model.RowSelected(cell);
						break;
					}
				}
			}

			Control.SelectedItem = null;
		}
	}
}