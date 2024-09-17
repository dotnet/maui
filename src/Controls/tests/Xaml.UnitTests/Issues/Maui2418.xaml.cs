using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
#if DEBUG
	[XamlProcessing(XamlInflator.Default, true)]
	public partial class Maui2418 : ContentPage
	{
		public Maui2418() => InitializeComponent();

		[TestFixture]
		class Tests
		{
			[Test] public void SourceInfoIsRelative([Values(XamlInflator.Runtime, XamlInflator.SourceGen)] XamlInflator inflator)
			{
				var page = new Maui2418(inflator);
				Assert.That(page, Is.Not.Null);
				var label0 = page.label0;
				var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
				Assert.That(sourceInfo.SourceUri.OriginalString, Is.EqualTo($"Issues{System.IO.Path.DirectorySeparatorChar}Maui2418.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"));
			}
		}
	}
#endif
}
