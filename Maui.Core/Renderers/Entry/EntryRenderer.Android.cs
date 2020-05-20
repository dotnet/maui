using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

#if __ANDROID_29__
using AndroidX.Core.View;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V4.View;
using Android.Support.V7.Widget;
#endif

using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace System.Maui.Platform
{
	public partial class EntryRenderer : AbstractViewRenderer<ITextInput, AppCompatEditText>
	{
		TextColorSwitcher _textColorSwitcher;

		protected override AppCompatEditText CreateView()
		{
			var editText = new AppCompatEditText(Context);
			_textColorSwitcher = new TextColorSwitcher(editText);

			editText.TextChanged += EditTextTextChanged;

			return editText;
		}

		private void EditTextTextChanged(object sender, Android.Text.TextChangedEventArgs args) =>
			VirtualView.Text =args.Text.ToString();

		protected override void DisposeView(AppCompatEditText nativeView)
		{
			_textColorSwitcher = null;
			nativeView.TextChanged -= EditTextTextChanged;

			base.DisposeView(nativeView);
		}

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer.NativeView is AppCompatEditText))
			{
				return;
			}

			if (!(renderer is EntryRenderer entryRenderer))
			{ 
				return;
			}

			entryRenderer.UpdateTextColor(entry.Color);
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer.NativeView is AppCompatEditText editText))
			{
				return;
			}

			editText.Hint = entry.Placeholder;
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) 
		{
			if (!(renderer.NativeView is AppCompatEditText))
			{
				return;
			}

			if (!(renderer is EntryRenderer entryRenderer))
			{
				return;
			}

			entryRenderer.UpdatePlaceholderColor(entry.PlaceholderColor);
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer.NativeView is AppCompatEditText editText))
			{
				return;
			}

			editText.Text = entry.Text;
		}

		protected virtual void UpdateTextColor(Color color) 
		{
			_textColorSwitcher?.UpdateTextColor(color);
		}

		protected virtual void UpdatePlaceholderColor(Color color)
		{
			_textColorSwitcher?.UpdateHintTextColor(color);
		}
	}
}
