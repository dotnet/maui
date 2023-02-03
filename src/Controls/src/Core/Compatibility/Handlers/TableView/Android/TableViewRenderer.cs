#nullable disable
using Android.Content;
using Android.Views;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewRenderer : ViewRenderer<TableView, AListView>
	{
		public static PropertyMapper<TableView, TableViewRenderer> Mapper =
				new PropertyMapper<TableView, TableViewRenderer>(VisualElementRendererMapper);


		public static CommandMapper<TableView, TableViewRenderer> CommandMapper =
			new CommandMapper<TableView, TableViewRenderer>(VisualElementRendererCommandMapper);

		TableViewModelRenderer _adapter;
		bool _reattached;
		bool _disposed;

		public TableViewRenderer(Context context) : base(context, Mapper, CommandMapper)
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

			if (Control != null)
				Control.NestedScrollingEnabled = (Parent.GetParentOfType<NestedScrollView>() != null);

			// There might be a better way to go about doing this but from what I can tell 
			// once you detach and then reattach a ListView the cells become unselectable 
			// and the Android.ListView in general is left in an odd state.
			// We didn't have to do this in XF because in XF there's an extra measure call that happens
			// when the listview is reattached that essentially does the exact same thing.
			// You can see this by adding back the legacy renderers and setting a breakpoint on the 
			// adapter.GetView call. In MAUI this never gets called when navigating back vs XF it does
			if (!_reattached)
			{
				_reattached = true;
			}
			else
			{
				Control?.InvalidateViews();
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (double.IsInfinity(heightConstraint))
			{
				if (Element.RowHeight > -1)
				{
					heightConstraint = (int)(_adapter.Count * Element.RowHeight);
				}
				else if (_adapter != null)
				{
					double totalHeight = 0;
					int adapterCount = _adapter.Count;
					for (int i = 0; i < adapterCount; i++)
					{
						var cell = (Cell)_adapter[i];
						if (cell.Height > -1)
						{
							totalHeight += cell.Height;
							continue;
						}

						AView listItem = _adapter.GetView(i, null, Control);
						int widthSpec;

						if (double.IsInfinity(widthConstraint))
							widthSpec = MeasureSpecMode.Unspecified.MakeMeasureSpec(0);
						else
							widthSpec = MeasureSpecMode.AtMost.MakeMeasureSpec((int)Context.ToPixels(widthConstraint));

						listItem.Measure(widthSpec, MeasureSpecMode.Unspecified.MakeMeasureSpec(0));
						totalHeight += Context.FromPixels(listItem.MeasuredHeight);
					}

					heightConstraint = totalHeight;
				}
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
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