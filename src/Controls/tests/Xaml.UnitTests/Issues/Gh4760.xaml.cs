using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public abstract class Gh4760Base<T> : IMarkupExtension<T>
	{
		public abstract T ProvideValue();
		public T ProvideValue(IServiceProvider serviceProvider) => ProvideValue();
		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
		  ProvideValue(serviceProvider);
	}

	[AcceptEmptyServiceProvider]
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
			[Test]
			public void GenericBaseClassForMarkups([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4760)));
				var layout = new Gh4760(useCompiledXaml);
				Assert.That(layout.label.Scale, Is.EqualTo(6));
			}
		}
	}
}
