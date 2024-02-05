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

		class Test : AListView
		{
			public Test(Context context) : base(context)
			{
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}
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

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			// If you do an AtMost (or Unspecified) height measure of ListView, Android will basically create
			// a scrap copy of all the ListView cells to calculate the height.
			// https://cs.android.com/android/platform/superproject/main/+/main:frameworks/base/core/java/android/widget/ListView.java;l=1314-1322?q=ListView
			// This causes issues, because if a TextCell already has a view that's attached to the visual tree then android
			// will call "GetView" without a convert view and then it just uses that views a scrap view for measuring.
			// Our problem, is that we don't have a way of knowing if a view we are returning from getView will be the one we
			// should track against our TextCellHandler or not. 
			// This all worked fine in XF because in XF we didn't really block against just creating as many renderers against a single
			// VirtualView as you wanted. This led to a whole different set of hard to track issues.
			// Fundamentally the ListView control on Android is an old control and the TableView should really be converted to
			// a BindableLayout or just generating xplat views against a VerticalStackLayout.
			//
			// We handle the Unspecified path inside "GetDesiredSize" by calculating the height of the cells ourselves and requesting an exact measure.\\
			// Because another quirk of ListView is that if you give it an unspecified measure it'll just size itself to the first row
			// https://cs.android.com/android/platform/superproject/main/+/main:frameworks/base/core/java/android/widget/ListView.java;l=1289-1304?q=ListView
			//
			// There is a path here where we could make our structures play friendly with the ListView and then just let ListView do its scrapview thing
			// But, for how we use TableView, converting to an Exactly measure seems good enough for us.	
			if (heightMeasureSpec.GetMode() == MeasureSpecMode.AtMost)
			{
				var size = MeasureSpec.GetSize(heightMeasureSpec);
				heightMeasureSpec = MeasureSpec.MakeMeasureSpec(size, MeasureSpecMode.Exactly);
			}

			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
		}
#pragma warning restore RS0016 // Add public types and members to the declared API

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (double.IsInfinity(heightConstraint))
			{
				if (Element.RowHeight > -1)
				{
					heightConstraint = (int)(_adapter.Count * Element.RowHeight);
				}
				else if (_adapter is not null)
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

						var platformView = cell.Handler?.PlatformView as AView;
						var convertView = (platformView?.Parent as AView) ?? platformView;

						AView listItem = _adapter.GetView(i, convertView, Control);
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