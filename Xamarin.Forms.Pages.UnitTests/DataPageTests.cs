using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Pages.UnitTests
{
	[TestFixture]
	public class DataPageTests
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

		class TestDataPage : DataPage
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create (nameof (Name), typeof (string), typeof (TestDataPage), null);

			public string Name
			{
				get { return (string)GetValue (NameProperty); }
				set { SetValue (NameProperty, value); }
			}

			public TestDataPage ()
			{
				SetBinding (NameProperty, new DataSourceBinding ("Name"));
			}
		}

		class DataSource : BaseDataSource
		{
			ObservableCollection<IDataItem> data = new ObservableCollection<IDataItem> (); 
			protected override Task<IList<IDataItem>> GetRawData ()
			{
				return Task.FromResult<IList<IDataItem>> (data);
			}

			protected override object GetValue (string key)
			{
				var target = data.FirstOrDefault (d => d.Name == key);
				if (target == null)
					throw new KeyNotFoundException ();
				return target.Value;
			}

			protected override bool SetValue (string key, object value)
			{
				var target = data.FirstOrDefault (d => d.Name == key);
				if (target == null)
					data.Add (new DataItem (key, value));
				else if (target.Value == value)
					return false;
				else
					target.Value = value;
				return true;
			}
		}

		[Test]
		public void DefaultBindingsLoad ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["Name"] = "Jason";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.DataSource = dataSource;

			Assert.AreEqual ("Jason", detailpage.Name);
		}

		[Test]
		public void RebindingDataSource ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["UserName"] = "Jason";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.SetBinding (TestDataPage.NameProperty, new DataSourceBinding ("UserName"));
			detailpage.DataSource = dataSource;

			Assert.AreEqual ("Jason", detailpage.Name);
		}

		[Test]
		public void RebindingDataSourceNotMasked ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["UserName"] = "Jason";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.DataSource = dataSource;

			detailpage.SetBinding (TestDataPage.NameProperty, new DataSourceBinding ("UserName"));
			Assert.AreEqual ("Jason", detailpage.Name);

			Assert.AreEqual (1, detailpage.DataSource.MaskedKeys.Count ());
		}

		[Test]
		public void UpdateDataSource ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["UserName"] = "Jason";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.SetBinding (TestDataPage.NameProperty, new DataSourceBinding ("UserName"));
			detailpage.DataSource = dataSource;

			dataSource["UserName"] = "Jerry";

			Assert.AreEqual ("Jerry", detailpage.Name);
		}

		[Test]
		public void MaskedItemsNotInData ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["Name"] = "Jason";
			dataSource["Other"] = "Foo";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.DataSource = dataSource;

			Assert.AreEqual ("Jason", detailpage.Name);

			Assert.AreEqual (1, detailpage.Data.Count ());
			Assert.AreEqual ("Other", detailpage.Data.First ().Name);
		}

		[Test]
		public void TwoWayDataSourceBinding ()
		{
			IDataSource dataSource = new DataSource ();
			dataSource["UserName"] = "Jason";

			var detailpage = new TestDataPage ();
			detailpage.Platform = new UnitPlatform ();
			detailpage.SetBinding (TestDataPage.NameProperty, new DataSourceBinding ("UserName", BindingMode.TwoWay));
			detailpage.DataSource = dataSource;

			((IElementController)detailpage).SetValueFromRenderer (TestDataPage.NameProperty, "John");

			Assert.AreEqual ("John", dataSource["UserName"]);
		}
	}
}
