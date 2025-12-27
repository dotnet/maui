using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13872 : ContentPage
{
	public Maui13872() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CompiledBindingToIReadOnlyListCount(XamlInflator inflator)
		{
			var page = new Maui13872(inflator);
			page.BindingContext = new Maui13872ViewModel();

			// Uncompiled bindings (no x:DataType) - should work with all inflators
			Assert.Equal("3", page.label0.Text);
			Assert.Equal("3", page.label1.Text);

			// Compiled bindings (with x:DataType) - IReadOnlyList<T>.Count should resolve correctly.
			// Count is defined on IReadOnlyCollection<T> which IReadOnlyList<T> inherits.
			Assert.Equal("3", page.label2.Text);
			Assert.Equal("3", page.label3.Text);
		}
	}
}

public class Maui13872ViewModel
{
	private readonly string[] _list = ["Bill", "Steve", "John"];

	public IReadOnlyList<string> List => _list;

	public int ListCount => _list.Length;
}