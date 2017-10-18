using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Xaml.UnitTests
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
			IReadOnlyList<string> _flags;

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				_flags = Device.Flags;
				if (Device.Flags == null)
					Device.SetFlags(new List<string>().AsReadOnly());
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Device.SetFlags(_flags);
			}

			[TestCase(true, "xamlDoubleImplicitOpHack")]
			[TestCase(false, "xamlDoubleImplicitOpHack")]
			[TestCase(true, null)]
			[TestCase(false, null)]
			public void Bz59818(bool useCompiledXaml, string flag)
			{
				Device.SetFlags(new List<string>(Device.Flags) {
					flag
				}.AsReadOnly());

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;

				if (flag != "xamlDoubleImplicitOpHack") {
					if (useCompiledXaml)
						Assert.Throws<InvalidCastException>(() => new Bz59818(useCompiledXaml));
					else
						Assert.Throws<XamlParseException>(() => new Bz59818(useCompiledXaml));
					return;
				}
				var layout = new Bz59818(useCompiledXaml);
				Assert.That(layout.grid.ColumnDefinitions[0].Width, Is.EqualTo(new GridLength(100)));
			}
		}
	}
}
