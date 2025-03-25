using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public partial class EnumerableExtensionTests
	{
		public static TheoryData<IEnumerable<int>, int, int> TestData()
		{
			var data = new TheoryData<IEnumerable<int>, int, int>();

			var list = Enumerable.Range(0, 10).ToList();
			var array = Enumerable.Range(0, 10).ToArray();
			var hashSet = Enumerable.Range(0, 10).ToHashSet();

			var collections = new List<IEnumerable<int>> { list, array, hashSet };

			foreach (var collection in collections)
			{
				data.Add(collection, 99, -1);
				data.Add(collection, 2, 2);
			}

			return data;
		}

		public static TheoryData<IEnumerable<string>, Func<string, int>, IDictionary<int, List<string>>> GroupToDictionaryData()
		{
			var data = new TheoryData<IEnumerable<string>, Func<string, int>, IDictionary<int, List<string>>>();

			var list = new List<string> { "apple", "banana", "cherry", "date", "elderberry" };
			var array = new string[] { "apple", "banana", "cherry", "date", "elderberry" };
			var hashSet = new HashSet<string> { "apple", "banana", "cherry", "date", "elderberry" };

			var collections = new List<IEnumerable<string>> { list, array, hashSet };

			Func<string, int> func = s => s.Length;

			foreach (var collection in collections)
			{
				var expected = collection.GroupBy(func).ToDictionary(g => g.Key, g => g.ToList());
				data.Add(collection, func, expected);
			}

			return data;
		}

		[Fact]
		public void ForEachDoesLoop()
		{
			int count = 0;

			int[] array = Enumerable.Range(0, 10).ToArray();

			EnumerableExtensions.ForEach(array, i => count++);

			Assert.Equal(count, array.Length);
		}

		[Theory]
		[Obsolete]
		[MemberData(nameof(GroupToDictionaryData))]
		public void GroupToDictionaryTest(IEnumerable<string> collection, Func<string, int> func, IDictionary<int, List<string>> expected)
		{
			var result = EnumerableExtensions.GroupToDictionary(collection, func);

			Assert.Equal(expected, result);
		}

		[Theory]
		[Obsolete]
		[MemberData(nameof(TestData))]
		public void IndexOfPredicateSucceeds(IEnumerable<int> collection, int value, int expectedIndex)
		{
			var index = EnumerableExtensions.IndexOf(collection, i => i == value);

			Assert.Equal(expectedIndex, index);
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void IndexOfGenericSucceeds(IEnumerable<int> collection, int value, int expectedIndex)
		{
			var index = EnumerableExtensions.IndexOf(collection, value);

			Assert.Equal(expectedIndex, index);
		}

		[Theory]
		[MemberData(nameof(TestData))]
		public void IndexOfSucceeds(IEnumerable<int> collection, int value, int expectedIndex)
		{
			var enumerable = collection as IEnumerable;

			var index = EnumerableExtensions.IndexOf(enumerable, value);

			Assert.Equal(expectedIndex, index);
		}

		[Fact]
		public void LastSucceeds()
		{
			var array = Enumerable.Range(0, 10).ToArray();

			var index = EnumerableExtensions.Last(array);

			Assert.Equal(9, index);
		}
	}
}