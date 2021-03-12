using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Platform.UWP;
using static Xamarin.Forms.Controls.Issues.Issue5886;

[assembly: Dependency(typeof(MyInterfaceImplementation))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class MyInterfaceImplementation : IReplaceUWPRendererService
	{
		public MyInterfaceImplementation()
		{
		}

		public void ConvertToNative(View formsView)
		{
			var renderer = Xamarin.Forms.Platform.UWP.Platform.GetRenderer(formsView);
			if (renderer != null)
			{
				renderer.Dispose();
				Platform.UWP.Platform.SetRenderer(formsView, null);
			}

			var newRenderer = formsView.GetOrCreateRenderer();
		}

		public void CreateRenderer(View formsView)
		{
			var newRenderer = formsView.GetOrCreateRenderer();
		}
	}
}