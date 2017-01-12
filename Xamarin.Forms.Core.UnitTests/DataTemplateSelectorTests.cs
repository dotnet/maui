using System;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DataTemplateSelectorTests : BaseTestFixture
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		class TemplateOne : DataTemplate
		{
			public TemplateOne () : base (typeof (ViewCell))
			{
				
			}
		}

		class TemplateTwo : DataTemplate
		{
			public TemplateTwo () : base (typeof (EntryCell))
			{
				
			}
		}

		class TestDTS : DataTemplateSelector
		{
			public TestDTS ()
			{
				templateOne = new TemplateOne ();
				templateTwo = new TemplateTwo ();
			}

			protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
			{
				if (item is double)
					return templateOne;
				if (item is byte)
					return new TestDTS ();
				return templateTwo;
			}

			readonly DataTemplate templateOne;
			readonly DataTemplate templateTwo;
		}

		[Test]
		public void Constructor ()
		{
			var dts = new TestDTS ();
		}

		[Test]
		public void ReturnsCorrectType ()
		{
			var dts = new TestDTS ();
			Assert.IsInstanceOf<TemplateOne> (dts.SelectTemplate (1d, null));
			Assert.IsInstanceOf<TemplateTwo> (dts.SelectTemplate ("test", null));
		}

		[Test]
		public void ListViewSupport ()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);
			listView.ItemsSource = new object[] { 0d, "test" };

			listView.ItemTemplate = new TestDTS ();
			Assert.IsInstanceOf<ViewCell> (listView.TemplatedItems[0]);
			Assert.IsInstanceOf<EntryCell> (listView.TemplatedItems[1]);
		}

		[Test]
		public void NestingThrowsException ()
		{
			var dts = new TestDTS ();
			Assert.Throws<NotSupportedException> (() => dts.SelectTemplate ((byte)0, null));
		}
	}
}