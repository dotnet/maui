using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace Xamarin.Forms.Platform.WinRT
{
	internal sealed class WindowsPhonePlatform
		: Platform
	{
		public WindowsPhonePlatform (Windows.UI.Xaml.Controls.Page page)
			: base (page)
		{
			_status = StatusBar.GetForCurrentView ();
			_status.Showing += OnStatusBarShowing;
			_status.Hiding += OnStatusBarHiding;
		}

		readonly StatusBar _status;

		void OnStatusBarHiding (StatusBar sender, object args)
		{
			UpdatePageSizes ();
		}

		void OnStatusBarShowing (StatusBar sender, object args)
		{
			UpdatePageSizes ();
		}
	}
}
