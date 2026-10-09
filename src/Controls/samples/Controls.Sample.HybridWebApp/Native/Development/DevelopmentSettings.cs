namespace Maui.Controls.Sample.HybridWebApp;

sealed record DevelopmentSettings(Uri? Upstream, string Token, int InspectionPort)
{
	public static DevelopmentSettings? Read()
	{
		var upstream = Environment.GetEnvironmentVariable("HYBRIDWEBAPP_DEV_URL");
		var token = Environment.GetEnvironmentVariable("HYBRIDWEBAPP_INSPECT_TOKEN");
		var port = Environment.GetEnvironmentVariable("HYBRIDWEBAPP_INSPECT_PORT");
		if (upstream is null && token is null && port is null)
			return null;

		if (token is null || token.Length is < 32 or > 128 || token.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
			throw new InvalidOperationException("Set HYBRIDWEBAPP_INSPECT_TOKEN to a 32–128 character random base64url token.");
		if (!int.TryParse(port, out var inspectionPort) || inspectionPort is < 1024 or > 65535 || inspectionPort == 5173)
			throw new InvalidOperationException("Set HYBRIDWEBAPP_INSPECT_PORT to an unused port from 1024 through 65535, other than 5173.");

		Uri? uri = null;
		if (upstream is not null &&
			(!Uri.TryCreate(upstream, UriKind.Absolute, out uri) ||
			uri.Scheme != "http" || uri.Host != "127.0.0.1" || uri.Port != 5173 ||
			uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0))
			throw new InvalidOperationException("HYBRIDWEBAPP_DEV_URL must be http://127.0.0.1:5173/.");

		return new DevelopmentSettings(uri, token, inspectionPort);
	}
}
