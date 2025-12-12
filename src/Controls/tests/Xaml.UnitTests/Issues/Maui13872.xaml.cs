using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13872 : ContentPage
{
	public Maui13872() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CompiledBindingToIReadOnlyListCount([Values] XamlInflator inflator)
		{
			var page = new Maui13872(inflator);
			page.BindingContext = new Maui13872ViewModel();

			// Uncompiled bindings (no x:DataType) - should work with all inflators
			Assert.That(page.label0.Text, Is.EqualTo("3"), "Uncompiled binding to List.Count");
			Assert.That(page.label1.Text, Is.EqualTo("3"), "Uncompiled binding to ListCount");

			// Compiled bindings (with x:DataType) - IReadOnlyList<T>.Count should resolve correctly.
			// Count is defined on IReadOnlyCollection<T> which IReadOnlyList<T> inherits.
			Assert.That(page.label2.Text, Is.EqualTo("3"), "Compiled binding to List.Count");
			Assert.That(page.label3.Text, Is.EqualTo("3"), "Compiled binding to ListCount");
		}
	}
}

public class Maui13872ViewModel
{
	private readonly string[] _list = ["Bill", "Steve", "John"];

	public IReadOnlyList<string> List => _list;

	public int ListCount => _list.Length;
}
