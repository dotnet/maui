using System;
using Android.Content;
using Android.Views;
using AView = Android.Views.View;
using AListView = Android.Widget.ListView;

namespace Xamarin.Forms.Platform.Android
{
	public class TableViewRenderer : ViewRenderer<TableView, AListView>
	{
		public TableViewRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TableViewRenderer(Context) instead.")]
		public TableViewRenderer()
		{
			AutoPackage = false;
		}

		protected virtual TableViewModelRenderer GetModelRenderer(AListView listView, TableView view)
		{
			return new TableViewModelRenderer(Context, listView, view);
		}

		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}

		protected override AListView CreateNativeControl()
		{
			return new AListView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			base.OnElementChanged(e);

			AListView listView = Control;
			if (listView == null)
			{
				listView = CreateNativeControl();
				SetNativeControl(listView);
			}

			listView.Focusable = false;
			listView.DescendantFocusability = DescendantFocusability.AfterDescendants;

			TableView view = e.NewElement;

			TableViewModelRenderer source = GetModelRenderer(listView, view);
			listView.Adapter = source;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				Control?.Adapter?.Dispose();

			base.Dispose(disposing);
		}
	}
}