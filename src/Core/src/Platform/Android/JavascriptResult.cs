#nullable disable
using System.Threading.Tasks;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	class JavascriptResult : Java.Lang.Object, IValueCallback
	{
		readonly TaskCompletionSource<string> source;

		public Task<string> JsResult { get { return source.Task; } }

		public JavascriptResult()
		{
			source = new TaskCompletionSource<string>();
		}

		public void OnReceiveValue(Java.Lang.Object result)
		{
			string json = ((Java.Lang.String)result).ToString();
			source.SetResult(json);
		}
	}
}
