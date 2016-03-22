using System;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.UITests
{
	public class AndroidUiTestType
	{
		public AndroidUiTestType ()
		{
				
		}
	}

	public static class PlatformQueries
	{
		public static Func<AppQuery, AppQuery> AbsoluteGalleryBackground = q => q.Raw ("view:'Xamarin_Forms_Platform_iOS_BoxRenderer' parent view index:0");
		public static Func<AppQuery, AppQuery> ActivityIndicators = q => q.Raw ("ActivityIndicatorView");
		public static Func<AppQuery, AppQuery> Back = q => q.Raw ("view:'UINavigationItemButtonView'");
		public static Func<AppQuery, AppQuery> BoxRendererQuery = q => q.Raw ("view:'Xamarin_Forms_Platform_iOS_BoxRenderer'");
		public static Func<AppQuery, AppQuery> Cells = q => q.Raw ("TableViewCell");
		public static Func<AppQuery, AppQuery> DismissPickerCustom = q => q.Marked ("Done");
		public static Func<AppQuery, AppQuery> DismissPickerNormal = q => q.Marked ("Done");
		public static Func<AppQuery, AppQuery> Entrys = q => q.Raw ("TextField");
		public static Func<AppQuery, AppQuery> Editors = q => q.Raw ("TextView");
		public static Func<AppQuery, AppQuery> Frames = q => q.Raw ("view:'Xamarin_Forms_Platform_iOS_FrameRenderer'");
		public static Func<AppQuery, AppQuery> Images = q => q.Raw ("view:'Xamarin_Forms_Platform_iOS_ImageRenderer'");
		public static Func<AppQuery, AppQuery> ImageView = q => q.Raw ("ImageView");
		public static Func<AppQuery, AppQuery> Labels = q => q.Raw ("Label");
		public static Func<AppQuery, AppQuery> LabelRenderers = q => q.Raw ("view:'Xamarin_Forms_Platform_iOS_LabelRenderer'");
		public static Func<AppQuery, AppQuery> List = q => q.Raw ("TableView");
		public static Func<AppQuery, AppQuery> Map = q => q.Raw ("view:'MKMapView'");
		public static Func<AppQuery, AppQuery> MapPins = q => q.Raw ("all view:'MKPinAnnotationView'");
		public static Func<AppQuery, AppQuery> NavigationBar = q => q.Raw ("NavigationBar");
		public static Func<AppQuery, AppQuery> NumberPicker = q => q.Raw ("PickerTableView");
		public static Func<AppQuery, AppQuery> ProgressBar = q => q.Raw ("ProgressView");
		public static Func<AppQuery, AppQuery> SearchBars = q => q.Raw ("SearchBar");
		public static Func<AppQuery, AppQuery> Sliders = q => q.Raw ("Slider");
		public static Func<AppQuery, AppQuery> Steppers = q => q.Raw ("Stepper");
		public static Func<AppQuery, AppQuery> Switch = q => q.Raw ("Switch");
		public static Func<AppQuery, AppQuery> Tables = q => q.Raw ("TableView");
		public static Func<AppQuery, AppQuery> ThreeXThreeGridCell = q => q.Raw ("view marked:'a block 3x3' parent view:'Xamarin_Forms_Platform_iOS_LabelRenderer'");
		public static Func<AppQuery, AppQuery> SpanningThreeRows = q => q.Raw ("view marked:'Spanning 3 rows' parent view:'Xamarin_Forms_Platform_iOS_LabelRenderer'");

		public static Func<AppQuery, AppQuery> EntryWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("TextField placeholder:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryWithText (string text) {
			return q => q.Raw (string.Format ("TextField text:'{0}'", text));
		}

		public static Func<AppQuery, AppQuery> EntryCellWithPlaceholder (string text) {
			return q => q.Raw (string.Format ("TextField placeholder:'{0}'", text));
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

		public static AppResult DetailPage (this iOSApp app) 
		{
			if (app.Query (q => q.Raw ("view:'UILayoutContainerView'")).Length == 3) {
				// iPad SplitView Landscape
				return app.Query (q => q.Raw ("view:'UILayoutContainerView'"))[2];
			} 
			return app.Query (q => q.Raw ("*"))[0];
		}

	}
}
