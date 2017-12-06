using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{ 
	public static class ColorExtensions
	{
		public static Brush ToBrush(this Color color)
		{
			return new SolidColorBrush(color.ToMediaColor());
		}

		public static System.Windows.Media.Color ToMediaColor(this Color color)
		{
			return System.Windows.Media.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}
