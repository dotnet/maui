using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public interface IButtonLayoutRenderer : IVisualElementRenderer
	{
		new AppCompatButton View { get; }
	}
}
