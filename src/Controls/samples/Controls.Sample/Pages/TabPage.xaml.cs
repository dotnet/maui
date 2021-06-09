using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TabPage : TabbedPage
	{
		public TabPage()
		{
			InitializeComponent();
		}
	}
}