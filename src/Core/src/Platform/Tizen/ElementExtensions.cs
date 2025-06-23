using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static partial class ElementExtensions
	{
		public static NView ToContainerView(this IElement? view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };
	}
}