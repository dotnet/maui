using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface IToolbarProvider
	{
		Task<CommandBar> GetCommandBarAsync();
	}
}