using System;

namespace ShapesDemos.Views
{
    public class SpiralRunnerDemoPage : SpiralDemoPage
    {
        public SpiralRunnerDemoPage()
        {
            polyline.StrokeDashArray.Add(4);
            polyline.StrokeDashArray.Add(2);
            double total = polyline.StrokeDashArray[0] + polyline.StrokeDashArray[1];

            //Device.StartTimer(TimeSpan.FromMilliseconds(15), () =>
            //{
            //    double secs = DateTime.Now.TimeOfDay.TotalSeconds;
            //    polyline.StrokeDashOffset = total * (secs % 1);
            //    return true;
            //});
        }
    }
}

