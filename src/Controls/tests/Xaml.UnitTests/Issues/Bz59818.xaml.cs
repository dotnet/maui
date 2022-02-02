using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz59818 : ContentPage
	{
		public Bz59818()
		{
			InitializeComponent();
		}

		public Bz59818(bool useCompiledXaml)
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

			[TestCase(true, true)]
			[TestCase(false, true)]
			[TestCase(true, false)]
			[TestCase(false, false)]
			public void Bz59818(bool useCompiledXaml, bool xamlDoubleImplicitOpHack)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;

				StaticResourceExtension.XamlDoubleImplicitOperation = xamlDoubleImplicitOpHack;

				if (!xamlDoubleImplicitOpHack)
				{
					if (useCompiledXaml)
						Assert.Throws<InvalidCastException>(() => new Bz59818(useCompiledXaml));
					else
						Assert.Throws<XamlParseException>(() => new Bz59818(useCompiledXaml));
				}
				else
				{
					var layout = new Bz59818(useCompiledXaml);
					Assert.That(layout.grid.ColumnDefinitions[0].Width, Is.EqualTo(new GridLength(100)));
				}

				StaticResourceExtension.XamlDoubleImplicitOperation = false;
			}
		}
	}
}
