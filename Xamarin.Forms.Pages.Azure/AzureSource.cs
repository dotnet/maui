using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Xamarin.Forms.Pages.Azure
{
	public abstract class AzureSource : Element
	{
		public static readonly BindableProperty UriProperty = 
			BindableProperty.Create(nameof(Uri), typeof(Uri), typeof(AzureSource), null);

		[TypeConverter(typeof(UriTypeConverter))]
		public Uri Uri
		{
			get { return (Uri)GetValue(UriProperty); }
			set { SetValue(UriProperty, value); }
		}

		public abstract Task<JToken> GetJson();
	}
}