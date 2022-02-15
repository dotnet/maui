using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using static Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Issue5886;

[assembly: Dependency(typeof(MyInterfaceImplementation))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class MyInterfaceImplementation : IReplaceUWPRendererService
	{
		public MyInterfaceImplementation()
		{
		}

		public void ConvertToNative(View formsView)
		{
			var renderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.Platform.GetRenderer(formsView);
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