using System.Windows;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsPathIcon : FormsElementIcon
	{
		public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(FormsPathIcon));

		public Geometry Data
		{
			get { return (Geometry)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		public FormsPathIcon()
		{
			this.DefaultStyleKey = typeof(FormsPathIcon);
		}
	}
}
