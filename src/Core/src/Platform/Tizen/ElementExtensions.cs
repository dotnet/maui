using ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static partial class ElementExtensions
	{
		public static EvasObject ToContainerView(this IElement view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };
	}
}