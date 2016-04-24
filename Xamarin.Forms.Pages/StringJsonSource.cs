using System.Threading.Tasks;

namespace Xamarin.Forms.Pages
{
	public class StringJsonSource : JsonSource
	{
		public static readonly BindableProperty JsonProperty = BindableProperty.Create(nameof(Json), typeof(string), typeof(StringJsonSource), null);

		public string Json
		{
			get { return (string)GetValue(JsonProperty); }
			set { SetValue(JsonProperty, value); }
		}

		public override Task<string> GetJson()
		{
			return Task.FromResult(Json);
		}
	}
}