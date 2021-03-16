using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	internal static class PlatformMethodQueries
	{
#if __IOS__ || __MACOS__
		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> PropertyPlatformMethodDictionary = new Dictionary<BindableProperty, Tuple<string[], bool>> {
			{ ActivityIndicator.ColorProperty, Tuple.Create (new[] { "color" }, false) },
			{ ActivityIndicator.IsRunningProperty, Tuple.Create (new[] { "isAnimating" }, false) },
			{ Button.CornerRadiusProperty, Tuple.Create (new[] { "layer", "cornerRadius" }, false) },
			{ Button.BorderWidthProperty, Tuple.Create (new[] { "layer", "borderWidth" }, false) },
			{ Button.FontProperty, Tuple.Create (new[] { "titleLabel", "font" }, false) },
			{ Button.TextProperty, Tuple.Create (new[] { "titleLabel", "text" }, false) },
			{ Button.TextColorProperty, Tuple.Create (new[] { "titleLabel", "textColor" }, false) },
			{ ImageButton.CornerRadiusProperty, Tuple.Create (new[] { "layer", "cornerRadius" }, false) },
			{ ImageButton.BorderWidthProperty, Tuple.Create (new[] { "layer", "borderWidth" }, false) },
			{ View.AnchorXProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
			{ View.AnchorYProperty, Tuple.Create (new[] { "lgetLayerTransformString" }, true) },
			{ View.BackgroundColorProperty, Tuple.Create (new[] { "backgroundColor" }, false) },
			{ View.IsEnabledProperty, Tuple.Create (new[] { "isEnabled" }, false) },
			{ View.OpacityProperty, Tuple.Create (new [] { "alpha" }, true) },
			{ View.RotationProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
			{ View.RotationXProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
			{ View.RotationYProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
			{ View.ScaleProperty, Tuple.Create (new[] { "getLayerTransformString" }, true) },
		};

#elif __ANDROID__ || __WINDOWS__
		public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> PropertyPlatformMethodDictionary = new Dictionary
			<BindableProperty, Tuple<string[], bool>>
			{
				{ ActivityIndicator.ColorProperty, Tuple.Create(new[] { "getProgressDrawable", "getColor" }, false) },
				{ ActivityIndicator.IsRunningProperty, Tuple.Create(new[] { "isIndeterminate" }, false) },
				{ BorderElement.BorderColorProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.ImageSourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ Button.FontProperty, Tuple.Create(new[] { "getTypeface", "isBold" }, false) },
				{ Button.TextProperty, Tuple.Create(new[] { "getText" }, false) },
				{ Button.TextColorProperty, Tuple.Create(new[] { "getCurrentTextColor" }, false) },
				{ ImageButton.CornerRadiusProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.BorderWidthProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ ImageButton.SourceProperty, Tuple.Create(new[] { "getBackground" }, false) },
				{ View.AnchorXProperty, Tuple.Create(new[] { "getPivotX" }, true) },
				{ View.AnchorYProperty, Tuple.Create(new[] { "getPivotY" }, true) },
				{ View.BackgroundColorProperty, Tuple.Create(new[] { "getBackground", "getColor" }, true) },
				{ View.IsEnabledProperty, Tuple.Create(new[] { "isEnabled" }, false) },
				{ View.OpacityProperty, Tuple.Create(new[] { "getAlpha" }, true) },
				{ View.RotationProperty, Tuple.Create(new[] { "getRotation" }, true) },
				{ View.RotationXProperty, Tuple.Create(new[] { "getRotationX" }, true) },
				{ View.RotationYProperty, Tuple.Create(new[] { "getRotationY" }, true) },
				{ View.ScaleProperty, Tuple.Create(new[] { "getScaleX", "getScaleY" }, true) },
			};
#endif
	}

	internal static class PlatformViews
	{
#if __IOS__ || __MACOS__
		public static readonly string ActivityIndicator = "UIActivityIndicatorView";
		public static readonly string BoxView = "Xamarin_Forms_Platform_iOS_BoxRenderer";
		public static readonly string Button = "UIButton";
		public static readonly string CheckBox = "Xamarin_Forms_Platform_iOS_CheckBoxRenderer";
		public static readonly string DatePicker = "UITextField";
		public static readonly string Editor = "UITextView";
		public static readonly string Entry = "UITextField";
		public static readonly string Frame = "view:'Xamarin_Forms_Platform_iOS_FrameRenderer'";
		public static readonly string Image = "UIImageView";
		public static readonly string ImageButton = "UIButton";
		public static readonly string Label = "UILabel";
		public static readonly string ListView = "UITableView";
		public static readonly string Map = "MKMapView";
		public static readonly string OpenGLView = "GLKView";
		public static readonly string Picker = "UITextField";
		public static readonly string Pin = "MKPinAnnotationView";
		public static readonly string ProgressBar = "UIProgressView";
		public static readonly string RadioButton = "Xamarin_Forms_Platform_iOS_RadioButtonRenderer";
		public static readonly string SearchBar = "UISearchBar";
		public static readonly string Slider = "UISlider";
		public static readonly string Stepper = "UIStepper";
		public static readonly string Switch = "UISwitch";
		public static readonly string TableView = "UITableView";
		public static readonly string TimePicker = "UITextField";
		public static readonly string WebView = "WKWebView";
#elif __ANDROID__ || __WINDOWS__
		public static readonly string ActivityIndicator = "android.widget.ProgressBar";
		public static readonly string BoxView = "xamarin.forms.platform.android.BoxRenderer";
		public static readonly string Button = "android.widget.Button";
		public static readonly string CheckBox = "android.widget.CheckBox";
		public static readonly string DatePicker = "android.widget.EditText";
		public static readonly string Editor = "xamarin.forms.platform.android.EditorEditText";
		public static readonly string Entry = "xamarin.forms.platform.android.EntryEditText";
		public static readonly string Frame = "xamarin.forms.platform.android.appcompat.FrameRenderer";
		public static readonly string Image = "android.widget.ImageView";
		public static readonly string ImageButton = "android.widget.ImageButton";
		public static readonly string Label = "android.widget.TextView";
		public static readonly string ListView = "android.widget.ListView";
		public static readonly string Map = "android.gms.maps.GoogleMap";
		public static readonly string OpenGLView = "android.widget.GLSurfaceView";
		public static readonly string Picker = "android.widget.EditText";
		public static readonly string Pin = "android.gms.maps.model.Marker";
		public static readonly string ProgressBar = "android.widget.ProgressBar";
		public static readonly string RadioButton = "android.widget.RadioButton";
		public static readonly string SearchBar = "android.widget.SearchView";
		public static readonly string Slider = "android.widget.SeekBar";
		public static readonly string Stepper = "button marked:'+'";
		public static readonly string Switch = "android.widget.Switch";
		public static readonly string TableView = "android.widget.ListView";
		public static readonly string TimePicker = "android.widget.EditText";
		public static readonly string WebView = "android.widget.WebView";
#endif
	}

	internal static class PlatformQueries
	{
#if __IOS__ || __MACOS__
		public static readonly Func<AppQuery, AppQuery> Root = q => q.Class("UIWindow");
		public static readonly Func<AppQuery, AppQuery> RootPageListView = q => q.Class("Xamarin_Forms_Platform_iOS_ListViewRenderer index:0");
		public static readonly Func<AppQuery, AppQuery> GalleryListView = q => q.Class("Xamarin_Forms_Platform_iOS_ListViewRenderer index:1");
		public static readonly Func<AppQuery, AppQuery> PageWithoutNavigationBar = q => q.Raw("*").Index(7);
		public static readonly Func<AppQuery, AppQuery> NavigationBarBackButton = q => q.Class("UINavigationItemButtonView");

#elif __ANDROID__ || __WINDOWS__
		public static readonly Func<AppQuery, AppQuery> Root = q => q.Id("content");
		public static readonly Func<AppQuery, AppQuery> RootPageListView = q => q.Raw("ListViewRenderer index:0");
		public static readonly Func<AppQuery, AppQuery> GalleryListView = q => q.Raw("ListViewRenderer index:1");
		public static readonly Func<AppQuery, AppQuery> PageWithoutNavigationBar = q => q.Raw("* id:'content' index:0");

		public static readonly Func<AppQuery, AppQuery> NavigationBarBackButton =
			q => q.Class("Toolbar").Child("android.widget.ImageButton");
#endif

		// Controls
		public static readonly Func<AppQuery, AppQuery> ActivityIndicator = q => q.ClassFull(PlatformViews.ActivityIndicator);
		public static readonly Func<AppQuery, AppQuery> Button = q => q.ClassFull(PlatformViews.Button);
		public static readonly Func<AppQuery, AppQuery> Pin = q => q.ClassFull(PlatformViews.Pin);

#if __ANDROID__
		public static Func<AppQuery, AppQuery> EntryWithPlaceholder(string text)
		{
			return q => q.Raw(string.Format("EntryEditText hint:'{0}'", text));
		}
		public static Func<AppQuery, AppQuery> EntryCellWithPlaceholder(string text)
		{
			return q => q.Raw(string.Format("EntryCellEditText hint:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithText(string text)
		{
			return q => q.Raw(string.Format("EntryEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithText(string text)
		{
			return q => q.Raw(string.Format("EntryCellEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EditorsWithText(string text)
		{
			return q => q.Raw(string.Format("EditorEditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithIndex(int index)
		{
			return q => q.Raw(string.Format("EntryEditText index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> SearchBarWithIndex(int index)
		{
			return q => q.Raw(string.Format("SearchView index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithIndex(int index)
		{
			return q => q.Raw(string.Format("TextView index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithText(string text)
		{
			return q => q.Raw(string.Format("TextView text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> LabelWithId(string id)
		{
			return q => q.Raw(string.Format("TextView id:'{0}'", id));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithIndex(int index)
		{
			return q => q.Raw(string.Format("EditText index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithPlaceholder(string placeholder)
		{
			return q => q.Raw(string.Format("EditText hint:'{0}'", placeholder));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithText(string text)
		{
			return q => q.Raw(string.Format("EditText text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> SwitchWithIndex(int index)
		{
			return q => q.Raw(string.Format("Switch index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> StepperWithIndex(int index)
		{
			return q => q.Raw(string.Format("button marked:'+' index:{0}", index));
		}

#else
		public static Func<AppQuery, AppQuery> EntryWithPlaceholder(string text)
		{
			return q => q.Raw(string.Format("TextField placeholder:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithText(string text)
		{
			return q => q.Raw(string.Format("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithPlaceholder(string text)
		{
			return q => q.Raw(string.Format("UITextFieldLabel text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithText(string text)
		{
			return q => q.Raw(string.Format("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EditorsWithText(string text)
		{
			return q => q.Raw(string.Format("TextView text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithIndex(int index)
		{
			return q => q.Raw(string.Format("TextField index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> SearchBarWithIndex(int index)
		{
			return q => q.Raw(string.Format("SearchBar index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithIndex(int index)
		{
			return q => q.Raw(string.Format("Label index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithText(string text)
		{
			return q => q.Raw(string.Format("Label text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> LabelWithId(string id)
		{
			return q => q.Raw(string.Format("Label id:'{0}'", id));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithIndex(int index)
		{
			return q => q.Raw(string.Format("TextField index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithPlaceholder(string placeholder)
		{
			return q => q.Raw(string.Format("TextField placeholder:'{0}'", placeholder));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithText(string text)
		{
			return q => q.Raw(string.Format("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> SwitchWithIndex(int index)
		{
			return q => q.Raw(string.Format("Switch index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> StepperWithIndex(int index)
		{
			return q => q.Raw(string.Format("Stepper index:{0}", index));
		}
#endif

	}
}