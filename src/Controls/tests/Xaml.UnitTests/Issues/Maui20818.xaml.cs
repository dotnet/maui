using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20818
{
	public Maui20818() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void TypeLiteralAndXTypeCanBeUsedInterchangeably([Values] XamlInflator inflator)
		{
			var page = new Maui20818(inflator);

			Assert.That((page.Resources["A"] as Style).TargetType, Is.EqualTo(typeof(Label)));
			Assert.That((page.Resources["B"] as Style).TargetType, Is.EqualTo(typeof(Label)));

			Assert.That(page.TriggerC.TargetType, Is.EqualTo(typeof(Label)));
			Assert.That(page.TriggerD.TargetType, Is.EqualTo(typeof(Label)));
			Assert.That(page.TriggerE.TargetType, Is.EqualTo(typeof(Label)));
			Assert.That(page.TriggerF.TargetType, Is.EqualTo(typeof(Label)));
			Assert.That(page.TriggerG.TargetType, Is.EqualTo(typeof(Label)));
			Assert.That(page.TriggerH.TargetType, Is.EqualTo(typeof(Label)));
		}
	}
}
