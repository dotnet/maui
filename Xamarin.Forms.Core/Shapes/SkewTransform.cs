using System;

namespace Xamarin.Forms.Shapes
{
    public class SkewTransform : Transform
    {
        public static readonly BindableProperty AngleXProperty =
            BindableProperty.Create(nameof(AngleX), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

        public static readonly BindableProperty AngleYProperty =
            BindableProperty.Create(nameof(AngleY), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

        public static readonly BindableProperty CenterXProperty =
            BindableProperty.Create(nameof(CenterX), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

        public static readonly BindableProperty CenterYProperty =
            BindableProperty.Create(nameof(CenterY), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

        public double AngleX
        {
            set { SetValue(AngleXProperty, value); }
            get { return (double)GetValue(AngleXProperty); }
        }

        public double AngleY
        {
            set { SetValue(AngleYProperty, value); }
            get { return (double)GetValue(AngleYProperty); }
        }

        public double CenterX
        {
            set { SetValue(CenterXProperty, value); }
            get { return (double)GetValue(CenterXProperty); }
        }

        public double CenterY
        {
            set { SetValue(CenterYProperty, value); }
            get { return (double)GetValue(CenterYProperty); }
        }

        static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as SkewTransform).OnTransformPropertyChanged();
        }

        void OnTransformPropertyChanged()
        {
            double radiansX = Math.PI * AngleX / 180;
            double radiansY = Math.PI * AngleY / 180;
            double tanX = Math.Tan(radiansX);
            double tanY = Math.Tan(radiansY);

            Value = new Matrix(1, tanY, tanX, 1, -CenterY * tanX, -CenterX * tanY);
        }
    }
}