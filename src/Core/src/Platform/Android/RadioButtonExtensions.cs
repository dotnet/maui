using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateBackground(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateIsChecked(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.Checked = radioButton.IsChecked;
		}

		public static void UpdateContent(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.Text = $"{radioButton.Content}";
		}

		public static void UpdateStrokeColor(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateStrokeThickness(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateCornerRadius(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateBorderDrawable(this AppCompatRadioButton nativeView, IRadioButton radioButton)
		{
			BorderDrawable? mauiDrawable = nativeView.Background as BorderDrawable;

			if (mauiDrawable == null)
			{
				mauiDrawable = new BorderDrawable(nativeView.Context);

				nativeView.Background = mauiDrawable;
			}

			mauiDrawable.SetBackground(radioButton.Background);

			if (radioButton.StrokeColor != null)
				mauiDrawable.SetBorderBrush(new SolidPaint { Color = radioButton.StrokeColor });

			if (radioButton.StrokeThickness > 0)
				mauiDrawable.SetBorderWidth(radioButton.StrokeThickness);

			if (radioButton.CornerRadius > 0)
				mauiDrawable.SetCornerRadius(radioButton.CornerRadius);

		}
	}
}