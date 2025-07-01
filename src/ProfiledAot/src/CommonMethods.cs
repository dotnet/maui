// This class represents common code paths we always want to AOT

// string interpolation & split
// int.Parse(), int.ToString()
// Culture-aware string comparisons
// ResourceManager
// Common Essentials methods
// GridLayoutManager
// Label & FontAttributes
// Microsoft.Maui.Graphics.Color
// System.Threading.Tasks.Task
// System.Net.Http.HttpClient

// Opt out of this warning, because we actually *want* culture-aware string behavior in the AOT profile
#pragma warning disable CA1307

static class CommonMethods
{
	// Returns '200 OK' if the caller wants to set that on the UI
	public static async Task<string> Invoke()
	{
		// NOTE: alternate web services if one of these is down
		//var url = $"https://httpstat.us/{200}";
		var url = $"https://httpbin.org/status/{200}";

		var foo = "foo";
		foo.StartsWith("f");
		foo.Contains("o");
		var split = "foo;bar".Split(';');
		var x = int.Parse("999");
		x.ToString();

		string someString = MauiTest.Resources.Strings.SomeString;

		InitializeCommonEssentials();

		_ = new Microsoft.Maui.Layouts.GridLayoutManager(new Grid()).Measure(100, 100);

		_ = new Label { FontAttributes = FontAttributes.Bold, Text = "Test" };

		Color c = Color.Parse("MistyRose");
		c = Color.Parse("#663399");

		using var client = new HttpClient();
		var send = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
		var getstring = client.GetStringAsync(url);
		await Task.WhenAll(send, getstring, Task.CompletedTask, Task.Delay(1));
		var text = getstring.Result;

		return text;
	}

	private static void InitializeCommonEssentials()
	{
		NetworkAccess networkAccess = Connectivity.NetworkAccess;
		DeviceIdiom deviceIdiom = DeviceInfo.Idiom;
		AppTheme appTheme = AppInfo.RequestedTheme;
	}
}