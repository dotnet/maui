namespace Xamarin.Forms.Maps.GTK
{
	public static class FormsMaps
	{
		static bool _isInitialized;

		internal static string AuthenticationToken { get; set; }

		public static void Init()
		{
			if (_isInitialized)
				return;

			GeocoderBackend.Register();
			_isInitialized = true;
		}

		public static void Init(string authenticationToken)
		{
			AuthenticationToken = authenticationToken;

			Init();
		}
	}
}