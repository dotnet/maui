using System.Threading.Tasks;

namespace System.Maui.Platform.iOS
{
	public interface IShellItemTransition
	{
		Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer);
	}
}