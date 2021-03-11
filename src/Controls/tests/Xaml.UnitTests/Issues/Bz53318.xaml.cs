using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz53318ListView : ListView
	{
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz53318 : ContentPage
	{
		public Bz53318()
		{
			InitializeComponent();
		}

		[TestFixture]
		public class Tests
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
			public void DoesCompilesArgsInsideDataTemplate()
			{
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Bz53318)));
			}
		}
	}
}