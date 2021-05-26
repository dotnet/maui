using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using static Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, AppCompatEditText>
	{
		static ColorStateList? DefaultTextColors { get; set; }
		static ColorStateList? DefaultPlaceholderTextColors { get; set; }
		static Drawable? DefaultBackground;

		EditorFocusChangeListener FocusChangeListener { get; } = new EditorFocusChangeListener();

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

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			FocusChangeListener.Handler = this;

			nativeView.OnFocusChangeListener = FocusChangeListener;
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			nativeView.OnFocusChangeListener = null;

			FocusChangeListener.Handler = null;
		}

		protected override void SetupDefaults(AppCompatEditText nativeView)
		{
			base.SetupDefaults(nativeView);

			DefaultTextColors = nativeView.TextColors;
			DefaultPlaceholderTextColors = nativeView.HintTextColors;
			DefaultBackground = nativeView.Background;
		}

		// This is a Android-specific mapping
		public static void MapBackground(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateBackground(editor, DefaultBackground);
		}

		public static void MapText(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateText(editor);
		}

		public static void MapTextColor(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateTextColor(editor, DefaultTextColors);
		}

		public static void MapPlaceholder(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdatePlaceholder(editor);
		}

		public static void MapPlaceholderColor(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdatePlaceholderColor(editor, DefaultPlaceholderTextColors);
		}

		public static void MapCharacterSpacing(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateCharacterSpacing(editor);
		}

		public static void MapMaxLength(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateMaxLength(editor);
		}

		public static void MapIsReadOnly(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateIsReadOnly(editor);
		}

		public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor)
		{
			handler.NativeView?.UpdateIsTextPredictionEnabled(editor);
		}

		public static void MapFont(EditorHandler handler, IEditor editor)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(editor, fontManager);
		}

		void OnFocusedChange(bool hasFocus)
		{
			if (!hasFocus)
				VirtualView?.Completed();
		}

		class EditorFocusChangeListener : Java.Lang.Object, IOnFocusChangeListener
		{
			public EditorHandler? Handler { get; set; }

			public void OnFocusChange(View? v, bool hasFocus)
			{
				Handler?.OnFocusedChange(hasFocus);
			}
		}
	}
}