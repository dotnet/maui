using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DataTemplateSelectorTests : BaseTestFixture
	{
		class TemplateOne : DataTemplate
		{
			public TemplateOne() : base(typeof(ViewCell))
			{

			}
		}

		class TemplateTwo : DataTemplate
		{
			public TemplateTwo() : base(typeof(EntryCell))
			{

			}
		}

		class TestDTS : DataTemplateSelector
		{
			public TestDTS()
			{
				templateOne = new TemplateOne();
				templateTwo = new TemplateTwo();
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				if (item is double)
					return templateOne;
				if (item is byte)
					return new TestDTS();
				return templateTwo;
			}

			readonly DataTemplate templateOne;
			readonly DataTemplate templateTwo;
		}

		[Fact]
		public void Constructor()
		{
			var dts = new TestDTS();
		}

		[Fact]
		public void ReturnsCorrectType()
		{
			var dts = new TestDTS();
			Assert.IsType<TemplateOne>(dts.SelectTemplate(1d, null));
			Assert.IsType<TemplateTwo>(dts.SelectTemplate("test", null));
		}

		[Fact]
		public void ListViewSupport()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);
			listView.ItemsSource = new object[] { 0d, "test" };

			listView.ItemTemplate = new TestDTS();
			Assert.IsType<ViewCell>(listView.TemplatedItems[0]);
			Assert.IsType<EntryCell>(listView.TemplatedItems[1]);
		}

		[Fact]
		public void NestingThrowsException()
		{
			var dts = new TestDTS();
			Assert.Throws<NotSupportedException>(() => dts.SelectTemplate((byte)0, null));
		}
	}


	public class DataTemplateRecycleTests : BaseTestFixture
	{
		public DataTemplateRecycleTests()
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
		}

		class TestDataTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate declarativeTemplate;
			readonly DataTemplate proceduralTemplate;

			public TestDataTemplateSelector()
			{
				declarativeTemplate = new DataTemplate(typeof(ViewCell));
				proceduralTemplate = new DataTemplate(() => new EntryCell());
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				Counter++;

				if (item is string)
					return declarativeTemplate;

				return proceduralTemplate;
			}

			public int Counter = 0;
		}

		[Fact]
		public void ListViewSupport()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElementAndDataTemplate);
			listView.ItemsSource = new object[] { "foo", "bar", 0 };

			Assert.Equal(ListViewCachingStrategy.RecycleElementAndDataTemplate, listView.CachingStrategy);

			var selector = new TestDataTemplateSelector();
			listView.ItemTemplate = selector;
			Assert.True(selector.Counter == 0);

			Assert.IsType<ViewCell>(listView.TemplatedItems[0]);
			Assert.True(selector.Counter == 1);

			Assert.IsType<ViewCell>(listView.TemplatedItems[1]);
			Assert.True(selector.Counter == 1);

			Assert.Throws<NotSupportedException>(
				() => { var o = listView.TemplatedItems[2]; });
		}
	}
}