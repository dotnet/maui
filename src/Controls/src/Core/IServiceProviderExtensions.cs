namespace Microsoft.Maui.Controls.Xaml
{
	public static class IServiceProviderExtensions
	{
		public static T GetService<T>(this System.IServiceProvider provider)
		{
			var service = provider.GetService(typeof(T));
			if (service is T tservice)
				return tservice;
			return default(T);
		}
	}
}