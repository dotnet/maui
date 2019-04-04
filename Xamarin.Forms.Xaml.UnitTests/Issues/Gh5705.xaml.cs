using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh5705 : Shell
	{
		public Gh5705() => InitializeComponent();
		public Gh5705(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture] class Tests
		{
			IReadOnlyList<string> _flags;

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				_flags = Device.Flags;
				Device.SetFlags(new List<string>() { ExperimentalFlags.ShellExperimental});
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Device.SetFlags(_flags);
			}

			[Test]
			public void SearchHandlerIneritBC([Values(false, true)]bool useCompiledXaml)
			{
				var vm = new object();
				var shell = new Gh5705(useCompiledXaml) { BindingContext = vm };
				Assert.That(shell.searchHandler.BindingContext, Is.EqualTo(vm));
			}
		}
	}
}
