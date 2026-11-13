using System;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class RadioButtonExtensions
	{
		public static void UpdateBackground(this AppCompatRadioButton platformRadioButton, IRadioButton radioButton)
		{
			if (radioButton.Background.IsNullOrEmpty())
			{
				if (platformRadioButton.Background is BorderDrawable existingDrawable)
				{
					platformRadioButton.Background = null;
					existingDrawable.Dispose();
				}
				return;
			}

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

			mauiDrawable.SetBackground(radioButton.Background);
			mauiDrawable.SetBorderBrush(new SolidPaint { Color = radioButton.StrokeColor });
			mauiDrawable.SetBorderWidth(Math.Max(0, radioButton.StrokeThickness));
			mauiDrawable.SetCornerRadius(Math.Max(0, radioButton.CornerRadius));
		}
	}
}