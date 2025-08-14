using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui19535CustomThemeDictionary : ResourceDictionary
{
}

public partial class Maui19535 : Maui19535CustomThemeDictionary
{
	public Maui19535() => InitializeComponent();

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
		public void SubClassOfRDShouldNotThrow([Values] XamlInflator inflator)
		{
			var rd = new Maui19535(inflator);
			Assert.That(rd.Count, Is.EqualTo(3));
			Assert.True(rd.TryGetValue("CustomTheme", out var theme));
			Assert.That(theme, Is.EqualTo("LightTheme"));
		}
	}

}