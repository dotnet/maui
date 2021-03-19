using System;
using Android.Content.Res;
using Android.Text;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, AppCompatEditText>
	{
		TextWatcher Watcher { get; } = new TextWatcher();

		static ColorStateList? DefaultTextColors { get; set; }

		protected override AppCompatEditText CreateNativeView()
		{
			return new AppCompatEditText(Context);
		}

		protected override void ConnectHandler(AppCompatEditText nativeView)
		{
			Watcher.Handler = this;
			nativeView.AddTextChangedListener(Watcher);
		}

		protected override void DisconnectHandler(AppCompatEditText nativeView)
		{
			nativeView.RemoveTextChangedListener(Watcher);
			Watcher.Handler = null;
		}

		protected override void SetupDefaults(AppCompatEditText nativeView)
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

		public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateHorizontalTextAlignment(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdatePlaceholder(entry);
		}

		public static void MapFont(EntryHandler handler, IEntry entry)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(entry, fontManager);
		}

		public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsReadOnly(entry);
		}

		public static void MapReturnType(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateReturnType(entry);
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