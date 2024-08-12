using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue9796 : ContentPage
	{
		public Issue9796()
		{
			InitializeComponent();
		}

		private void Editor_Completed(object sender, EventArgs e)
		{

		}

		private void Entry_Completed(object sender, EventArgs e)
		{

		}
	}
}