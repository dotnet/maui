using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh4516VM
	{
		public Uri[] Images { get; } = { };
	}

	public partial class Gh4516 : ContentPage
	{
		public Gh4516() => InitializeComponent();
		public Gh4516(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void BindingToEmptyCollection(bool useCompiledXaml)
			{
				Gh4516 layout = null;
				Assert.DoesNotThrow(() => layout = new Gh4516(useCompiledXaml) { BindingContext = new Gh4516VM() });
				Assert.That((layout.image.Source as FileImageSource).File, Is.EqualTo("foo.jpg"));
			}
		}
	}
}