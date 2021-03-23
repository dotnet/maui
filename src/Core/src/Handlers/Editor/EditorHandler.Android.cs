using Android.Content.Res;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : AbstractViewHandler<IEditor, AppCompatEditText>
	{
		static ColorStateList? DefaultPlaceholderTextColors { get; set; }

		protected override AppCompatEditText CreateNativeView()
		{
			var editText = new AppCompatEditText(Context)
			{
				ImeOptions = ImeAction.Done
			};

			editText.SetSingleLine(false);
			editText.Gravity = GravityFlags.Top;
			editText.TextAlignment = Android.Views.TextAlignment.ViewStart;
			editText.SetHorizontallyScrolling(false);

			return editText;
		}

		protected override void SetupDefaults(AppCompatEditText nativeView)
		{
			base.SetupDefaults(nativeView);
			DefaultPlaceholderTextColors = nativeView.HintTextColors;
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateText(editor);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdatePlaceholderColor(editor, DefaultPlaceholderTextColors);
		}

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(editor);
		}

		public static void MapMaxLength(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateMaxLength(editor);
		}

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(editor);
		}
	}
}