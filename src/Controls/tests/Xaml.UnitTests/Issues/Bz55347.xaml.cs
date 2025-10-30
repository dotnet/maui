using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz55347 : ContentPage
{
	public Bz55347()
	{
	}

	[TestFixture]
	class Tests
	{
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void PaddingThicknessResource([Values] XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary {
					{"Padding", new Thickness(8)}
				}
			};
			var layout = new Bz55347(inflator);
		}
	}
}