using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void GenericBaseClassForMarkups([Values] XamlInflator inflator)
		{
			var layout = new Gh4760(inflator);
			Assert.That(layout.label.Scale, Is.EqualTo(6));
		}
	}
}
