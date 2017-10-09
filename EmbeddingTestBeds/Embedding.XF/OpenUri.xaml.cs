using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Embedding.XF
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OpenUri : ContentPage
	{
		public OpenUri()
		{
			InitializeComponent();

			OpenUriButton.Clicked += OpenUriButtonOnClicked;
		}

		void OpenUriButtonOnClicked(object sender, EventArgs eventArgs)
		{
			Device.OpenUri(new Uri("http://www.bing.com"));
		}
	}
}