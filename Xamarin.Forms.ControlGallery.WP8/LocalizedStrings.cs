using Xamarin.Forms.ControlGallery.WP8.Resources;

namespace Xamarin.Forms.ControlGallery.WP8
{
	/// <summary>
	/// Provides access to string resources.
	/// </summary>
	public class LocalizedStrings
	{
		static AppResources _localizedResources = new AppResources();

		public AppResources LocalizedResources { get { return _localizedResources; } }
	}
}
