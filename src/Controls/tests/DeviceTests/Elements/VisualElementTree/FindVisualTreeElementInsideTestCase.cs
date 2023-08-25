using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit.Abstractions;

namespace Microsoft.Maui.DeviceTests
{
	public class FindVisualTreeElementInsideTestCase : IXunitSerializable
	{

		public string TestCaseName { get; private set; }

		public FindVisualTreeElementInsideTestCase() { }

		public FindVisualTreeElementInsideTestCase(string testCaseName)
		{
			TestCaseName = testCaseName;
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			TestCaseName = info.GetValue<string>(nameof(TestCaseName));
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(TestCaseName), TestCaseName, typeof(string));
		}

		public override string ToString()
		{
			return TestCaseName;
		}

		public (VisualElement rootView, VisualElement testView) CreateVisualElement()
		{
			switch (TestCaseName)
			{
				case "CollectionView":
					{
						var cv = new CollectionView();
						NestingView view = new NestingView();
						cv.ItemTemplate = new DataTemplate(() =>
						{
							return view;
						});
						cv.ItemsSource = new[] { 0 };
						return (cv, view);
					}
				case "ContentView":
					{
						var contentView = new ContentView();
						NestingView view = new NestingView();
						contentView.ControlTemplate = new ControlTemplate(() =>
						{
							return view;
						});
						return (contentView, view);
					}
			}

			throw new Exception(String.Concat(TestCaseName, " ", "Not Found"));
		}
	}

	public class FindVisualTreeElementInsideTestCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
			{
				new object[] { new FindVisualTreeElementInsideTestCase("CollectionView") },
				new object[] { new FindVisualTreeElementInsideTestCase("ContentView") }
			};

		public IEnumerator<object[]> GetEnumerator()
			=> _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}