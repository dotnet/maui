using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz42531 : ContentPage
	{
		public Bz42531()
		{
			InitializeComponent();
		}

		public Bz42531(bool useCompiledXaml)
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
			public void RDInDataTemplates(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Bz42531));
				var p = new Bz42531(useCompiledXaml);
				ListView lv = p.lv;
				var template = lv.ItemTemplate;
				var cell = template.CreateContent(null, lv) as ViewCell;
				var sl = cell.View as StackLayout;
				Assert.AreEqual(1, sl.Resources.Count);
				var label = sl.Children[0] as Label;
				Assert.AreEqual(LayoutOptions.Center, label.HorizontalOptions);
			}
		}
	}
}