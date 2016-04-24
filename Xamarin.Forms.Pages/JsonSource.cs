using System;
using System.Threading.Tasks;

namespace Xamarin.Forms.Pages
{
	public abstract class JsonSource : Element
	{
		public static JsonSource FromString(string json)
		{
			return new StringJsonSource { Json = json };
		}

		public static JsonSource FromUri(Uri uri)
		{
			return new UriJsonSource { Uri = uri };
		}

		public abstract Task<string> GetJson();

		public static implicit operator JsonSource(string json)
		{
			return FromString(json);
		}
	}
}