using Windows.UI.Core;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class WindowsPlatformServices : WindowsBasePlatformServices
	{
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		public WindowsPlatformServices(Microsoft.System.DispatcherQueue dispatcher) : base(dispatcher)
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		{
		}
	}
}