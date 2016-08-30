namespace Xamarin.Forms.Platform.iOS
{
	public static class PlatformConfigurationExtensions
	{
		public static IPlatformElementConfiguration<PlatformConfiguration.iOS, T> OnThisPlatform<T>(this T element) 
			where T : Element, IElementConfiguration<T>
		{
			return (element).On<PlatformConfiguration.iOS>();
		}
	}
}