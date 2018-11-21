using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh4438VM : Gh4438VMBase<string>
	{
		public Gh4438VM()
		{
			Add("test");
			SelectedItem = this.First();
		}
	}

	public class Gh4438VMBase<T> : Collection<string>
	{
		public virtual T SelectedItem { get; set; }
	}

	public partial class Gh4438 : ContentPage
	{
		public Gh4438()
		{
			InitializeComponent();
		}

		public Gh4438(bool useCompiledXaml)
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
			public void GenericBaseClassResolution(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4438)));
				var layout = new Gh4438(useCompiledXaml) { BindingContext = new Gh4438VM() };
				Assert.That(layout.label.Text, Is.EqualTo("test"));
			}
		}
	}
}
