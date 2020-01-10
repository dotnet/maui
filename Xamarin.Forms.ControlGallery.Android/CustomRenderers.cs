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
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using Android.OS;
using System.Reflection;
using Android.Text;
using Android.Text.Method;
using Xamarin.Forms.Controls.Issues;
#if __ANDROID_29__
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using NestedScrollView = global::AndroidX.Core.Widget.NestedScrollView;
#else
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using NestedScrollView = global::Android.Support.V4.Widget.NestedScrollView;
#endif

[assembly: ExportRenderer(typeof(Issue5461.ScrollbarFadingEnabledFalseScrollView), typeof(ScrollbarFadingEnabledFalseScrollViewRenderer))]
[assembly: ExportRenderer(typeof(Issue1942.CustomGrid), typeof(Issue1942GridRenderer))]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.Effects.AttachedStateEffectLabel), typeof(AttachedStateEffectLabelRenderer))]
[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.LegacyComponents.NonAppCompatSwitch), typeof(NonAppCompatSwitchRenderer))]
[assembly: ExportRenderer(typeof(Bugzilla31395.CustomContentView), typeof(CustomContentRenderer))]
[assembly: ExportRenderer(typeof(NativeListView), typeof(NativeListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeListView2), typeof(NativeAndroidListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeCell), typeof(NativeAndroidCellRenderer))]

[assembly: ExportRenderer(typeof(Bugzilla42000._42000NumericEntryNoDecimal), typeof(EntryRendererNoDecimal))]
[assembly: ExportRenderer(typeof(Bugzilla42000._42000NumericEntryNoNegative), typeof(EntryRendererNoNegative))]
[assembly: ExportRenderer(typeof(Issue1683.EntryKeyboardFlags), typeof(EntryRendererKeyboardFlags))]
[assembly: ExportRenderer(typeof(Issue1683.EditorKeyboardFlags), typeof(EditorRendererKeyboardFlags))]
//[assembly: ExportRenderer(typeof(AndroidHelpText.HintLabel), typeof(HintLabel))]
[assembly: ExportRenderer(typeof(QuickCollectNavigationPage), typeof(QuickCollectNavigationPageRenderer))]
[assembly: ExportRenderer(typeof(Issue4782.Issue4782ImageButton), typeof(Issue4782ImageButtonImageButtonRenderer))]
[assembly: ExportRenderer(typeof(Issue4561.CustomView), typeof(Issue4561CustomViewRenderer))]

[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.Issues.NoFlashTestNavigationPage), typeof(Xamarin.Forms.ControlGallery.Android.NoFlashTestNavigationPage))]
[assembly: ExportRenderer(typeof(ShellGestures.TouchTestView), typeof(ShellGesturesTouchTestViewRenderer))]
[assembly: ExportRenderer(typeof(Issue7249Switch), typeof(Issue7249SwitchRenderer))]

#if PRE_APPLICATION_CLASS
#elif FORMS_APPLICATION_ACTIVITY
#else
[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(NativeDroidMasterDetail))]
#endif
namespace Xamarin.Forms.ControlGallery.Android
{
	public class NonAppCompatSwitchRenderer : Xamarin.Forms.Platform.Android.SwitchRenderer
	{
		public NonAppCompatSwitchRenderer(Context context) : base(context)
		{
		}
	}

	public class ScrollbarFadingEnabledFalseScrollViewRenderer : ScrollViewRenderer
	{
		public ScrollbarFadingEnabledFalseScrollViewRenderer(Context context) : base(context)
		{
			// I do a cast here just so this will fail just to be sure we don't change the base types
			var castingTest = (NestedScrollView)this;
			castingTest.ScrollbarFadingEnabled = false;
		}
	}

	public class AttachedStateEffectLabelRenderer :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.LabelRenderer
#else
		LabelRenderer
#endif
	{
		public AttachedStateEffectLabelRenderer(Context context) : base(context)
		{
		}

#if TEST_EXPERIMENTAL_RENDERERS
		protected override void Dispose(bool disposing)
		{
			foreach (var effect in Element.Effects.OfType<Controls.Effects.AttachedStateEffect>())
			{
				effect.Detached(Element);
			}

			base.Dispose(disposing);
		}
#endif
	}

	public class NativeDroidMasterDetail : Xamarin.Forms.Platform.Android.AppCompat.MasterDetailPageRenderer
	{
		MasterDetailPage _page;
		bool _disposed;

#pragma warning disable 618
		public NativeDroidMasterDetail()
#pragma warning restore 618
		{
			System.Diagnostics.Debug.WriteLine($">>>>> NativeDroidMasterDetail NativeDroidMasterDetail 53: This is the obsolete constructor being selected");
		}

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
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing && _page != null)
			{
				_page.LayoutChanged -= Page_LayoutChanged;
				_page.PropertyChanged -= Page_PropertyChanged;
				_page = null;
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
#pragma warning disable 618
		public NativeListViewRenderer()
#pragma warning restore 618
		{
		}

		protected override global::Android.Widget.ListView CreateNativeControl()
		{
#pragma warning disable 618
			// Disabled the warning so we have a test that this obsolete stuff still works
			return new global::Android.Widget.ListView(Forms.Context);
#pragma warning restore 618
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
			}

			if (e.OldElement != null)
			{
				// unsubscribe
				Control.ItemClick -= Clicked;
			}

			if (e.NewElement != null)
			{
				// subscribe

#pragma warning disable 618
				// Disabled the warning so we have a test that this obsolete stuff still works
				Control.Adapter = new NativeListViewAdapter(Forms.Context.GetActivity(), e.NewElement);
#pragma warning restore 618
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

#pragma warning disable 618
				// Disabled the warning so we have a test that this obsolete stuff still works
				Control.Adapter = new NativeListViewAdapter(Forms.Context.GetActivity(), Element);
#pragma warning restore 618
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
				view = (context.GetActivity()).LayoutInflater.Inflate(Resource.Layout.NativeAndroidCell, null);
			}
			else
			{ // re-use, clear image
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
			else
			{
				// clear the image
				view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(null);
			}

			return view;
		}
	}

	public class NativeAndroidListViewRenderer : ViewRenderer<NativeListView2, global::Android.Widget.ListView>
	{

#pragma warning disable 618
		public NativeAndroidListViewRenderer()
#pragma warning restore 618
		{
		}

		protected override global::Android.Widget.ListView CreateNativeControl()
		{
#pragma warning disable 618
			// Disabled the warning so we have a test that this obsolete stuff still works
			return new global::Android.Widget.ListView(Forms.Context);
#pragma warning restore 618
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView2> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
			}

			if (e.OldElement != null)
			{
				// unsubscribe
				Control.ItemClick -= Clicked;
			}

			if (e.NewElement != null)
			{
				// subscribe
#pragma warning disable 618
				// Disabled the warning so we have a test that this obsolete stuff still works
				Control.Adapter = new NativeAndroidListViewAdapter(Forms.Context.GetActivity(), e.NewElement);
#pragma warning restore 618
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

#pragma warning disable 618
				// Disabled the warning so we have a test that this obsolete stuff still works
				Control.Adapter = new NativeAndroidListViewAdapter(Forms.Context.GetActivity(), Element);
#pragma warning restore 618
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
			else
			{ // re-use, clear image
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
			else
			{
				// clear the image
				view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(null);
			}

			return view;
		}
	}

	[Preserve]
	public class CustomContentRenderer : ViewRenderer
	{
#pragma warning disable 618
		[Preserve]
		public CustomContentRenderer()
#pragma warning restore 618
		{
			AutoPackage = true;
		}

		protected override AView CreateNativeControl()
		{
			return new AView(Context);
		}
	}

	[Preserve]
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
		public CustomButtonRenderer(Context context) : base(context)
		{
		}

		protected override AButton CreateNativeControl()
		{
			return new CustomNativeButton(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (Control == null)
			{
				CustomNativeButton b = (CustomNativeButton)CreateNativeControl();
				SetNativeControl(b);
			}

			base.OnElementChanged(e);
		}
	}

	// Custom renderers for Bugzilla42000 demonstration purposes
	public class EntryRendererNoNegative : EntryRenderer
	{
		public EntryRendererNoNegative(Context context) : base(context)
		{
		}

		protected override NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Disable the NumberFlagSigned bit
			inputTypes &= ~InputTypes.NumberFlagSigned;

			return base.GetDigitsKeyListener(inputTypes);
		}
	}

	public class EntryRendererNoDecimal : EntryRenderer
	{
		public EntryRendererNoDecimal(Context context) : base(context)
		{
		}

		protected override NumberKeyListener GetDigitsKeyListener(InputTypes inputTypes)
		{
			// Disable the NumberFlagDecimal bit
			inputTypes &= ~InputTypes.NumberFlagDecimal;

			return base.GetDigitsKeyListener(inputTypes);
		}
	}

	public class EntryRendererKeyboardFlags : EntryRenderer
	{
		public EntryRendererKeyboardFlags(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var FlagsToSet = ((Issue1683.EntryKeyboardFlags)Element).FlagsToSet;
			var FlagsToTestFor = ((Issue1683.EntryKeyboardFlags)Element).FlagsToTestFor;

			base.OnElementPropertyChanged(sender, e);

			Control.SetKeyboardFlags(FlagsToSet);
			Control.TestKeyboardFlags(FlagsToTestFor);
		}
	}

	public class EditorRendererKeyboardFlags : EditorRenderer
	{
		public EditorRendererKeyboardFlags(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var FlagsToSet = ((Issue1683.EditorKeyboardFlags)Element).FlagsToSet;
			var FlagsToTestFor = ((Issue1683.EditorKeyboardFlags)Element).FlagsToTestFor;
			base.OnElementPropertyChanged(sender, e);

			Control.SetKeyboardFlags(FlagsToSet);
			Control.TestKeyboardFlags(FlagsToTestFor);
		}
	}

	public class Issue4782ImageButtonImageButtonRenderer : ImageButtonRenderer
	{
		public Issue4782ImageButtonImageButtonRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
		{
			base.OnElementChanged(e);
			SetImageDrawable(null);
		}
	}

	public class Issue1942GridRenderer : VisualElementRenderer<Grid>, AView.IOnTouchListener, ViewTreeObserver.IOnGlobalLayoutListener
	{
		AView _gridChild;
		public Issue1942GridRenderer(Context context) : base(context)
		{
		}

		bool AView.IOnTouchListener.OnTouch(AView v, MotionEvent e)
		{
			((Element.Children.First() as Layout).Children.First() as Label).Text = Issue1942.SuccessString;
			ViewGroup.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
			_gridChild.SetOnTouchListener(null);
			return true;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Grid> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				ViewGroup.ViewTreeObserver.AddOnGlobalLayoutListener(this);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ViewGroup.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
				_gridChild.SetOnTouchListener(null);
				_gridChild = null;
			}

			base.Dispose(disposing);
		}

		void ViewTreeObserver.IOnGlobalLayoutListener.OnGlobalLayout()
		{
			_gridChild = ViewGroup.GetChildAt(0);
			_gridChild.SetOnTouchListener(this);
		}
	}

	[Preserve]
	public class Issue4561CustomView : LinearLayout
	{
		public Issue4561CustomView(Context context)
			: base(context)
		{
			Initialize();
		}

		public Issue4561CustomView(Context context, IAttributeSet attrs) :
			base(context, attrs)
		{
			Initialize();
		}

		public Issue4561CustomView(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle)
		{
			Initialize();
		}

		void Initialize()
		{
			var editText1 = new EditText(Context)
			{
				InputType = InputTypes.NumberFlagDecimal,
				Id = 12345,
				Text = "customEdit1"
			};
			var editText2 = new EditText(Context)
			{
				InputType = InputTypes.NumberFlagDecimal,
				Id = 123456,
				Text = "customEdit2"
			};

			editText1.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			editText2.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);

			editText1.NextFocusForwardId = editText2.Id;
			editText2.NextFocusForwardId = editText1.Id;

			AddView(editText1);
			AddView(editText2);

			Orientation = Orientation.Vertical;
		}
	}

	[Preserve]
	public class Issue4561CustomViewRenderer : ViewRenderer<Issue4561.CustomView, Issue4561CustomView>
	{
		public Issue4561CustomViewRenderer(Context context) : base(context)
		{
		}

		protected override Issue4561CustomView CreateNativeControl() => new Issue4561CustomView(Context);

		protected override void OnElementChanged(ElementChangedEventArgs<Issue4561.CustomView> e)
		{
			base.OnElementChanged(e);
			if (Element != null)
			{
				if (Control == null)
				{
					var view = CreateNativeControl();
					SetNativeControl(view);
				}
			}
		}
	}

	public static class KeyboardFlagExtensions
	{
		public static void TestKeyboardFlags(this FormsEditText Control, KeyboardFlags? flags)
		{
			if (flags == null)
			{
				return;
			}
			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				if (!Control.InputType.HasFlag(InputTypes.TextFlagCapSentences))
				{
					throw new Exception("TextFlagCapSentences not correctly set");
				}
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				if (!Control.InputType.HasFlag(InputTypes.TextFlagCapCharacters))
				{
					throw new Exception("TextFlagCapCharacters not correctly set");
				}
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				if (!Control.InputType.HasFlag(InputTypes.TextFlagCapWords))
				{
					throw new Exception("TextFlagCapWords not correctly set");
				}
			}
		}

		public static void SetKeyboardFlags(this FormsEditText Control, KeyboardFlags? flags)
		{
			if (flags == null)
			{
				return;
			}

			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				Control.InputType = Control.InputType | InputTypes.TextFlagCapCharacters;
			}

			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				Control.InputType = Control.InputType | InputTypes.TextFlagCapSentences;
			}

			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				Control.InputType = Control.InputType | InputTypes.TextFlagCapWords;
			}
		}
	}


	//public class HintLabel : Xamarin.Forms.Platform.Android.AppCompat.LabelRenderer
	//{
	//	public HintLabel()
	//	{
	//		Hint = AndroidHelpText.HintLabel.Success;
	//	}
	// }

#pragma warning disable CS0618 // Leaving in old constructor so we can verify it works
	public class NoFlashTestNavigationPage
#if FORMS_APPLICATION_ACTIVITY
		: Xamarin.Forms.Platform.Android.NavigationRenderer
#else
		: Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
#endif
	{
#if !FORMS_APPLICATION_ACTIVITY
		protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
		{
			transaction.SetTransition((int)FragmentTransit.None);
		}
#endif
	}
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Leaving in old constructor so we can verify it works
	public class QuickCollectNavigationPageRenderer
#if FORMS_APPLICATION_ACTIVITY
		: Xamarin.Forms.Platform.Android.NavigationRenderer
#else
		: Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
#endif
	{
		bool _disposed;
		NavigationPage _page;

		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
			{
				if (e.OldElement != null)
				{
					((IPageController)e.OldElement).InternalChildren.CollectionChanged -= OnInternalPageCollectionChanged;
				}

				return;
			}

			((IPageController)e.NewElement).InternalChildren.CollectionChanged += OnInternalPageCollectionChanged;
		}

		private void OnInternalPageCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				// Force a collection on popped to simulate the problem.
				GC.Collect();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing && _page != null)
			{
				_page.InternalChildren.CollectionChanged -= OnInternalPageCollectionChanged;
				_page = null;
			}

			base.Dispose(disposing);
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete


	public class ShellGesturesTouchTestViewRenderer : ViewRenderer<ShellGestures.TouchTestView, global::Android.Views.View>, AView.IOnTouchListener
	{
		global::Android.Graphics.Paint paint;

		public List<Point> pointList = new List<Point>();
		public ShellGesturesTouchTestViewRenderer(Context context) : base(context)
		{
		}

		public bool OnTouch(global::Android.Views.View v, MotionEvent e)
		{
			switch (e.Action)
			{
				case MotionEventActions.Up:
					Element.Results.Text = Xamarin.Forms.Controls.Issues.ShellGestures.TouchListenerSuccess;
					break;
				case MotionEventActions.Cancel:
					Element.Results.Text = "Fail";
					break;
			}
			return true;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ShellGestures.TouchTestView> e)
		{
			base.OnElementChanged(e);
			paint = new global::Android.Graphics.Paint();
			if (e.NewElement != null)
				SetOnTouchListener(this);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if(disposing)
				SetOnTouchListener(null);

			paint = null;
		}
	}
}

