using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace Xamarin.Forms.Pages.Azure
{
	public class AzureEasyTableSource : AzureSource
	{
		public static readonly BindableProperty TableNameProperty =
			BindableProperty.Create(nameof(TableName), typeof(string), typeof(AzureEasyTableSource), null);

		public string TableName
		{
			get { return (string)GetValue(TableNameProperty); }
			set { SetValue(TableNameProperty, value); }
		}

		public override async Task<JToken> GetJson()
		{
			var mobileServiceClient = new MobileServiceClient(Uri);
			var table = mobileServiceClient.GetTable(TableName);
			return await table.ReadAsync(string.Empty);
		}
	}
}