namespace Microsoft.Maui.Controls.Platform
{
	public static class PlatformConfigurationExtensions
	{
		public static IPlatformElementConfiguration<PlatformConfiguration.Windows, T> OnThisPlatform<T>(this T element) 
			where T : Element, IElementConfiguration<T>
		{
			return (element).On<PlatformConfiguration.Windows>();
		}
	}
}
