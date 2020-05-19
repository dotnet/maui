using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xamarin.Forms.Pages
{
	public class UriJsonSource : JsonSource
	{
		public static readonly BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(Uri),
			typeof(UriJsonSource), null);

		[TypeConverter(typeof(UriTypeConverter))]
		public Uri Uri
		{
			get { return (Uri)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}

		public override async Task<string> GetJson()
		{
			var webClient = new HttpClient();
			try
			{
				string json = await webClient.GetStringAsync(Uri);
				return json;
			}
			catch
			{
				return null;
			}
		}
	}
}