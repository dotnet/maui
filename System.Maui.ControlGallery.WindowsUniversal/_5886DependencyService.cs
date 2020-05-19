using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Platform.UWP;
using static System.Maui.Controls.Issues.Issue5886;

[assembly: Dependency(typeof(MyInterfaceImplementation))]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class MyInterfaceImplementation : IReplaceUWPRendererService
	{
		public MyInterfaceImplementation()
		{
		}

		public void ConvertToNative(View formsView)
		{
			var renderer = System.Maui.Platform.UWP.Platform.GetRenderer(formsView);
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