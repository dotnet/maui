using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Core;

namespace Sample
{
	public class MyApp : IApp
	{
		public MyApp()
		{
			Platform.Init();
		}

		public IView CreateView()
		{
			return new Button() { Color = Color.Green , Text = "Hello I'm a button", BackgroundColor = Color.Purple };
		}
	}
}