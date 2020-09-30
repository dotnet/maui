
using System;
using Android.Content;
using Google.Android.Material.TextField;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.Material.Android
{
	internal static class MaterialFormsEditTextManager
	{
		// These paddings are a hack to center the hint
		// once this issue is resolved we can get rid of these paddings
		// https://github.com/material-components/material-components-android/issues/120
		// https://stackoverflow.com/questions/50487871/how-to-make-the-hint-text-of-textinputlayout-vertically-center

		static Thickness _centeredText = new Thickness(16, 8, 12, 27);
		static Thickness _alignedWithUnderlineText = new Thickness(16, 20, 12, 16);

		public static void Init(TextInputEditText textInputEditText)
		{
			textInputEditText.TextChanged += OnTextChanged;
			textInputEditText.FocusChange += OnFocusChanged;
		}

		public static void Dispose(TextInputEditText textInputEditText)
		{
			textInputEditText.TextChanged -= OnTextChanged;
			textInputEditText.FocusChange -= OnFocusChanged;
		}

		private static void OnFocusChanged(object sender, global::Android.Views.View.FocusChangeEventArgs e)
		{
			if (sender is TextInputEditText textInputEditText)
			{
				// Delay padding update until after the keyboard has showed up otherwise updating the padding
				// stops the keyboard from showing up
				// TODO closure
				if (e.HasFocus)
					Device.BeginInvokeOnMainThread(() => UpdatePadding(textInputEditText));
				else
					UpdatePadding(textInputEditText);
			}
		}

		private static void OnTextChanged(object sender, global::Android.Text.TextChangedEventArgs e)
		{
			if (e.BeforeCount == 0 || e.AfterCount == 0)
				UpdatePadding(sender as TextInputEditText);
		}

		static void UpdatePadding(TextInputEditText textInputEditText)
		{
			Thickness rect = _centeredText;

			if (!String.IsNullOrWhiteSpace(textInputEditText.Text) || textInputEditText.HasFocus)
			{
				rect = _alignedWithUnderlineText;
			}

			Context Context = textInputEditText.Context;
			var left = (int)Context.ToPixels(rect.Left);
			var top = (int)Context.ToPixels(rect.Top);
			var right = (int)Context.ToPixels(rect.Right);
			var bottom = (int)Context.ToPixels(rect.Bottom);

			if (textInputEditText.PaddingLeft != left ||
			   textInputEditText.PaddingTop != top ||
			   textInputEditText.PaddingRight != right ||
			   textInputEditText.PaddingBottom != bottom)
				textInputEditText.SetPadding(left, top, right, bottom);
		}
	}
}