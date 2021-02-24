using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellItemTransition
	{
		Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer);
	}
}