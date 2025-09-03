// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh7837VMBase //using a base class to test #2131
{
	public string this[int index] => "";
	public string this[string index] => "";
}

public class Gh7837VM : Gh7837VMBase
{
	public new string this[int index] => index == 42 ? "forty-two" : "dull number";
	public new string this[string index] => index.ToUpperInvariant();
}

public partial class Gh7837 : ContentPage
{
	public Gh7837() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingWithMultipleIndexers([Values] XamlInflator inflator)
		{
			var layout = new Gh7837(inflator);
			Assert.That(layout.label0.Text, Is.EqualTo("forty-two"));
			Assert.That(layout.label1.Text, Is.EqualTo("FOO"));
			Assert.That(layout.label2.Text, Is.EqualTo("forty-two"));
			Assert.That(layout.label3.Text, Is.EqualTo("FOO"));
		}
	}
}
