using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foundation;
using NUnit.Framework;

namespace Xamarin.Forms.Platform.iOS.UnitTests
{
	[TestFixture(Category = "CollectionView")]
	public class IndexPathTests
	{
		[Test]
		public void GenerateIndexPathRange() 
		{
			var result = IndexPathHelpers.GenerateIndexPathRange(0, 0, 5);

			Assert.That(result.Length, Is.EqualTo(5));
			Assert.That(result[0].Section, Is.EqualTo(0));
			Assert.That((int)result[0].Item, Is.EqualTo(0));

			Assert.That(result[4].Section, Is.EqualTo(0));
			Assert.That((int)result[4].Item, Is.EqualTo(4));
		}

		[Test]
		public void GenerateIndexPathRangeForLoop()
		{
			// Section 0
			// 5 items, looped 3 times
			// Looking for all the items corresponding to indexes 2, 3, and 4

			var result = IndexPathHelpers.GenerateLoopedIndexPathRange(0, 15, 3, 2, 3);

			// Source:
			// 0, 1, 2, 3, 4, 0, 1, 2, 3, 4,  0,  1,  2,  3,  4
			// 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14


			// Result:
			// 2, 3, 4, 2, 3, 4,  2,  3,  4
			// 2, 3, 4, 7, 8, 9, 12, 13, 14

			Assert.That(result.Length, Is.EqualTo(9));
			
			Assert.That(result[0].Section, Is.EqualTo(0));
			Assert.That(result[1].Section, Is.EqualTo(0));
			Assert.That(result[2].Section, Is.EqualTo(0));
			Assert.That(result[3].Section, Is.EqualTo(0));
			Assert.That(result[4].Section, Is.EqualTo(0));
			Assert.That(result[5].Section, Is.EqualTo(0));
			Assert.That(result[6].Section, Is.EqualTo(0));
			Assert.That(result[7].Section, Is.EqualTo(0));
			Assert.That(result[8].Section, Is.EqualTo(0));

			Assert.That((int)result[0].Item, Is.EqualTo(2));
			Assert.That((int)result[1].Item, Is.EqualTo(3));
			Assert.That((int)result[2].Item, Is.EqualTo(4));
			Assert.That((int)result[3].Item, Is.EqualTo(7));
			Assert.That((int)result[4].Item, Is.EqualTo(8));
			Assert.That((int)result[5].Item, Is.EqualTo(9));
			Assert.That((int)result[6].Item, Is.EqualTo(12));
			Assert.That((int)result[7].Item, Is.EqualTo(13));
			Assert.That((int)result[8].Item, Is.EqualTo(14));
		}

		[Test]
		public void IndexPathValidTest() 
		{
			var list = new List<string>
			{
				"one",
				"two",
				"three"
			};

			var source = new ListSource(list);

			var valid = NSIndexPath.FromItemSection(2, 0);
			var invalidItem = NSIndexPath.FromItemSection(7, 0);
			var invalidSection = NSIndexPath.FromItemSection(1, 9);

			Assert.IsTrue(source.IsIndexPathValid(valid));
			Assert.IsFalse(source.IsIndexPathValid(invalidItem));
			Assert.IsFalse(source.IsIndexPathValid(invalidSection));
		}
	}
}