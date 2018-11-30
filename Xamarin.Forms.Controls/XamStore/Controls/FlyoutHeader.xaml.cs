using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.XamStore
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FlyoutHeader : ContentView
	{
		public FlyoutHeader ()
		{
			InitializeComponent ();
		}
	}
}