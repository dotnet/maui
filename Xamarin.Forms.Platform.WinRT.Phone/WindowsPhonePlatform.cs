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

		internal override Rectangle WindowBounds
		{
			get
			{
				bool landscape = Device.Info.CurrentOrientation.IsLandscape ();
				double offset = (landscape) ? _status.OccludedRect.Width : _status.OccludedRect.Height;

				Rectangle original = base.WindowBounds;
				return new Rectangle (original.X, original.Y, original.Width - ((landscape) ? offset : 0), original.Height - ((landscape) ? 0 : offset));
			}
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
