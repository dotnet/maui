using System;
using System.Collections.Generic;

using NUnit.Framework;

using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public class Gh4751VM
	{
		public string Title { get; }
		public Gh4751VM(string title = null) => Title = title; //a .ctor with a default value IS NOT a default .ctor
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh4751 : ContentPage
	{
		public Gh4751() => InitializeComponent();
		public Gh4751(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void ErrorOnMissingDefaultCtor([Values (false, true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh4751)));
				else
					Assert.Throws<XamlParseException>(() => new Gh4751(useCompiledXaml));
			}
		}
	}
}
