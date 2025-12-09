using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WItemsControl = Microsoft.UI.Xaml.Controls.ItemsControl;
using WSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.TableViewRenderer instead")]
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class TableViewRenderer : ViewRenderer<TableView, Microsoft.UI.Xaml.Controls.ListView>
#pragma warning restore CS0618 // Type or member is obsolete
	{
		bool _ignoreSelectionEvent;
		bool _disposed;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (e.OldElement != null)
			{
				e.OldElement.ModelChanged -= OnModelChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{

					SetNativeControl(new Microsoft.UI.Xaml.Controls.ListView
					{
						ItemContainerStyle = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["FormsListViewItem"],
						ItemTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["CellTemplate"],
						GroupStyle = { new GroupStyle { HidesIfEmpty = false, HeaderTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["TableSection"] } },
						HeaderTemplate = (Microsoft.UI.Xaml.DataTemplate)Microsoft.UI.Xaml.Application.Current.Resources["TableRoot"],
						SelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode.Single
					});

					// You can't set ItemsSource directly to a CollectionViewSource, it crashes.
					Control.SetBinding(WItemsControl.ItemsSourceProperty, "");
					Control.SelectionChanged += OnSelectionChanged;
				}

				e.NewElement.ModelChanged += OnModelChanged;
				OnModelChanged(e.NewElement, EventArgs.Empty);
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
				{
					Control.SelectionChanged -= OnSelectionChanged;
				}
			}
			base.Dispose(disposing);
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

		void OnSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			if (!_ignoreSelectionEvent)
			{
				foreach (object item in e.AddedItems)
				{
					if (item is Cell cell)
					{
						if (cell.IsEnabled)
							Element.Model.RowSelected(cell);
						break;
					}
				}
			}

			if (Control == null)
				return;

			Control.SelectedItem = null;
		}
	}
}
