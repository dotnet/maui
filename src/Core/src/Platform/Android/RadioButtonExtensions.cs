using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class RadioButtonExtensions
	{
		public static void UpdateBackground(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateIsChecked(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.Checked = radioButton.IsChecked;
		}

		public static void UpdateContent(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.Text = $"{radioButton.Content}";
		}

		public static void UpdateStrokeColor(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateStrokeThickness(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		public static void UpdateCornerRadius(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateBorderDrawable(this AppCompatRadioButton platformView, IRadioButton radioButton)
		{
			BorderDrawable? mauiDrawable = platformView.Background as BorderDrawable;

			if (mauiDrawable == null)
			{
				mauiDrawable = new BorderDrawable(platformView.Context);

				platformView.Background = mauiDrawable;
			}

			// TODO: Material3 RadioButtons should not draw any background surface by design, but when radioButton.Background 
			// is null, BorderDrawable.SetBackground() falls back to SetDefaultBackgroundColor() which uses the theme's 
			// windowBackground attribute, causing unwanted surface backgrounds.
			// Workaround: provide default transparent background for Material3 to prevent this fallback.
   			var background = radioButton.Background ??
			(RuntimeFeature.IsMaterial3Enabled ? new SolidPaint(Colors.Transparent) : null);
   
  			mauiDrawable.SetBackground(background);

			if (radioButton.StrokeColor != null)
				mauiDrawable.SetBorderBrush(new SolidPaint { Color = radioButton.StrokeColor });

			if (radioButton.StrokeThickness > 0)
				mauiDrawable.SetBorderWidth(radioButton.StrokeThickness);

			if (radioButton.CornerRadius > 0)
				mauiDrawable.SetCornerRadius(radioButton.CornerRadius);
		}
	}
}