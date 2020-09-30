using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz27863 : ContentPage
	{
		public Bz27863()
		{
			InitializeComponent();
		}

		public Bz27863(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void DataTemplateInResourceDictionaries(bool useCompiledXaml)
			{
				var layout = new Bz27863(useCompiledXaml);
				var listview = layout.Resources["listview"] as ListView;
				Assert.NotNull(listview.ItemTemplate);
				var template = listview.ItemTemplate;
				var cell = template.CreateContent() as ViewCell;
				cell.BindingContext = "Foo";
				Assert.AreEqual("ooF", ((Label)((StackLayout)cell.View).Children[0]).Text);
			}
		}
	}
}