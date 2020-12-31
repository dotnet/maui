
using System;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.Core.View;
using Google.Android.Material.TextField;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

namespace Xamarin.Forms.Material.Android
{
	public class MaterialFormsTextInputLayoutBase : TextInputLayout
	{
		Color _formsTextColor;
		Color _formsPlaceholderColor;
		bool _isSetup = false;
		ColorStateList _placeholderColorsList;

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

			var underlineColors = MaterialColors.GetUnderlineColor(_formsPlaceholderColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(_formsPlaceholderColor, _formsTextColor);
			_placeholderColorsList = MaterialColors.CreateEntryFilledPlaceholderColors(placeHolderColors.InlineColor, placeHolderColors.FloatingColor);


			var textColor = MaterialColors.GetEntryTextColor(formsTextColor).ToArgb();
			EditText.SetTextColor(new ColorStateList(s_colorStates, new[] { textColor, textColor }));
		}

		public virtual void ApplyTheme(Color formsTextColor, Color formsPlaceHolderColor)
		{
			if (_disposed)
				return;

			if (!_isSetup)
			{
				_isSetup = true;
				EditText.FocusChange += OnFocusChange;
				ResetTextColors(formsTextColor, formsPlaceHolderColor);
			}
			else if (formsTextColor != _formsTextColor || _formsPlaceholderColor != formsPlaceHolderColor)
			{
				ResetTextColors(formsTextColor, formsPlaceHolderColor);
			}
			this.DefaultHintTextColor = _placeholderColorsList;
			this.HintTextColor = _placeholderColorsList.ToDefaultOnlyColorStateList();
			this.BoxStrokeColor = _placeholderColorsList.DefaultColor;
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

		public virtual void SetHint(string hint, VisualElement element)
		{
			HintEnabled = !string.IsNullOrWhiteSpace(hint);
			if (HintEnabled)
			{
				element?.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.VerticalOptionsChanged);
				Hint = hint;
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
