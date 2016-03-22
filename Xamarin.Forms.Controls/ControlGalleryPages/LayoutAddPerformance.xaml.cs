using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public partial class LayoutAddPerformance : ContentPage
	{
		public LayoutAddPerformance ()
		{
			InitializeComponent ();
		}

		protected override async void OnAppearing ()
		{
			base.OnAppearing ();

			layout.Children.Clear ();

			await Task.Delay (2000);

			Stopwatch sw = new Stopwatch();
			sw.Start ();
			for (int i = 0; i < 500; i++) {
				layout.Children.Add (new Label { Text = i.ToString () });
			}
			sw.Stop ();
			this.timingLabel.Text = sw.ElapsedMilliseconds.ToString ();
		}
	}
}
