// This class represents common code paths we always want to AOT

// System.Threading.Tasks.Task
// System.Net.Http.HttpClient

static class CommonMethods
{
    // Returns '200 OK' if the caller wants to set that on the UI
    public static async Task<string> Invoke()
    {
        const string url = "https://httpstat.us/200";

        using var client = new HttpClient();
        var send = client.SendAsync (new HttpRequestMessage (HttpMethod.Get, url));
        var getstring = client.GetStringAsync (url);
        await Task.WhenAll (send, getstring);
        var text = getstring.Result;

        return text;
    }
}