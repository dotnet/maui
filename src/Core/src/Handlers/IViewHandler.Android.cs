using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface IAndroidViewHandler : IViewHandler
	{
		void SetContext(Context context);

		AView? View { get; }
	}
}