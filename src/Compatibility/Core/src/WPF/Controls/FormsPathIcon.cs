using System.Windows;
using WGeometry = System.Windows.Media.Geometry;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF.Controls
{
	public class FormsPathIcon : FormsElementIcon
	{
		public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(WGeometry), typeof(FormsPathIcon));

		public WGeometry Data
		{
			get { return (WGeometry)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		public FormsPathIcon()
		{
			this.DefaultStyleKey = typeof(FormsPathIcon);
		}
	}
}
