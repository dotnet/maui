using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	// TODO: internal for now as we are not yet sure we want this
	interface IWindowHandler : IElementHandler
	{
		IAnimationManager AnimationManager { get; }
	}
}