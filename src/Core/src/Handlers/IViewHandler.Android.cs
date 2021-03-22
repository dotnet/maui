using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface IAndroidViewHandler : IViewHandler
	{
		AView? View { get; }
	}
}