using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Gh5706 : Shell
	{
		class VM
		{
			public VM()
			{
				FilterCommand = new Command((p) => Param = p);
			}

			public Command FilterCommand { get; set; }

			public object Param { get; set; }
		}

		public Gh5706() => InitializeComponent();
		public Gh5706(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture] class Tests
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

			[Test]
			public void ReportSyntaxError([Values(false, true)]bool useCompiledXaml)
			{
				var layout = new Gh5706(useCompiledXaml);
				layout.searchHandler.BindingContext = new VM();

				Assert.That(layout.searchHandler.CommandParameter, Is.Null);
				layout.searchHandler.Query = "Foo";
				Assert.That(layout.searchHandler.CommandParameter, Is.EqualTo("Foo"));
			}
		}
	}
}
