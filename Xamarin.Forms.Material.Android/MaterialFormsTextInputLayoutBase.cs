
using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
#if __ANDROID_29__
using AndroidX.Core.View;
using Google.Android.Material.TextField;
#else
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using Xamarin.Forms.Platform.Android.AppCompat;
#endif
using Android.Content.Res;
using AView = Android.Views.View;
using Xamarin.Forms.Platform.Android;
using Android.Widget;

namespace Xamarin.Forms.Material.Android
{
	public class MaterialFormsTextInputLayoutBase : TextInputLayout
	{
		Color _formsTextColor;
		Color _formsPlaceholderColor;
		bool _isSetup = false;
		ColorStateList _focusedFilledColorList;
		ColorStateList _unfocusedEmptyColorList;
		private ColorStateList _unfocusedUnderlineColorsList;
		private ColorStateList _focusedUnderlineColorsList;
		static readonly int[][] s_colorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };
		bool _disposed = false;

		public MaterialFormsTextInputLayoutBase(Context context) : base(context)
		{
		}

		public MaterialFormsTextInputLayoutBase(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public MaterialFormsTextInputLayoutBase(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		protected MaterialFormsTextInputLayoutBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		void ResetTextColors(Color formsTextColor, Color formsPlaceHolderColor)
		{
			_formsPlaceholderColor = formsPlaceHolderColor;
			_formsTextColor = formsTextColor;

			var underlineColors = MaterialColors.GetUnderlineColor(_formsTextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(_formsPlaceholderColor, _formsTextColor);

			// I realize these are the same but I have to set it to a difference instance
			// otherwise when focused it won't change to the color I want it to and it'll just think
			// I'm not actually changing anything
			_unfocusedUnderlineColorsList = MaterialColors.CreateEntryUnderlineColors(underlineColors.FocusedColor, underlineColors.UnFocusedColor);
			_focusedUnderlineColorsList = MaterialColors.CreateEntryUnderlineColors(underlineColors.FocusedColor, underlineColors.UnFocusedColor);

			_focusedFilledColorList = MaterialColors.CreateEntryFilledPlaceholderColors(placeHolderColors.FloatingColor, placeHolderColors.FloatingColor);
			_unfocusedEmptyColorList = MaterialColors.CreateEntryFilledPlaceholderColors(placeHolderColors.InlineColor, placeHolderColors.FloatingColor);

			var textColor = MaterialColors.GetEntryTextColor(formsTextColor).ToArgb();
			EditText.SetTextColor(new ColorStateList(s_colorStates, new[] { textColor, textColor }));
		}

		internal void ApplyTheme(Color formsTextColor, Color formsPlaceHolderColor)
		{
			if (_disposed)
				return;

			if(!_isSetup)
			{
				_isSetup = true;
				EditText.FocusChange += OnFocusChange;
				ResetTextColors(formsTextColor, formsPlaceHolderColor);
			}
			else if(formsTextColor != _formsTextColor || _formsPlaceholderColor != formsPlaceHolderColor)
			{
				ResetTextColors(formsTextColor, formsPlaceHolderColor);
			}

			if(HasFocus)
				ViewCompat.SetBackgroundTintList(EditText, _focusedUnderlineColorsList);
			else
				ViewCompat.SetBackgroundTintList(EditText, _unfocusedUnderlineColorsList);

			if (HasFocus || !string.IsNullOrWhiteSpace(EditText.Text))
				this.DefaultHintTextColor = _focusedFilledColorList;
			else
				this.DefaultHintTextColor = _unfocusedEmptyColorList;
		}

		void ApplyTheme() => ApplyTheme(_formsTextColor, _formsPlaceholderColor);

		/*
		 * This currently does two things
		 * 1) It's a hacky way of keeping the underline color matching the TextColor.
		 * when the entry gets focused the underline gets changed to the themes active color 
		 * and this is the only way to set it away from that and to whatever the user specified
		 * 2) The HintTextColor has a different alpha when focused vs not focused
		 * */
		void OnFocusChange(object sender, FocusChangeEventArgs e)
		{
			if (EditText == null)
				return;

			Device.BeginInvokeOnMainThread(() => ApplyTheme());

			// propagate the focus changed event to the View Renderer base class
			if (Parent is AView.IOnFocusChangeListener focusChangeListener)
				focusChangeListener.OnFocusChange(EditText, e.HasFocus);

		}

		internal void SetHint(string hint, VisualElement element)
		{
			HintEnabled = !string.IsNullOrWhiteSpace(hint);
			if (HintEnabled)
			{
				element?.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.VerticalOptionsChanged);
				Hint = hint;
				// EditText.Hint => Hint
				// It is impossible to reset it but you can make it invisible.
				EditText.SetHintTextColor(global::Android.Graphics.Color.Transparent);
			}
		}

		public override EditText EditText
		{
			get
			{
				if (this.IsDisposed())
					return null;

				return base.EditText;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (EditText != null)
					EditText.FocusChange -= OnFocusChange;
			}

			base.Dispose(disposing);
		}
	}
}