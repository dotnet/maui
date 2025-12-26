using AndroidX.AppCompat.Widget;
using Google.Android.Material.RadioButton;
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

			mauiDrawable.SetBackground(radioButton.Background);

			if (radioButton.StrokeColor != null)
				mauiDrawable.SetBorderBrush(new SolidPaint { Color = radioButton.StrokeColor });

			if (radioButton.StrokeThickness > 0)
				mauiDrawable.SetBorderWidth(radioButton.StrokeThickness);

			if (radioButton.CornerRadius > 0)
				mauiDrawable.SetCornerRadius(radioButton.CornerRadius);
		}

		internal static void UpdateBackground(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateIsChecked(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.Checked = radioButton.IsChecked;
		}

		internal static void UpdateContent(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.Text = $"{radioButton.Content}";
		}

		internal static void UpdateStrokeColor(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateStrokeThickness(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateCornerRadius(this MaterialRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.UpdateBorderDrawable(radioButton);
		}

		internal static void UpdateBorderDrawable(this MaterialRadioButton platformView, IRadioButton radioButton)
		{
			BorderDrawable? mauiDrawable = platformView.Background as BorderDrawable;

			if (mauiDrawable is null)
			{
				mauiDrawable = new BorderDrawable(platformView.Context);
				platformView.Background = mauiDrawable;
			}

			mauiDrawable.SetBackground(radioButton.Background);

			if (radioButton.StrokeColor is not null)
			{
				mauiDrawable.SetBorderBrush(new SolidPaint { Color = radioButton.StrokeColor });
			}

			if (radioButton.StrokeThickness > 0)
			{
				mauiDrawable.SetBorderWidth(radioButton.StrokeThickness);
			}

			if (radioButton.CornerRadius > 0)
			{
				mauiDrawable.SetCornerRadius(radioButton.CornerRadius);
			}
		}
	}
}