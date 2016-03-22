using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal static class AndroidLoaderIdentifier {}

	internal static class PlatformMethodQueries
	{
		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> PropertyPlatformMethodDictionary = new Dictionary<BindableProperty, Tuple<string[], bool>> {
			{ ActivityIndicator.ColorProperty, Tuple.Create (new[] { "getProgressDrawable", "getColor" }, false) },
			{ ActivityIndicator.IsRunningProperty, Tuple.Create (new[] { "isIndeterminate" }, false) },
			{ Button.BorderColorProperty, Tuple.Create (new[] { "getBackground" }, false) },
			{ Button.BorderRadiusProperty, Tuple.Create (new[] { "getBackground" }, false) },
			{ Button.BorderWidthProperty, Tuple.Create (new[] { "getBackground" }, false) },
			{ Button.ImageProperty, Tuple.Create (new[] { "getBackground" }, false) },
			{ Button.FontProperty, Tuple.Create (new[] { "getTypeface", "isBold" }, false) },
			{ Button.TextProperty, Tuple.Create (new[] { "getText" }, false) },
			{ Button.TextColorProperty, Tuple.Create (new[] { "getCurrentTextColor" }, false) },
			{ View.AnchorXProperty, Tuple.Create (new[] { "getPivotX" }, true) },
			{ View.AnchorYProperty, Tuple.Create (new[] { "getPivotY" }, true) },
			{ View.BackgroundColorProperty, Tuple.Create (new[] { "getBackground", "getColor" }, true) },
			{ View.IsEnabledProperty, Tuple.Create (new[] { "isEnabled" }, false) },
			{ View.OpacityProperty, Tuple.Create (new[] { "getAlpha" }, true) },
			{ View.RotationProperty, Tuple.Create (new[] { "getRotation" }, true) },
			{ View.RotationXProperty, Tuple.Create (new[] { "getRotationX" }, true) },
			{ View.RotationYProperty, Tuple.Create (new[] { "getRotationY" }, true) },
			{ View.ScaleProperty, Tuple.Create (new[] { "getScaleX", "getScaleY" }, true) },
		};
	}

	internal static class PlatformViews
	{
		public static readonly string ActivityIndicator = "android.widget.ProgressBar";
		public static readonly string BoxView = "xamarin.forms.platform.android.BoxRenderer";
		public static readonly string Button = "android.widget.Button";
		public static readonly string DatePicker = "android.widget.EditText";
		public static readonly string Editor = "xamarin.forms.platform.android.EditorEditText";
		public static readonly string Entry = "xamarin.forms.platform.android.EntryEditText";
		public static readonly string Frame = "xamarin.forms.platform.android.appcompat.FrameRenderer";
		public static readonly string Image = "android.widget.ImageView";
		public static readonly string Label = "android.widget.TextView";
		public static readonly string ListView = "android.widget.ListView";
		public static readonly string OpenGLView = "android.widget.GLSurfaceView";
		public static readonly string Picker = "android.widget.EditText";
		public static readonly string ProgressBar = "android.widget.ProgressBar";
		public static readonly string SearchBar = "android.widget.SearchView";
		public static readonly string Slider = "android.widget.SeekBar";
		public static readonly string Stepper = "button marked:'+'";
		public static readonly string Switch = "android.widget.Switch";
		public static readonly string TableView = "android.widget.ListView";
		public static readonly string TimePicker = "android.widget.EditText";
		public static readonly string WebView = "android.widget.WebView";
	}

	internal static class PlatformQueries
	{
		public static readonly Func<AppQuery, AppQuery> Root = q => q.Id ("content");
		public static readonly Func<AppQuery, AppQuery> RootPageListView = q => q.Raw ("ListViewRenderer index:0");
		public static readonly Func<AppQuery, AppQuery> GalleryListView = q => q.Raw ("ListViewRenderer index:1");
		public static readonly Func<AppQuery, AppQuery> PageWithoutNavigationBar = q => q.Raw ("* id:'content' index:0");
		public static readonly Func<AppQuery, AppQuery> NavigationBarBackButton = q => q.Class ("android.support.v7.widget.Toolbar").Child ("android.widget.ImageButton");

		// Views
		public static readonly Func<AppQuery, AppQuery> ActivityIndicator = q => q.ClassFull (PlatformViews.ActivityIndicator);
		public static readonly Func<AppQuery, AppQuery> Button = q => q.ClassFull (PlatformViews.Button);

		public static Func<AppQuery, AppQuery> EntryWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("EntryEditText hint:'{0}'", text));
		}
		public static Func<AppQuery, AppQuery> EntryCellWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("EntryCellEditText hint:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithText (string text) {
			return q => q.Raw (string.Format ("EntryEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithText (string text) {
			return q => q.Raw (string.Format ("EntryCellEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EditorsWithText (string text) {
			return q => q.Raw (string.Format ("EditorEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithIndex (int index) {
			return q => q.Raw (string.Format ("EntryEditText index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> SearchBarWithIndex (int index) {
			return q => q.Raw (string.Format ("SearchView index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithIndex (int index) {
			return q => q.Raw (string.Format ("TextView index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithText (string text) {
			return q => q.Raw (string.Format ("TextView text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> LabelWithId (string id) {
			return q => q.Raw (string.Format ("TextView id:'{0}'", id));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithIndex (int index) {
			return q => q.Raw (string.Format ("EditText index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithPlaceholder (string placeholder) {
			return q => q.Raw (string.Format ("EditText hint:'{0}'", placeholder));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithText (string text) {
			return q => q.Raw (string.Format ("EditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> SwitchWithIndex (int index) {
			return q => q.Raw (string.Format ("Switch index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> StepperWithIndex (int index) {
			return q => q.Raw (string.Format ("button marked:'+' index:{0}", index));
		}
	}
}