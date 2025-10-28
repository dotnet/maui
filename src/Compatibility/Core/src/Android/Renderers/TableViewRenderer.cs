using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AListView = Android.Widget.ListView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.TableViewRenderer instead")]
	public class TableViewRenderer : ViewRenderer<TableView, AListView>
	{
		TableViewModelRenderer _adapter;
		bool _disposed;

		public TableViewRenderer(Context context) : base(context)
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

			Control?.NestedScrollingEnabled = (Parent.GetParentOfType<NestedScrollView>() != null);
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
				Control?.Adapter = null;

				_adapter?.Dispose();
				_adapter = null;
			}


			base.Dispose(disposing);
		}
	}
}