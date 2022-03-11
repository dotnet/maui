using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal interface IToolbarProvider
	{
		Task<CommandBar> GetCommandBarAsync();
	}
}