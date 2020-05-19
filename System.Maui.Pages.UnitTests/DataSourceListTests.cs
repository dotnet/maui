using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Pages.UnitTests
{
	[TestFixture]
	public class DataSourceListTests
	{
		public class TestDataSource : BaseDataSource
		{
			public ObservableCollection<IDataItem> DataItems { get; } = new ObservableCollection<IDataItem> ();

			protected override Task<IList<IDataItem>> GetRawData ()
			{
				return Task.FromResult<IList<IDataItem>> (DataItems);
			}

			protected override object GetValue (string key)
			{
				var target = DataItems.FirstOrDefault (d => d.Name == key);
				if (target == null)
					throw new KeyNotFoundException ();
				return target.Value;
			}

			protected override bool SetValue (string key, object value)
			{
				var target = DataItems.FirstOrDefault (d => d.Name == key);
				if (target == null)
					DataItems.Add (new DataItem (key, value));
				else if (target.Value == value)
					return false;
				else
					target.Value = value;
				return true;
			}
		}

		[Test]
		public void DataSourceListIndexer ()
		{
			var source = new TestDataSource ();
			IDataSource s = source;
			source.DataItems.Add (new DataItem ("Foo", "Bar"));

			Assert.AreEqual ("Bar", s["Foo"]);

			Assert.AreEqual ("Bar", s.Data[0].Value);
		}

		[Test]
		public void CompoundListPrepend ()
		{
			var source = new TestDataSource ();
			IDataSource s = source;
			source.DataItems.Add (new DataItem ("Foo1", "Bar1"));
			source.DataItems.Add (new DataItem ("Foo2", "Bar2"));

			var compoundList = new CompoundCollection {
				MainList = s.Data
			};
			var prependItem = new DataItem ("Pre1", "Val1");
			compoundList.PrependList.Add (prependItem);

			Assert.AreEqual (prependItem, compoundList[0]);
			Assert.AreEqual (source.DataItems[0], s.Data[0]);
			Assert.AreEqual (source.DataItems[1], s.Data[1]);

			Assert.AreEqual (source.DataItems[0], compoundList[1]);
			Assert.AreEqual (source.DataItems[1], compoundList[2]);

			s.MaskKey ("Foo1");

			Assert.AreEqual (source.DataItems[1], compoundList[1]);
		}
	}
}