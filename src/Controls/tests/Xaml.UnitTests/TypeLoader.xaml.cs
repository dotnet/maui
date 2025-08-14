using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeLoader : ContentPage
{
	public TypeLoader() => InitializeComponent();

	class Tests
	{
		[SetUp] public void SetUp() => Application.Current = new MockApplication();

		[Test]
		public void LoadTypeFromXmlns([Values] XamlInflator inflator)
		{
			TypeLoader layout = null;
			Assert.DoesNotThrow(() => layout = new TypeLoader(inflator));
			Assert.NotNull(layout.customview0);
			Assert.That(layout.customview0, Is.TypeOf<CustomView>());
		}

		[Test]
		public void LoadTypeFromXmlnsWithoutAssembly([Values] XamlInflator inflator)
		{
			TypeLoader layout = null;
			Assert.DoesNotThrow(() => layout = new TypeLoader(inflator));
			Assert.NotNull(layout.customview1);
			Assert.That(layout.customview1, Is.TypeOf<CustomView>());
		}
	}
}