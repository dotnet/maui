using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	public partial class Issue16978 : ContentPage
	{
		public Issue16978()
		{
			InitializeComponent();
		}
		private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
		{
			(sender as Grid).BackgroundColor = Colors.GreenYellow;
		}
	}
}