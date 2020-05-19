using System.Maui.Maps.UWP;

namespace System.Maui
{
	public static class FormsMaps
	{
		static bool s_isInitialized;

		internal static string AuthenticationToken { get; set; }

		public static void Init()
		{
			if (s_isInitialized)
				return;
			GeocoderBackend.Register();
			s_isInitialized = true;
		}

		public static void Init(string authenticationToken)
		{
			AuthenticationToken = authenticationToken;

			Init();
		}
	}
}