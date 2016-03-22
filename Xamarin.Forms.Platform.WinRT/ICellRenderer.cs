
#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public interface ICellRenderer : IRegisterable
	{
		Windows.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}