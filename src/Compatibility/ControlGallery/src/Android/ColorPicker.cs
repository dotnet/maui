using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;
using Droid = Android;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class ColorPickerView : ViewGroup
	//, INotifyPropertyChanged
	{
		static readonly int[] COLORS = new[] {
				new Droid.Graphics.Color(255,0,0,255).ToArgb(), new Droid.Graphics.Color(255,0,255,255).ToArgb(), new Droid.Graphics.Color(0,0,255,255).ToArgb(),
				new Droid.Graphics.Color(0,255,255,255).ToArgb(), new Droid.Graphics.Color(0,255,0,255).ToArgb(), new Droid.Graphics.Color(255,255,0,255).ToArgb(),
				new Droid.Graphics.Color(255,0,0,255).ToArgb()
			};
		Droid.Graphics.Point currentPoint;
		ColorPointer colorPointer;
		ImageView imageViewSelectedColor;
		ImageView imageViewPallete;
		Droid.Graphics.Color selectedColor;
		Droid.Graphics.Color previewColor;

		//public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler ColorPicked;

		public ColorPickerView(Context context, int minWidth, int minHeight) : base(context)
		{
			SelectedColor = Colors.Black.ToAndroid();

			SetMinimumHeight(minHeight);
			SetMinimumWidth(minWidth);

			imageViewPallete = new ImageView(context);
			imageViewPallete.Background = new Droid.Graphics.Drawables.GradientDrawable(Droid.Graphics.Drawables.GradientDrawable.Orientation.LeftRight, COLORS);

			imageViewPallete.Touch += (object sender, TouchEventArgs e) =>
			{
				if (e.Event.Action == MotionEventActions.Down || e.Event.Action == MotionEventActions.Move)
				{
					using (Droid.Graphics.Bitmap bitmap = Droid.Graphics.Bitmap.CreateBitmap(imageViewPallete.Width, imageViewPallete.Height, Droid.Graphics.Bitmap.Config.Argb8888))
					{
						Droid.Graphics.Canvas canvas = new Droid.Graphics.Canvas(bitmap);
						imageViewPallete.Background.Draw(canvas);

						currentPoint = new Droid.Graphics.Point((int)e.Event.GetX(), (int)e.Event.GetY());
						previewColor = GetCurrentColor(bitmap, (int)e.Event.GetX(), (int)e.Event.GetY());
					}
				}
				if (e.Event.Action == MotionEventActions.Up)
				{
					SelectedColor = previewColor;
				}
			};

			imageViewSelectedColor = new ImageView(context);
			colorPointer = new ColorPointer(context);

			AddView(imageViewPallete);
			AddView(imageViewSelectedColor);
			AddView(colorPointer);
		}

		public Droid.Graphics.Color SelectedColor
		{
			get
			{
				return selectedColor;
			}

			set
			{
				if (selectedColor == value)
					return;

				selectedColor = value;
				UpdateUi();
				OnPropertyChanged();
				OnColorPicked();
			}
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			var half = (bottom - top) / 2;
			var margin = 20;

			var palleteY = top + half;

			imageViewSelectedColor.Layout(left, top, right, bottom - half - margin);
			imageViewPallete.Layout(left, palleteY, right, bottom);
			colorPointer.Layout(left, palleteY, right, bottom);
		}

		void UpdateUi()
		{
			imageViewSelectedColor?.SetBackgroundColor(selectedColor);
			colorPointer?.UpdatePoint(currentPoint);
		}

		Droid.Graphics.Color GetCurrentColor(Droid.Graphics.Bitmap bitmap, int x, int y)
		{
			if (bitmap == null)
				return new Droid.Graphics.Color(255, 255, 255, 255);

			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x >= bitmap.Width)
				x = bitmap.Width - 1;
			if (y >= bitmap.Height)
				y = bitmap.Height - 1;

			int color = bitmap.GetPixel(x, y);
			return new Droid.Graphics.Color(color);
		}

		void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		{
			//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		void OnColorPicked()
		{
			ColorPicked?.Invoke(this, new EventArgs());
		}
	}

	public class ColorPointer : Droid.Views.View
	{
		Droid.Graphics.Paint colorPointerPaint;
		Droid.Graphics.Point currentPoint;
		Droid.Graphics.Point nextPoint;

		public ColorPointer(Context context) : base(context)
		{

			colorPointerPaint = new Droid.Graphics.Paint();
			colorPointerPaint.SetStyle(Droid.Graphics.Paint.Style.Stroke);
			colorPointerPaint.StrokeWidth = 5f;
			colorPointerPaint.SetARGB(255, 0, 0, 0);

		}

		public void UpdatePoint(Droid.Graphics.Point p)
		{
			if (p == null)
				return;

			if (currentPoint == null)
				currentPoint = nextPoint;

			nextPoint = p;
		}

		protected override void OnDraw(Droid.Graphics.Canvas canvas)
		{
			base.OnDraw(canvas);
		}
	}
}
