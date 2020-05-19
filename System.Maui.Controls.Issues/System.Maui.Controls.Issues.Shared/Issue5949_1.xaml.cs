using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Internals;
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	public partial class Issue5949_1 : MasterDetailPage
	{
		public Issue5949_1()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}