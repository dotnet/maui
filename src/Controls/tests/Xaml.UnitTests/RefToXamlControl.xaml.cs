using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class RefToXamlControl : ContentPage
{
	public RefToXamlControl() => InitializeComponent();


	[TestFixture]
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
		public void CanRefToXamlControlsWithoutBaseClass([Values] XamlInflator inflator)
		{
			var page = new RefToXamlControl(inflator);
			Assert.That(page.Content, Is.TypeOf<CustomButtonNoBaseClass>());

		}
		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;

	}
}
