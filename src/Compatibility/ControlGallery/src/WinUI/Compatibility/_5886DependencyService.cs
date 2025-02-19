using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.ControlGallery.WinUI;
using static Microsoft.Maui.Controls.ControlGallery.Issues.Issue5886;

[assembly: Dependency(typeof(MyInterfaceImplementation))]
namespace Microsoft.Maui.Controls.ControlGallery.WinUI
{
	public class MyInterfaceImplementation : IReplaceUWPRendererService
	{
		public MyInterfaceImplementation()
		{
		}

		public void ConvertToNative(View formsView)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			var renderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.Platform.GetRenderer(formsView);
			if (renderer != null)
			{
				renderer.Dispose();
				Platform.UWP.Platform.SetRenderer(formsView, null);
			}

			var newRenderer = formsView.GetOrCreateRenderer();
#pragma warning restore CS0612 // Type or member is obsolete
		}

		public void CreateRenderer(View formsView)
		{
			var newRenderer = formsView.GetOrCreateRenderer();
		}
	}
}