using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui35018 : ContentPage
{
	public Maui35018()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		protected internal override void Setup()
		{
			base.Setup();
			var app = new MockApplication();
			app.Resources.Add("Headline", new Style(typeof(Label))
			{
				Setters = { new Setter { Property = Label.FontSizeProperty, Value = 32.0 } }
			});
			Application.SetCurrentApplication(app);
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		protected internal override void TearDown()
		{
			AppInfo.SetCurrent(null);
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			base.TearDown();
		}

#if DEBUG
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void InitialLoadWithResourceSucceeds(XamlInflator inflator)
		{
			// "Headline" exists in Application.Resources — page loads fine
			var page = new Maui35018(inflator);
			Assert.NotNull(page);
			Assert.Equal(32.0, page.headlineLabel.FontSize);
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void HotReloadAfterResourceRenameShouldNotCrash(XamlInflator inflator)
		{
			// Issue #35018: During Hot Reload the user renames a resource key in Styles.xaml
			// ("Headline" → "Headline2"). The page still references {StaticResource Headline},
			// which no longer exists. PR #33859 changed StaticResourceExtension to always throw
			// even when ExceptionHandler2 is set. On iOS this propagates through UIKit lifecycle
			// callbacks during Shell item setup, corrupting Shell state and crashing the app.
			//
			// Expected: When ExceptionHandler2 is set (Hot Reload context), a missing
			// StaticResource should be reported to the handler but the page should still load
			// gracefully (with degraded styling), not throw and crash the app.

			// Step 1: Initial load succeeds
			var page = new Maui35018(inflator);
			Assert.NotNull(page);

			// Step 2: Simulate Hot Reload resource rename ("Headline" → "Headline2")
			Application.Current.Resources.Remove("Headline");
			Application.Current.Resources.Add("Headline2", new Style(typeof(Label))
			{
				Setters = { new Setter { Property = Label.FontSizeProperty, Value = 32.0 } }
			});

			// Step 3: ExceptionHandler2 is set (IDE Hot Reload context)
			bool handlerInvoked = false;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				handlerInvoked = true;
			};

			// Step 4: Rebuild the page — should load without throwing.
			// REGRESSION: Currently throws XamlParseException despite handler being set,
			// which on iOS crashes the app via UIKit callback propagation.
			var reloadedPage = new Maui35018(inflator);
			Assert.NotNull(reloadedPage);
			Assert.True(handlerInvoked, "ExceptionHandler2 should be invoked for the missing resource");
		}
#else
		[Fact(Skip = "Hot Reload tests run only in DEBUG")]
		public void InitialLoadWithResourceSucceeds() { }

		[Fact(Skip = "Hot Reload tests run only in DEBUG")]
		public void HotReloadAfterResourceRenameShouldNotCrash() { }
#endif
	}
}
