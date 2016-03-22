using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal static class iOSLoaderIdentifier
	{
		
	}

    internal static class PlatformMethodQueries
    {
        public static readonly Dictionary<BindableProperty, Tuple<string[], bool>> PropertyPlatformMethodDictionary = new Dictionary<BindableProperty, Tuple<string[], bool>> {
			{ ActivityIndicator.ColorProperty, Tuple.Create (new[] { "color" }, false) },
			{ ActivityIndicator.IsRunningProperty, Tuple.Create (new[] { "isAnimating" }, false) },
			{ Button.BorderRadiusProperty, Tuple.Create (new[] { "layer", "cornerRadius" }, false) },
			{ Button.BorderWidthProperty, Tuple.Create (new[] { "layer", "borderWidth" }, false) },
			{ Button.FontProperty, Tuple.Create (new[] { "titleLabel", "font" }, false) },
			{ Button.TextProperty, Tuple.Create (new[] { "titleLabel", "text" }, false) },
			{ Button.TextColorProperty, Tuple.Create (new[] { "titleLabel", "textColor" }, false) },    
			{ View.AnchorXProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
			{ View.AnchorYProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
			{ View.BackgroundColorProperty, Tuple.Create (new[] { "backgroundColor" }, false) },
			{ View.IsEnabledProperty, Tuple.Create (new[] { "isEnabled" }, false) },
			{ View.OpacityProperty, Tuple.Create (new [] { "alpha" }, true) },
            { View.RotationProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
            { View.RotationXProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
            { View.RotationYProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
			{ View.ScaleProperty, Tuple.Create (new[] { "layer", "transform" }, true) },
        };
    }

	internal static class PlatformViews
	{
		public static readonly string ActivityIndicator = "UIActivityIndicatorView";
		public static readonly string BoxView = "Xamarin_Forms_Platform_iOS_BoxRenderer";
		public static readonly string Button = "UIButton";
		public static readonly string DatePicker = "UITextField";
		public static readonly string Editor = "UITextView";
		public static readonly string Entry = "UITextField";
		public static readonly string Frame = "view:'Xamarin_Forms_Platform_iOS_FrameRenderer'";
		public static readonly string Image = "UIImageView";
		public static readonly string Label = "UILabel";
		public static readonly string ListView = "UITableView";
		public static readonly string OpenGLView = "GLKView";
		public static readonly string Picker = "UITextField";
		public static readonly string ProgressBar = "UIProgressView";
		public static readonly string SearchBar = "UISearchBar";
		public static readonly string Slider = "UISlider";
		public static readonly string Stepper = "UIStepper";
		public static readonly string Switch = "UISwitch";
		public static readonly string TableView = "UITableView";
		public static readonly string TimePicker = "UITextField";
		public static readonly string WebView = "UIWebView";
	}

	internal static class PlatformQueries
	{
		public static readonly Func<AppQuery, AppQuery> Root = q => q.Class ("UIWindow");
		public static readonly Func<AppQuery, AppQuery> RootPageListView = q => q.Class ("Xamarin_Forms_Platform_iOS_ListViewRenderer index:0");
		public static readonly Func<AppQuery, AppQuery> GalleryListView = q => q.Class ("Xamarin_Forms_Platform_iOS_ListViewRenderer index:1");
		public static readonly Func<AppQuery, AppQuery> PageWithoutNavigationBar = q => q.Raw ("*").Index (7);
		public static readonly Func<AppQuery, AppQuery> NavigationBarBackButton = q => q.Class ("UINavigationItemButtonView");

		// Controls
		public static readonly Func<AppQuery, AppQuery> ActivityIndicator = q => q.ClassFull (PlatformViews.ActivityIndicator);
		public static readonly Func<AppQuery, AppQuery> Button = q => q.ClassFull (PlatformViews.Button);

		public static Func<AppQuery, AppQuery> EntryWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("TextField placeholder:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithText (string text) {
			return q => q.Raw (string.Format ("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("UITextFieldLabel text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithText (string text) {
			return q => q.Raw (string.Format ("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EditorsWithText (string text) {
			return q => q.Raw (string.Format ("TextView text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithIndex (int index) {
			return q => q.Raw (string.Format ("TextField index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> SearchBarWithIndex (int index) {
			return q => q.Raw (string.Format ("SearchBar index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithIndex (int index) {
			return q => q.Raw (string.Format ("Label index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> LabelWithText (string text) {
			return q => q.Raw (string.Format ("Label text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> LabelWithId (string id) {
			return q => q.Raw (string.Format ("Label id:'{0}'", id));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithIndex (int index) {
			return q => q.Raw (string.Format ("TextField index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithPlaceholder (string placeholder) {
			return q => q.Raw (string.Format ("TextField placeholder:'{0}'", placeholder));
		}

		public static Func<AppQuery, AppQuery> PickerEntryWithText (string text) {
			return q => q.Raw (string.Format ("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> SwitchWithIndex (int index) {
			return q => q.Raw (string.Format ("Switch index:{0}", index));
		}

		public static Func<AppQuery, AppQuery> StepperWithIndex (int index) {
			return q => q.Raw (string.Format ("Stepper index:{0}", index));
		}

	}
}
