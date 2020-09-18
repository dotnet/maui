using Android.Content;
using AView = Android.Views.View;

namespace Xamarin.Platform
{
	public interface IAndroidViewHandler : IViewHandler
	{
		void SetContext(Context context);

		AView View { get; }
	}
}