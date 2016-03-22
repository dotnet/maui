using Xamarin.Forms.Maps.WP8;

namespace Xamarin
{
	public static class FormsMaps
	{
		static bool s_isInitialized;

		internal static string ApplicationId { get; set; }

		internal static string AuthenticationToken { get; set; }

		public static void Init()
		{
			if (s_isInitialized)
				return;
			GeocoderBackend.Register();
			s_isInitialized = true;
		}

		public static void Init(string applicationId, string authenticationToken)
		{
			ApplicationId = applicationId;
			AuthenticationToken = authenticationToken;

			Init();
		}
	}
}