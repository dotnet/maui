using Android.Content.Res;
using Android.Text;
using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, EditText>
	{
		TextWatcher Watcher { get; } = new TextWatcher();

		static ColorStateList? DefaultTextColors { get; set; }

		protected override EditText CreateNativeView()
		{
			return new EditText(Context);
		}

		protected override void ConnectHandler(EditText nativeView)
		{
			Watcher.Handler = this;
			nativeView.AddTextChangedListener(Watcher);
		}

		protected override void DisconnectHandler(EditText nativeView)
		{
			nativeView.RemoveTextChangedListener(Watcher);
			Watcher.Handler = null;
		}

		protected override void SetupDefaults(EditText nativeView)
		{
			base.SetupDefaults(nativeView);
			DefaultTextColors = nativeView.TextColors;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColors);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		{
			var fontManager = services.GetRequiredService<IFontManager>();

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
		}

		void OnTextChanged(string? text)
		{
			if (VirtualView == null || TypedNativeView == null)
				return;

			// Even though <null> is technically different to "", it has no
			// functional difference to apps. Thus, hide it.
			var mauiText = VirtualView.Text ?? string.Empty;
			var nativeText = text ?? string.Empty;
			if (mauiText != nativeText)
				VirtualView.Text = nativeText;
		}

		class TextWatcher : Java.Lang.Object, ITextWatcher
		{
			public EntryHandler? Handler { get; set; }

			void ITextWatcher.AfterTextChanged(IEditable? s)
			{
			}

			void ITextWatcher.BeforeTextChanged(Java.Lang.ICharSequence? s, int start, int count, int after)
			{
			}

			void ITextWatcher.OnTextChanged(Java.Lang.ICharSequence? s, int start, int before, int count)
			{
				Handler?.OnTextChanged(s?.ToString());
			}
		}
	}
}