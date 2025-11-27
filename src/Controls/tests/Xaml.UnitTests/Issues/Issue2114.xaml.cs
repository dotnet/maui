using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2114 : Application
{
	public Issue2114() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void SetUp() => Application.Current = null;

		[Test]
		public void StaticResourceOnApplication([Values] XamlInflator inflator)
		{
			Issue2114 app;
			Assert.DoesNotThrow(() => app = new Issue2114(inflator));

			Assert.True(Current.Resources.ContainsKey("ButtonStyle"));
			Assert.True(Current.Resources.ContainsKey("NavButtonBlueStyle"));
			Assert.True(Current.Resources.ContainsKey("NavButtonGrayStyle"));
		}
	}
}