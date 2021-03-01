using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls

namespace Maui.Controls.Sample
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Page1 : ContentPage, IPage
	{
		public Page1()
		{
			InitializeComponent();
		}

		public IView View { get => (IView)Content; set => Content = (View)value; }
	}
}