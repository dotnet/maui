using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Android.Graphics.Drawables;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AButton = Android.Widget.Button;
using Android.OS;
using System.Reflection;

[assembly: ExportRenderer(typeof(Bugzilla31395.CustomContentView), typeof(CustomContentRenderer))]
[assembly: ExportRenderer(typeof(NativeListView), typeof(NativeListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeListView2), typeof(NativeAndroidListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeCell), typeof(NativeAndroidCellRenderer))]
#if PRE_APPLICATION_CLASS
#elif FORMS_APPLICATION_ACTIVITY
#else
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(NativeDroidMasterDetail))]
#endif
namespace Xamarin.Forms.ControlGallery.Android
{
	public class NativeDroidMasterDetail : Xamarin.Forms.Platform.Android.AppCompat.MasterDetailPageRenderer
	{
		MasterDetailPage _page;

		protected override void OnElementChanged(VisualElement oldElement, VisualElement newElement)
		{
			base.OnElementChanged(oldElement, newElement);

			if (newElement == null)
			{
				return;
			}

			_page = newElement as MasterDetailPage;
			_page.PropertyChanged += Page_PropertyChanged; 
			_page.LayoutChanged += Page_LayoutChanged;
		}

		void Page_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			pChange();
		}

		void Page_LayoutChanged(object sender, EventArgs e)
		{
			pChange();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _page != null)
			{
				_page.LayoutChanged -= Page_LayoutChanged;
				_page.PropertyChanged -= Page_PropertyChanged;
			}

			base.Dispose(disposing);
		}

		public void pChange()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				var drawer = GetChildAt(1);
				var detail = GetChildAt(0);

				var padding = detail.GetType().GetRuntimeProperty("TopPadding");

				try
				{
					int value = (int)padding.GetValue(detail);
					padding.SetValue(drawer, value);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}

	public class NativeListViewRenderer : ViewRenderer<NativeListView, global::Android.Widget.ListView>
	{
		public NativeListViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new global::Android.Widget.ListView(Forms.Context));
			}

			if (e.OldElement != null)
			{
				// unsubscribe
				Control.ItemClick -= Clicked;
			}

			if (e.NewElement != null)
			{
				// subscribe

				Control.Adapter = new NativeListViewAdapter(Forms.Context as Activity, e.NewElement);
				Control.ItemClick += Clicked;
			}
		}

		void Clicked(object sender, AdapterView.ItemClickEventArgs e)
		{
			Element.NotifyItemSelected(Element.Items.ToList()[e.Position]);
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource

				Control.Adapter = new NativeListViewAdapter(Forms.Context as Activity, Element);
			}
		}
	}

	public class NativeListViewAdapter : BaseAdapter<string>
	{
		readonly Activity _context;
		IList<string> _tableItems = new List<string>();

		public IEnumerable<string> Items
		{
			set
			{
				_tableItems = value.ToList();
			}
		}

		public NativeListViewAdapter(Activity context, NativeListView view)
		{
			_context = context;
			_tableItems = view.Items.ToList();
		}

		public override string this[int position]
		{
			get
			{
				return _tableItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return _tableItems.Count; }
		}

		public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
		{
			// Get our object for this position
			var item = _tableItems[position];

			var view = convertView;
			if (view == null)
			{ // no view to re-use, create new
				view = _context.LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleListItem1, null);
			}

			view.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = item;

			return view;
		}
	}

	/// <summary>
	/// This renderer uses a view defined in /Resources/Layout/NativeAndroidCell.axml
	/// as the cell layout
	/// </summary>
	public class NativeAndroidCellRenderer : ViewCellRenderer
	{
		public NativeAndroidCellRenderer()
		{
		}

		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
			var x = (NativeCell)item;

			var view = convertView;

			if (view == null)
			{// no view to re-use, create new
				view = (context as Activity).LayoutInflater.Inflate(Resource.Layout.NativeAndroidCell, null);
			}
			else { // re-use, clear image
				   // doesn't seem to help
				   //view.FindViewById<ImageView> (Resource.Id.Image).Drawable.Dispose ();
			}

			view.FindViewById<TextView>(Resource.Id.Text1).Text = x.Name;
			view.FindViewById<TextView>(Resource.Id.Text2).Text = x.Category;

			// grab the old image and dispose of it
			// TODO: optimize if the image is the *same* and we want to just keep it
			if (view.FindViewById<ImageView>(Resource.Id.Image).Drawable != null)
			{
				using (var image = view.FindViewById<ImageView>(Resource.Id.Image).Drawable as BitmapDrawable)
				{
					if (image != null)
					{
						if (image.Bitmap != null)
						{
							//image.Bitmap.Recycle ();
							image.Bitmap.Dispose();
						}
					}
				}
			}

			// If a new image is required, display it
			if (!string.IsNullOrWhiteSpace(x.ImageFilename))
			{
				context.Resources.GetBitmapAsync(x.ImageFilename).ContinueWith((t) =>
				{
					var bitmap = t.Result;
					if (bitmap != null)
					{
						view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(bitmap);
						bitmap.Dispose();
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());

			}
			else {
				// clear the image
				view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(null);
			}

			return view;
		}
	}

	public class NativeAndroidListViewRenderer : ViewRenderer<NativeListView2, global::Android.Widget.ListView>
	{
		public NativeAndroidListViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView2> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new global::Android.Widget.ListView(Forms.Context));
			}

			if (e.OldElement != null)
			{
				// unsubscribe
				Control.ItemClick -= Clicked;
			}

			if (e.NewElement != null)
			{
				// subscribe
				Control.Adapter = new NativeAndroidListViewAdapter(Forms.Context as Activity, e.NewElement);
				Control.ItemClick += Clicked;
			}
		}

		//		public override void Layout (int l, int t, int r, int b)
		//		{
		//			base.Layout (l, t, r, b);
		//		}

		void Clicked(object sender, AdapterView.ItemClickEventArgs e)
		{
			Element.NotifyItemSelected(Element.Items.ToList()[e.Position]);
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource

				Control.Adapter = new NativeAndroidListViewAdapter(Forms.Context as Activity, Element);
			}
		}
	}

	/// <summary>
	/// This adapter uses a view defined in /Resources/Layout/NativeAndroidListViewCell.axml
	/// as the cell layout
	/// </summary>
	public class NativeAndroidListViewAdapter : BaseAdapter<DataSource>
	{
		readonly Activity _context;
		IList<DataSource> _tableItems = new List<DataSource>();

		public IEnumerable<DataSource> Items
		{
			set
			{
				_tableItems = value.ToList();
			}
		}

		public NativeAndroidListViewAdapter(Activity context, NativeListView2 view)
		{
			_context = context;
			_tableItems = view.Items.ToList();
		}

		public override DataSource this[int position]
		{
			get
			{
				return _tableItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return _tableItems.Count; }
		}

		public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
		{
			var item = _tableItems[position];

			var view = convertView;
			if (view == null)
			{// no view to re-use, create new
				view = _context.LayoutInflater.Inflate(Resource.Layout.NativeAndroidListViewCell, null);
			}
			else { // re-use, clear image
				   // doesn't seem to help
				   //view.FindViewById<ImageView> (Resource.Id.Image).Drawable.Dispose ();
			}
			view.FindViewById<TextView>(Resource.Id.Text1).Text = item.Name;
			view.FindViewById<TextView>(Resource.Id.Text2).Text = item.Category;

			// grab the old image and dispose of it
			// TODO: optimize if the image is the *same* and we want to just keep it
			if (view.FindViewById<ImageView>(Resource.Id.Image).Drawable != null)
			{
				using (var image = view.FindViewById<ImageView>(Resource.Id.Image).Drawable as BitmapDrawable)
				{
					if (image != null)
					{
						if (image.Bitmap != null)
						{
							//image.Bitmap.Recycle ();
							image.Bitmap.Dispose();
						}
					}
				}
			}

			// If a new image is required, display it
			if (!string.IsNullOrWhiteSpace(item.ImageFilename))
			{
				_context.Resources.GetBitmapAsync(item.ImageFilename).ContinueWith((t) =>
				{
					var bitmap = t.Result;
					if (bitmap != null)
					{
						view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(bitmap);
						bitmap.Dispose();
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
			else {
				// clear the image
				view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(null);
			}

			return view;
		}
	}
	public class CustomContentRenderer : ViewRenderer
	{
	}

	public class CustomNativeButton : AButton
	{
		public CustomNativeButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public CustomNativeButton(Context context) : base(context)
		{
		}

		public CustomNativeButton(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public CustomNativeButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		public CustomNativeButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
		}
	}

	public class CustomButtonRenderer : ButtonRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (Control == null)
			{
				CustomNativeButton b = new CustomNativeButton(Context);
				SetNativeControl(b);
			}

			base.OnElementChanged(e);
		}
	}
}

