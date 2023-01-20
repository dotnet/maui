#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyout;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface IMenuFlyoutHandler : IElementHandler
	{
		void Add(IMenuElement view);
		void Remove(IMenuElement view);
		void Clear();
		void Insert(int index, IMenuElement view);

		new PlatformView PlatformView { get; }
		new IMenuFlyout VirtualView { get; }
	}
}
