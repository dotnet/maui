using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;

namespace System.Maui.Controls.XamStore
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