using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal interface IToolbarProvider
	{
		Task<CommandBar> GetCommandBarAsync();
	}
}