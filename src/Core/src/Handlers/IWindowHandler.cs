using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	public interface IWindowHandler : IElementHandler
	{
		IAnimationManager AnimationManager { get; }
	}
}