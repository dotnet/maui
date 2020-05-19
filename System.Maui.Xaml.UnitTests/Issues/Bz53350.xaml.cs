using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz53350Generic<T> : ContentView
	{
		public static readonly BindableProperty SomeBPProperty =
			BindableProperty.Create("SomeBP", typeof(T), typeof(Bz53350Generic<T>), default(T));

		public T SomeBP
		{
			get { return (T)GetValue(SomeBPProperty); }
			set { SetValue(SomeBPProperty, value); }
		}

		public T SomeProperty { get; set; }
	}

	public class Bz53350String : Bz53350Generic<string>
	{
		
	}

	public partial class Bz53350
	{
		public Bz53350()
		{
		}

		public Bz53350(bool useCompiledXaml)
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
				Application.Current = null;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void PropertiesWithGenericType(bool useCompiledXaml)
			{
				var layout = new Bz53350(useCompiledXaml);
				Assert.That(layout.content.SomeBP, Is.EqualTo("Foo"));
				Assert.That(layout.content.SomeProperty, Is.EqualTo("Bar"));
			}
		}
	}
}
