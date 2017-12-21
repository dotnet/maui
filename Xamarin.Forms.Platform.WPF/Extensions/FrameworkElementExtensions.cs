using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xamarin.Forms.Platform.WPF
{
	public static class FrameworkElementExtensions
	{
		public static object UpdateDependencyColor(this DependencyObject depo, DependencyProperty dp, Color newColor)
		{
			if (!newColor.IsDefault)
				depo.SetValue(dp, newColor.ToBrush());
			else
				depo.ClearValue(dp);

			return depo.GetValue(dp);
		}
	}
}
