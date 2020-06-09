using System;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TableViewRenderer : ViewRenderer<TableView, Native.ListView>
	{
		internal static BindableProperty PresentationProperty = BindableProperty.Create("Presentation", typeof(View), typeof(TableSectionBase), null, BindingMode.OneWay, null, null, null, null, null as BindableProperty.CreateDefaultValueDelegate);

		public TableViewRenderer()
		{
			RegisterPropertyHandler(TableView.HasUnevenRowsProperty, UpdateHasUnevenRows);
			RegisterPropertyHandler(TableView.RowHeightProperty, UpdateRowHeight);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl(Forms.NativeParent));
				Control.ItemSelected += OnSelected;
			}

			if (e.OldElement != null)
			{
				e.OldElement.ModelChanged -= OnRootPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ModelChanged += OnRootPropertyChanged;
				(Control as ITableView)?.ApplyTableRoot(e.NewElement.Root);
			}

			base.OnElementChanged(e);
		}

		protected virtual Native.ListView CreateNativeControl(EvasObject parent)
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				return new Native.Watch.WatchTableView(parent, Forms.CircleSurface);
			}
			else
			{
				return new Native.TableView(parent);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.ModelChanged -= OnRootPropertyChanged;
				}

				if (Control != null)
				{
					Control.ItemSelected -= OnSelected;
				}
			}

			base.Dispose(disposing);
		}

		void OnSelected(object sender, GenListItemEventArgs e)
		{
			var item = e.Item as GenListItem;

			if (item != null)
			{
				var clickedCell = item.Data as Native.ListView.ItemContext;
				if (null != clickedCell)
				{
					Element.Model.RowSelected(clickedCell.Cell);
				}
			}
		}

		void OnRootPropertyChanged(object sender, EventArgs e)
		{
			if (Element != null)
			{
				(Control as ITableView)?.ApplyTableRoot(Element.Root);
			}
		}

		void UpdateHasUnevenRows()
		{
			Control.SetHasUnevenRows(Element.HasUnevenRows);
		}

		void UpdateRowHeight()
		{
			Control.UpdateRealizedItems();
		}

	}
}
