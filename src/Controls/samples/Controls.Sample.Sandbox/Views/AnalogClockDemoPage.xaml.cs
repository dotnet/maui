using System;
using Microsoft.Maui.Controls;

namespace ShapesDemos.Views
{
    public partial class AnalogClockDemoPage : ContentPage
    {
        public static readonly BindableProperty SecondHandAngleProperty = BindableProperty.Create(nameof(SecondHandAngle), typeof(double), typeof(AnalogClockDemoPage), default(double));
        public static readonly BindableProperty MinuteHandAngleProperty = BindableProperty.Create(nameof(MinuteHandAngle), typeof(double), typeof(AnalogClockDemoPage), default(double));
        public static readonly BindableProperty HourHandAngleProperty = BindableProperty.Create(nameof(HourHandAngle), typeof(double), typeof(AnalogClockDemoPage), default(double));

        public double SecondHandAngle
        {
            get { return (double)GetValue(SecondHandAngleProperty); }
            set { SetValue(SecondHandAngleProperty, value); }
        }

        public double MinuteHandAngle
        {
            get { return (double)GetValue(MinuteHandAngleProperty); }
            set { SetValue(MinuteHandAngleProperty, value); }
        }

        public double HourHandAngle
        {
            get { return (double)GetValue(HourHandAngleProperty); }
            set { SetValue(HourHandAngleProperty, value); }
        }

        public AnalogClockDemoPage()
        {
            InitializeComponent();

            BindingContext = this;
            //Device.StartTimer(TimeSpan.FromMilliseconds(15), () =>
            //{
            //    DateTime dateTime = DateTime.Now;
            //    SecondHandAngle = 6 * (dateTime.Second + dateTime.Millisecond / 1000.0);
            //    MinuteHandAngle = 6 * dateTime.Minute + SecondHandAngle / 60;
            //    HourHandAngle = 30 * (dateTime.Hour % 12) + MinuteHandAngle / 12;
            //    return true;
            //});

            SizeChanged += (sender, args) =>
            {
                grid.AnchorX = 0;
                grid.AnchorY = 0;
                grid.Scale = Math.Min(Width, Height) / 200;
            };
        }
    }
}
