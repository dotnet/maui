#if __ANDROID_28__
using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Content.Res;

namespace Xamarin.Forms.Platform.Android.Material
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
		bool _isDisposed = false;

		public MaterialFormsTextInputLayoutBase(Context context) : base(context)
		{
			Init();
		}

		public MaterialFormsTextInputLayoutBase(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init();
		}

		public MaterialFormsTextInputLayoutBase(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Init();
		}

		protected MaterialFormsTextInputLayoutBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Init();
		}

		void Init()
		{
			VisualElement.VerifyVisualFlagEnabled();	
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
			if (_isDisposed)
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
		void OnFocusChange(object sender, FocusChangeEventArgs e) => 
			Device.BeginInvokeOnMainThread(() => ApplyTheme());


		internal void SetHint(string hint, VisualElement element)
		{
			if (HintEnabled != !String.IsNullOrWhiteSpace(hint))
			{
				HintEnabled = !String.IsNullOrWhiteSpace(hint);
				Hint = hint ?? String.Empty;
				EditText.Hint = String.Empty;
				element?.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.VerticalOptionsChanged);
			}
			else
			{
				Hint = hint ?? String.Empty;
			}
		}


		protected override void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				if (EditText != null)
					EditText.FocusChange -= OnFocusChange;
			}

			base.Dispose(disposing);
		}
	}
}
#endif