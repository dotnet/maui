using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh2752VM
	{
		public Gh2752VM Foo { get; set; }
		public Gh2752VM Bar { get; set; }
		public string Baz { get; set; }
	}
	public partial class Gh2752 : ContentPage
	{
		public static readonly BindableProperty MyProperty =
			BindableProperty.Create("My", typeof(string), typeof(Gh2752), default(string), defaultValueCreator: b => "default created value");

		public string My
		{
			get { return (string)GetValue(MyProperty); }
			set { SetValue(MyProperty, value); }
		}

		public Gh2752() => InitializeComponent();
		public Gh2752(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void FallbcakToDefaultValueCreator(bool useCompiledXaml)
			{
				var layout = new Gh2752(useCompiledXaml) { BindingContext = null };
				Assert.That(layout.My, Is.EqualTo("default created value"));
			}
		}
	}
}