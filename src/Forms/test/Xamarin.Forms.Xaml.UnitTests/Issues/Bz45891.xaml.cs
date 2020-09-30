using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz45891 : ContentPage
	{
		public Bz45891()
		{
			InitializeComponent();
		}

		public Bz45891(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public static readonly BindableProperty ListProperty =
			BindableProperty.Create("List", typeof(IEnumerable<string>), typeof(Bz45891), default(IEnumerable<string>));

		public IEnumerable<string> List
		{
			get { return (IEnumerable<string>)GetValue(ListProperty); }
			set { SetValue(ListProperty, value); }
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

			[TestCase(true)]
			[TestCase(false)]
			public void LookForInheritanceOnOpImplicit(bool useCompiledXaml)
			{
				var p = new Bz45891(useCompiledXaml);
				Assert.AreEqual("Foo", p.List.First());
			}
		}
	}
}