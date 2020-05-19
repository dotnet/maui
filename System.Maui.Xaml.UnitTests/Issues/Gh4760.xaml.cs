using System;

using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public abstract class Gh4760Base<T> : IMarkupExtension<T>
	{
		public abstract T ProvideValue();
		public T ProvideValue(IServiceProvider serviceProvider) => ProvideValue();
		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
		  ProvideValue(serviceProvider);
	}

	public class Gh4760MultiplyExtension : Gh4760Base<double>
	{
		public double Base { get; set; }
		public double By { get; set; }

		public override double ProvideValue() => Base * By;
	}

	public partial class Gh4760 : ContentPage
	{
		public Gh4760() => InitializeComponent();
		public Gh4760(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void GenericBaseClassForMarkups([Values (false, true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4760)));
				var layout = new Gh4760(useCompiledXaml);
				Assert.That(layout.label.Scale, Is.EqualTo(6));
			}
		}
	}
}
