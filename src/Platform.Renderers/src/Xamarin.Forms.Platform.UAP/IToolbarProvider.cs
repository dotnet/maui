using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface IToolbarProvider
	{
		Task<CommandBar> GetCommandBarAsync();
	}
}