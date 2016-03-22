using System;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.UITests
{
	public class iOSUiTestType
	{
		public iOSUiTestType ()
		{
				
		}
	}

	public static class PlatformQueries
	{
		public static Func<AppQuery, AppQuery> AbsoluteGalleryBackground = q => q.Raw ("xamarin.forms.platform.android.BoxRenderer parent xamarin.forms.platform.android.RendererFactory_DefaultRenderer index:0");
		public static Func<AppQuery, AppQuery> ActivityIndicators = q => q.Raw ("ProgressBar");
		public static Func<AppQuery, AppQuery> Back = q => q.Id ("up");
		public static Func<AppQuery, AppQuery> BoxRendererQuery = q => q.Raw ("xamarin.forms.platform.android.BoxRenderer");
		public static Func<AppQuery, AppQuery> Cells = q => q.Raw ("xamarin.forms.platform.android.ViewCellRenderer_ViewCellContainer");
		public static Func<AppQuery, AppQuery> DismissPickerCustom = q => q.Marked ("OK");
		public static Func<AppQuery, AppQuery> DismissPickerNormal = q => q.Marked ("Done");
		public static Func<AppQuery, AppQuery> Entrys = q => q.Raw ("EntryEditText");
		public static Func<AppQuery, AppQuery> EntryCells = q => q.Raw ("EntryCellEditText");
		public static Func<AppQuery, AppQuery> Editors = q => q.Raw ("EditorEditText");
		public static Func<AppQuery, AppQuery> Frames = q => q.Raw ("FrameRenderer");
		public static Func<AppQuery, AppQuery> Images = q => q.Raw ("xamarin.forms.platform.android.ImageRenderer");
		public static Func<AppQuery, AppQuery> ImageView = q => q.Raw("ImageView");
		public static Func<AppQuery, AppQuery> LabelRenderers = q => q.Raw ("LabelRenderer");
		public static Func<AppQuery, AppQuery> List = q => q.Raw ("ListView");
		public static Func<AppQuery, AppQuery> Labels = q => q.Raw ("TextView");
		public static Func<AppQuery, AppQuery> Map = q => q.Raw ("MapView");
		public static Func<AppQuery, AppQuery> NumberPicker = q => q.Raw ("NumberPicker");
		public static Func<AppQuery, AppQuery> ProgressBar = q => q.Raw ("ProgressBar");
		public static Func<AppQuery, AppQuery> Tables = q => q.Raw ("ListView");
		public static Func<AppQuery, AppQuery> SearchBars = q => q.Raw ("SearchView");
		public static Func<AppQuery, AppQuery> Sliders = q => q.Raw ("SeekBar");
		public static Func<AppQuery, AppQuery> SpanningThreeRows = q => q.Marked ("Spanning 3 rows");	
		public static Func<AppQuery, AppQuery> Steppers = q => q.Raw ("button marked:'+'");
		public static Func<AppQuery, AppQuery> Switch = q => q.Raw("Switch");
		public static Func<AppQuery, AppQuery> ThreeXThreeGridCell = q => q.Marked ("a block 3x3");	

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

		public static AppResult DetailPage (this AndroidApp app)
		{
			return app.Query (q => q.Raw ("*"))[0];
		}
	}
}
