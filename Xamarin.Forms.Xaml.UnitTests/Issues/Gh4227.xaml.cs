using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public interface IGh4227Level0
	{
		string Level0 { get; }
	}

	public interface IGh4227Level1 : IGh4227Level0
	{
		string Level1 { get; }
	}

	public class Gh4227VM : IGh4227Level1
	{
		public string Level0 => "level0";
		public string Level1 => "level1";
	}

	public partial class Gh4227 : ContentPage
	{
		public Gh4227()
		{
			InitializeComponent();
		}

		public Gh4227(bool useCompiledXaml)
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

			[TestCase(true), TestCase(false)]
			public void FindMemberOnInterfaces(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4227)));
				var layout = new Gh4227(useCompiledXaml) { BindingContext = new Gh4227VM() };
				Assert.That(layout.label0.Text, Is.EqualTo("level0"));
				Assert.That(layout.label1.Text, Is.EqualTo("level1"));
			}
		}
	}
}