using System;
using Android.Content;
using Android.Views;
using AView = Android.Views.View;
using AListView = Android.Widget.ListView;
using System.ComponentModel;
#if __ANDROID_29__
using AndroidX.Core.Widget;
#else
using Android.Support.V4.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public class TableViewRenderer : ViewRenderer<TableView, AListView>
	{
		TableViewModelRenderer _adapter;
		bool _disposed;

		public TableViewRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TableViewRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
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

			_adapter = GetModelRenderer(listView, view);
			listView.Adapter = _adapter;
		}
		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (Forms.IsLollipopOrNewer && Control != null)
				Control.NestedScrollingEnabled = (Parent.GetParentOfType<NestedScrollView>() != null);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				// Unhook the adapter from the ListView before disposing of it
				if (Control != null)
				{
					Control.Adapter = null;
				}

				if (_adapter != null)
				{
					_adapter.Dispose();
					_adapter = null;
				}
 			} 


			base.Dispose(disposing);
		}
	}
}