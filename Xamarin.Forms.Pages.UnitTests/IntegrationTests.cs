using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Pages.UnitTests
{
	[TestFixture]
	internal class IntegrationTests
	{
		[SetUp]
		public void Setup ()
		{
			Device.PlatformServices = new MockPlatformServices ();
		}

		[TearDown]
		public void TearDown ()
		{
			Device.PlatformServices = null;
		}

		public class SimpleDataSource : BaseDataSource
		{
			readonly ObservableCollection<IDataItem> dataItems = new ObservableCollection<IDataItem> ();
			string json;

			public SimpleDataSource ()
			{
				Json = @"{
 'Name' : 'Jason Smith',
 'Phone' : '(555)248-7561',
 'PrimaryEmail' : 'jason@xamarin.com',
 'JobTitle' : 'Software Engineering Manager',
 'TimeZone' : 'PST',
 'Image' : 'https://c1.staticflickr.com/3/2877/10612763564_7f2d1734ea_b.jpg',
 'Address' : '430 Pacific Ave, San Francisico CA, 55592',
}";
			}

			public string Json
			{
				get { return json; }
				set
				{
					json = value;
					try {
						var dict = JsonConvert.DeserializeObject<Dictionary<string, object>> (json);
						foreach (var kvp in dict)
							dataItems.Add (new DataItem (kvp.Key, kvp.Value));
					} catch (Exception ex) {
						Debug.WriteLine (ex.Message);
					}
				}
			}

			protected override Task<IList<IDataItem>> GetRawData ()
			{
				return Task.FromResult<IList<IDataItem>> (dataItems);
			}

			protected override object GetValue (string key)
			{
				var target = dataItems.FirstOrDefault (d => d.Name == key);
				if (target == null)
					return null;
					//throw new KeyNotFoundException ();
				return target.Value;
			}

			protected override bool SetValue (string key, object value)
			{
				var target = dataItems.FirstOrDefault (d => d.Name == key);
				if (target == null) {
					dataItems.Add (new DataItem (key, value));
					return true;
				}
				if (target.Value == value)
					return false;
				target.Value = value;
				return true;
			}
		}

		public class TestPage : PersonDetailPage
		{
			public TestPage ()
			{
				SetBinding (DisplayNameProperty, new DataSourceBinding ("Name"));
				SetBinding (PhoneNumberProperty, new DataSourceBinding ("Phone"));
				SetBinding (EmailProperty, new DataSourceBinding ("PrimaryEmail"));
			}
		}

		[Test]
		public void Test1 ()
		{
			var page = new TestPage ();
			
			page.DataSource = new SimpleDataSource ();
			page.Platform = new UnitPlatform();

			Assert.AreEqual (9, page.DataSource.MaskedKeys.Count ());
		}

		[Test]
		public void JsonDataSourceComplex ()
		{
			var jsonDataSource = new JsonDataSource ();

			var json = @"[
	{
		'Name': 'Kristen Perez',
		'Address': 'Ap #325-3386 Ac Av.',
		'Phone': '(016977) 7108',
		'Title': 'Lorem Ipsum Dolor Incorporated',
		'Email': 'ac.risus.Morbi@interdum.co.uk',
		'List' : [
          'Foo', 'Bar', 'Baz'
        ]
	},
	{
		'Name': 'Murphy Cote',
		'Address': '906-6938 Porttitor Ave',
		'Phone': '076 9223 8954',
		'Title': 'Vulputate Industries',
		'Email': 'non@consequat.ca',
        'List' : [ { 'Second' : 'Thing' } ]
	},
	{
		'Name': 'Nicole Valdez',
		'Address': '485-9530 Ut Rd.',
		'Phone': '0800 364 0760',
		'Title': 'Diam At Ltd',
		'Email': 'nisl@ipsum.edu'
	}
]";
			jsonDataSource.Source = json;
			Debug.WriteLine (jsonDataSource);
		}
	}
}
