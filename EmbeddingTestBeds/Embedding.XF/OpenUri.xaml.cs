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
	public partial class OpenUri : ContentPage
	{
		public OpenUri()
		{
			InitializeComponent();

			OpenUriButton.Clicked += OpenUriButtonOnClicked;
		}

		void OpenUriButtonOnClicked(object sender, EventArgs eventArgs)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			Device.OpenUri(new Uri("http://www.bing.com"));
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}