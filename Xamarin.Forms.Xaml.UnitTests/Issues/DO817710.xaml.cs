using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class DO817710 : ContentPage
	{
		public DO817710() => InitializeComponent();
		public DO817710(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void EmptyResourcesElement([Values(false, true)]bool useCompiledXaml)
			{
				Assert.DoesNotThrow(() => new DO817710(useCompiledXaml: useCompiledXaml));
			}
		}
	}
}
