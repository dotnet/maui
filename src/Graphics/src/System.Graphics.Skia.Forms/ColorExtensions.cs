using Xamarin.Forms;

namespace System.Graphics.Forms
{
    public static class ColorExtensions
    {
        public static Xamarin.Forms.Color AsFormsColor(this Color color)
        {
            if (color == null)
                return Xamarin.Forms.Color.Black;

            return new Xamarin.Forms.Color(color.Red, color.Green, color.Blue, color.Alpha);
        }
    }
}