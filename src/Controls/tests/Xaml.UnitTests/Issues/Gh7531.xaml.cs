// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh7531 : ContentPage
{
	public Gh7531() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XamlOnlyResourceResolvesLocalAssembly([Values] XamlInflator inflator)
		{
			Gh7531 layout = null;
			Assert.DoesNotThrow(() => layout = new Gh7531(inflator));
			var style = ((ResourceDictionary)layout.Resources["Colors"])["style"] as Style;
			Assert.That(style.TargetType, Is.EqualTo(typeof(Gh7531)));
		}
	}
}