using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	internal static class ImageExtensions
	{
		public static Stretch ToStretch(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return Stretch.Fill;
				case Aspect.AspectFill:
					return Stretch.UniformToFill;
				default:
				case Aspect.AspectFit:
					return Stretch.Uniform;
			}
		}
	}
}
