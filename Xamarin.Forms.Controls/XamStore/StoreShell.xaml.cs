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
	public partial class StoreShell : Shell
	{
		public StoreShell ()
		{
			InitializeComponent ();

			CurrentItem = _storeItem;
		}

		//bool allow = false;

		//protected override void OnNavigating(ShellNavigatingEventArgs args)
		//{
		//	if (allow)
		//		args.Cancel();

		//	allow = !allow;
		//	base.OnNavigating(args);
		//}
	}
}