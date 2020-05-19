using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;

namespace Embedding.XF
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Hello : ContentPage
	{
		public Hello()
		{
			InitializeComponent();
		}
	}
}