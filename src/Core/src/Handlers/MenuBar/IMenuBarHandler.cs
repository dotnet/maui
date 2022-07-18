#if IOS || MACCATALYST
using PlatformView = UIKit.IUIMenuBuilder;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuBar;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface IMenuBarHandler : IElementHandler
	{
		void Add(IMenuBarItem view);
		void Remove(IMenuBarItem view);
		void Clear();
		void Insert(int index, IMenuBarItem view);
		new PlatformView PlatformView { get; }
		new IMenuBar VirtualView { get; }
	}
}
