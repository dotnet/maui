#nullable disable
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellItemTransition
	{
		Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer);
	}
}