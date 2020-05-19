using System.Threading.Tasks;
using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	internal interface IToolbarProvider
	{
		Task<CommandBar> GetCommandBarAsync();
	}
}