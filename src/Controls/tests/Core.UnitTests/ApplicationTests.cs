using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;


using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ApplicationTests : BaseTestFixture
    {
        [Fact]
        public void NewApplicationHasNoWindowsNorPage()
        {
            var app = new Application();

            Assert.Null(app.MainPage);
            Assert.Empty(app.Windows);
        }

        [Fact]
        public void SettingMainPageSetsMainPageButNotWindow()
        {
            var app = new Application();
            var page = new ContentPage();

            app.MainPage = page;

            Assert.Equal(page, app.MainPage);
            Assert.Empty(app.Windows);
        }

        [Fact]
        public void CreateWindowUsesMainPage()
        {
            var app = new Application();
            var iapp = app as IApplication;
            var page = new ContentPage();

            app.MainPage = page;

            var window = iapp.CreateWindow(null);

            Assert.Equal(page, app.MainPage);
            Assert.NotEmpty(app.Windows);
            Assert.Single(app.Windows);
            Assert.Equal(window, app.Windows[0]);
            Assert.Equal(page, app.Windows[0].Page);
        }

        [Fact]
        public void SettingMainPageUpdatesWindow()
        {
            var app = new Application();
            var iapp = app as IApplication;
            var page = new ContentPage();

            app.MainPage = page;
            var window = iapp.CreateWindow(null);

            var page2 = new ContentPage();
            app.MainPage = page2;

            Assert.Equal(page2, app.MainPage);
            Assert.NotEmpty(app.Windows);
            Assert.Single(app.Windows);
            Assert.Equal(window, app.Windows[0]);
            Assert.Equal(page2, app.Windows[0].Page);
        }

        [Fact]
        public void NotSettingMainPageThrows()
        {
            var app = new Application();
            var iapp = app as IApplication;

            Assert.Throws<NotImplementedException>(() => iapp.CreateWindow(null));
        }

        [Fact]
        public void SettingMainPageAndOverridingCreateWindowWithSamePageIsValid()
        {
            var page = new ContentPage();
            var window = new Window(page);

            var app = new StubApp() { MainWindow = window, MainPage = page };
            var iapp = app as IApplication;

            var win = iapp.CreateWindow(null);

            Assert.Equal(window, win);
            Assert.Equal(window.Page, page);
            Assert.Equal(app.MainPage, page);
        }

        [Fact]
        public void SettingMainPageAndOverridingCreateWindowThrows()
        {
            var window = new Window(new ContentPage());

            var app = new StubApp() { MainWindow = window, MainPage = new ContentPage() };
            var iapp = app as IApplication;

            Assert.Throws<InvalidOperationException>(() => iapp.CreateWindow(null));
        }

        [Fact]
        public void CanUseExistingWindow()
        {
            var window = new Window();

            var app = new StubApp { MainWindow = window };
            var iapp = app as IApplication;

            var win = iapp.CreateWindow(null);

            Assert.Equal(window, win);
            Assert.Null(app.MainPage);
        }

        [Fact]
        public void CanUseExistingWindowWithPage()
        {
            var window = new Window { Page = new ContentPage() };

            var app = new StubApp { MainWindow = window };
            var iapp = app as IApplication;

            var win = iapp.CreateWindow(null);

            Assert.Equal(window, win);
            Assert.Equal(window.Page, app.MainPage);
        }

        [Fact]
        public void SettingMainPageOverwritesExistingPage()
        {
            var window = new Window { Page = new ContentPage() };

            var app = new StubApp { MainWindow = window };
            var iapp = app as IApplication;

            var win = iapp.CreateWindow(null);

            var page2 = new ContentPage();
            app.MainPage = page2;

            Assert.Equal(window, win);
            Assert.Equal(page2, app.MainPage);
            Assert.Equal(window.Page, app.MainPage);
        }

        [Fact]
        public void SettingWindowPageOverwritesMainPage()
        {
            var window = new Window { Page = new ContentPage() };

            var app = new StubApp { MainWindow = window };
            var iapp = app as IApplication;

            var win = iapp.CreateWindow(null);

            var page2 = new ContentPage();
            window.Page = page2;

            Assert.Equal(window, win);
            Assert.Equal(page2, app.MainPage);
            Assert.Equal(window.Page, app.MainPage);
        }

        [Fact]
        public void OnStartFiresOnceFromWindowCreated()
        {
            var window = new Window { Page = new ContentPage() };
            var app = new StubApp { MainWindow = window };
            var iapp = app as IApplication;
            var win = iapp.CreateWindow(null);

            Assert.Equal(0, app.OnStartCount);
            (window as IWindow).Created();
            Assert.Equal(1, app.OnStartCount);
            Assert.Throws<InvalidOperationException>(() => (window as IWindow).Created());
            Assert.Equal(1, app.OnStartCount);

        }

        [Fact]
        public void FailsOnNoPageOrWindowCreator()
        {
            IApplication app = new Application();
            var ex = Record.Exception(() => app.CreateWindow(null));

            Assert.IsType<NotImplementedException>(ex);

            Assert.Equal($"Either set {nameof(Application.MainPage)} or override {nameof(IApplication.CreateWindow)}.", ex.Message);
        }

        [Fact]
        public void CreatesWindowFromIWindowCreator()
        {
            var app = new Application();

            var window = new Window(new ContentPage
            {
                Content = new Label
                {
                    Text = "Hello World"
                }
            });
            var windowCreator = Substitute.For<IWindowCreator>();
            var services = Substitute.For<IServiceProvider>();
            services.GetService(typeof(IWindowCreator)).Returns(windowCreator);
            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);
            var activationState = Substitute.For<IActivationState>();
            activationState.Context.Returns(mauiContext);

            windowCreator.CreateWindow(app, activationState).Returns(window);
            var iApp = (IApplication)app;
            iApp.CreateWindow(activationState);
            Assert.Single(app.Windows);
            Assert.Same(window, app.Windows[0]);
        }

        [Fact]
        public void FailsOnNoPageAndNoRegistrationForIWindowCreator()
        {
            var app = new Application();

            var services = Substitute.For<IServiceProvider>();
            services.GetService(typeof(IWindowCreator)).Returns(null);
            var mauiContext = Substitute.For<IMauiContext>();
            mauiContext.Services.Returns(services);
            var activationState = Substitute.For<IActivationState>();
            activationState.Context.Returns(mauiContext);

            var iApp = (IApplication)app;
            var ex = Record.Exception(() => iApp.CreateWindow(activationState));
            Assert.IsType<NotImplementedException>(ex);

            Assert.Equal($"Either set {nameof(Application.MainPage)} or override {nameof(IApplication.CreateWindow)}.", ex.Message);
        }

        class StubApp : Application
        {
            public int OnStartCount { get; private set; }
            public StubApp() : base(false)
            {

            }

            public Window MainWindow { get; set; }

            protected override Window CreateWindow(IActivationState activationState)
            {
                return MainWindow;
            }

            protected override void OnStart()
            {
                base.OnStart();
                OnStartCount++;
            }
        }

        /// <summary>
        /// Tests that the base OnSleep method can be called without throwing exceptions.
        /// The base implementation is empty, so it should complete successfully.
        /// </summary>
        [Fact]
        public void OnSleep_BaseImplementation_DoesNotThrow()
        {
            // Arrange
            var app = new TestableApplication();

            // Act & Assert
            var exception = Record.Exception(() => app.CallOnSleep());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that an overridden OnSleep method is called when invoked.
        /// Verifies that the virtual method mechanism works correctly.
        /// </summary>
        [Fact]
        public void OnSleep_OverriddenImplementation_IsCalled()
        {
            // Arrange
            var app = new TestableApplicationWithOnSleep();

            // Act
            app.CallOnSleep();

            // Assert
            Assert.Equal(1, app.OnSleepCallCount);
        }

        /// <summary>
        /// Tests that multiple calls to OnSleep are handled correctly.
        /// Verifies that the method can be called repeatedly without issues.
        /// </summary>
        [Fact]
        public void OnSleep_MultipleCalls_HandledCorrectly()
        {
            // Arrange
            var app = new TestableApplicationWithOnSleep();

            // Act
            app.CallOnSleep();
            app.CallOnSleep();
            app.CallOnSleep();

            // Assert
            Assert.Equal(3, app.OnSleepCallCount);
        }

        /// <summary>
        /// Test application class that exposes the protected OnSleep method for testing.
        /// </summary>
        class TestableApplication : Application
        {
            public TestableApplication() : base(false)
            {
            }

            public void CallOnSleep()
            {
                OnSleep();
            }
        }

        /// <summary>
        /// Test application class that overrides OnSleep and tracks call count.
        /// </summary>
        class TestableApplicationWithOnSleep : Application
        {
            public int OnSleepCallCount { get; private set; }

            public TestableApplicationWithOnSleep() : base(false)
            {
            }

            public void CallOnSleep()
            {
                OnSleep();
            }

            protected override void OnSleep()
            {
                base.OnSleep();
                OnSleepCallCount++;
            }
        }

        /// <summary>
        /// Tests that SetAppIndexingProvider correctly assigns a valid provider to the internal field.
        /// Input: Valid IAppIndexingProvider mock
        /// Expected: The provider is stored in the _appIndexProvider field
        /// </summary>
        [Fact]
        public void SetAppIndexingProvider_ValidProvider_StoresProvider()
        {
            // Arrange
            var application = new Application();
            var provider = Substitute.For<IAppIndexingProvider>();

            // Act
            application.SetAppIndexingProvider(provider);

            // Assert
            var field = typeof(Application).GetField("_appIndexProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field.GetValue(application);
            Assert.Same(provider, fieldValue);
        }

        /// <summary>
        /// Tests that SetAppIndexingProvider correctly assigns null to the internal field.
        /// Input: null provider
        /// Expected: null is stored in the _appIndexProvider field
        /// </summary>
        [Fact]
        public void SetAppIndexingProvider_NullProvider_StoresNull()
        {
            // Arrange
            var application = new Application();

            // Act
            application.SetAppIndexingProvider(null);

            // Assert
            var field = typeof(Application).GetField("_appIndexProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field.GetValue(application);
            Assert.Null(fieldValue);
        }

        /// <summary>
        /// Tests that SetAppIndexingProvider overwrites the previously stored provider.
        /// Input: First provider, then second provider
        /// Expected: The second provider overwrites the first one in the _appIndexProvider field
        /// </summary>
        [Fact]
        public void SetAppIndexingProvider_MultipleProviders_OverwritesPrevious()
        {
            // Arrange
            var application = new Application();
            var firstProvider = Substitute.For<IAppIndexingProvider>();
            var secondProvider = Substitute.For<IAppIndexingProvider>();

            // Act
            application.SetAppIndexingProvider(firstProvider);
            application.SetAppIndexingProvider(secondProvider);

            // Assert
            var field = typeof(Application).GetField("_appIndexProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field.GetValue(application);
            Assert.Same(secondProvider, fieldValue);
            Assert.NotSame(firstProvider, fieldValue);
        }

        /// <summary>
        /// Tests that SetAppIndexingProvider overwrites a null provider with a valid provider.
        /// Input: null provider first, then valid provider
        /// Expected: The valid provider replaces null in the _appIndexProvider field
        /// </summary>
        [Fact]
        public void SetAppIndexingProvider_NullThenValidProvider_ReplacesNull()
        {
            // Arrange
            var application = new Application();
            var provider = Substitute.For<IAppIndexingProvider>();

            // Act
            application.SetAppIndexingProvider(null);
            application.SetAppIndexingProvider(provider);

            // Assert
            var field = typeof(Application).GetField("_appIndexProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field.GetValue(application);
            Assert.Same(provider, fieldValue);
        }

        /// <summary>
        /// Tests that SetAppIndexingProvider overwrites a valid provider with null.
        /// Input: valid provider first, then null
        /// Expected: null replaces the valid provider in the _appIndexProvider field
        /// </summary>
        [Fact]
        public void SetAppIndexingProvider_ValidProviderThenNull_ReplacesWithNull()
        {
            // Arrange
            var application = new Application();
            var provider = Substitute.For<IAppIndexingProvider>();

            // Act
            application.SetAppIndexingProvider(provider);
            application.SetAppIndexingProvider(null);

            // Assert
            var field = typeof(Application).GetField("_appIndexProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldValue = field.GetValue(application);
            Assert.Null(fieldValue);
        }

        /// <summary>
        /// Tests that OnResume method can be called without throwing an exception.
        /// Verifies the base implementation of the virtual method is safe to invoke.
        /// </summary>
        [Fact]
        public void OnResume_BaseImplementation_DoesNotThrow()
        {
            // Arrange
            var testApp = new TestApplicationForOnResume();

            // Act & Assert
            var exception = Record.Exception(() => testApp.CallOnResume());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnResume method can be overridden and the override is called.
        /// Verifies the virtual method behavior works correctly for derived classes.
        /// </summary>
        [Fact]
        public void OnResume_WhenOverridden_CallsOverriddenImplementation()
        {
            // Arrange
            var testApp = new TestApplicationWithOverriddenOnResume();

            // Act
            testApp.CallOnResume();

            // Assert
            Assert.True(testApp.OnResumeWasCalled);
        }

        /// <summary>
        /// Tests that multiple calls to OnResume work correctly.
        /// Verifies the method can be called multiple times without issues.
        /// </summary>
        [Fact]
        public void OnResume_MultipleInvocations_DoesNotThrow()
        {
            // Arrange
            var testApp = new TestApplicationWithOverriddenOnResume();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                testApp.CallOnResume();
                testApp.CallOnResume();
                testApp.CallOnResume();
            });

            Assert.Null(exception);
            Assert.Equal(3, testApp.OnResumeCallCount);
        }

        private class TestApplicationForOnResume : Application
        {
            public TestApplicationForOnResume() : base(false)
            {
            }

            public void CallOnResume()
            {
                OnResume();
            }
        }

        private class TestApplicationWithOverriddenOnResume : Application
        {
            public bool OnResumeWasCalled { get; private set; }
            public int OnResumeCallCount { get; private set; }

            public TestApplicationWithOverriddenOnResume() : base(false)
            {
            }

            public void CallOnResume()
            {
                OnResume();
            }

            protected override void OnResume()
            {
                OnResumeWasCalled = true;
                OnResumeCallCount++;
                base.OnResume();
            }
        }

        /// <summary>
        /// Tests that SendOnAppLinkRequestReceived calls OnAppLinkRequestReceived with a valid Uri.
        /// This verifies the method correctly delegates to the protected virtual method.
        /// </summary>
        [Fact]
        public void SendOnAppLinkRequestReceived_ValidUri_CallsOnAppLinkRequestReceived()
        {
            // Arrange
            var testApp = new TestApplicationForAppLink(false);
            var testUri = new Uri("https://example.com/test");

            // Act
            testApp.SendOnAppLinkRequestReceived(testUri);

            // Assert
            Assert.True(testApp.OnAppLinkRequestReceivedCalled);
            Assert.Same(testUri, testApp.ReceivedUri);
        }

        /// <summary>
        /// Tests that SendOnAppLinkRequestReceived handles null Uri parameter.
        /// This verifies the method passes null through to the protected virtual method.
        /// </summary>
        [Fact]
        public void SendOnAppLinkRequestReceived_NullUri_CallsOnAppLinkRequestReceivedWithNull()
        {
            // Arrange
            var testApp = new TestApplicationForAppLink(false);
            Uri nullUri = null;

            // Act
            testApp.SendOnAppLinkRequestReceived(nullUri);

            // Assert
            Assert.True(testApp.OnAppLinkRequestReceivedCalled);
            Assert.Null(testApp.ReceivedUri);
        }

        /// <summary>
        /// Tests that SendOnAppLinkRequestReceived works with various Uri types and schemes.
        /// This verifies the method correctly handles different valid Uri formats.
        /// </summary>
        /// <param name="uriString">The Uri string to test with.</param>
        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://localhost:8080/path")]
        [InlineData("custom://scheme/path")]
        [InlineData("file:///C:/path/to/file")]
        [InlineData("ftp://ftp.example.com/file")]
        [InlineData("mailto:test@example.com")]
        public void SendOnAppLinkRequestReceived_VariousUriTypes_CallsOnAppLinkRequestReceived(string uriString)
        {
            // Arrange
            var testApp = new TestApplicationForAppLink(false);
            var testUri = new Uri(uriString);

            // Act
            testApp.SendOnAppLinkRequestReceived(testUri);

            // Assert
            Assert.True(testApp.OnAppLinkRequestReceivedCalled);
            Assert.Same(testUri, testApp.ReceivedUri);
            Assert.Equal(uriString, testApp.ReceivedUri.ToString());
        }

        /// <summary>
        /// Tests that SendOnAppLinkRequestReceived handles very long Uri.
        /// This verifies the method works with boundary case of extremely long Uris.
        /// </summary>
        [Fact]
        public void SendOnAppLinkRequestReceived_VeryLongUri_CallsOnAppLinkRequestReceived()
        {
            // Arrange
            var testApp = new TestApplicationForAppLink(false);
            var longPath = new string('a', 2000); // Very long path
            var longUri = new Uri($"https://example.com/{longPath}");

            // Act
            testApp.SendOnAppLinkRequestReceived(longUri);

            // Assert
            Assert.True(testApp.OnAppLinkRequestReceivedCalled);
            Assert.Same(longUri, testApp.ReceivedUri);
        }

        /// <summary>
        /// Tests that SendOnAppLinkRequestReceived handles relative Uri.
        /// This verifies the method works with relative Uri objects.
        /// </summary>
        [Fact]
        public void SendOnAppLinkRequestReceived_RelativeUri_CallsOnAppLinkRequestReceived()
        {
            // Arrange
            var testApp = new TestApplicationForAppLink(false);
            var relativeUri = new Uri("/relative/path", UriKind.Relative);

            // Act
            testApp.SendOnAppLinkRequestReceived(relativeUri);

            // Assert
            Assert.True(testApp.OnAppLinkRequestReceivedCalled);
            Assert.Same(relativeUri, testApp.ReceivedUri);
        }

        /// <summary>
        /// Test helper class that inherits from Application to track calls to OnAppLinkRequestReceived.
        /// </summary>
        private class TestApplicationForAppLink : Application
        {
            public bool OnAppLinkRequestReceivedCalled { get; private set; }
            public Uri ReceivedUri { get; private set; }

            public TestApplicationForAppLink(bool setCurrentApplication) : base(setCurrentApplication)
            {
            }

            protected override void OnAppLinkRequestReceived(Uri uri)
            {
                OnAppLinkRequestReceivedCalled = true;
                ReceivedUri = uri;
                base.OnAppLinkRequestReceived(uri);
            }
        }

        /// <summary>
        /// Tests that OnParentSet throws InvalidOperationException when called.
        /// This verifies that setting a parent on an Application is prevented as designed.
        /// </summary>
        [Fact]
        public void OnParentSet_WhenCalled_ThrowsInvalidOperationException()
        {
            // Arrange
            var application = new TestableApplication();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => application.CallOnParentSet());
            Assert.Equal("Setting a Parent on Application is invalid.", exception.Message);
        }

    }

    public partial class ApplicationAppLinksTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that accessing AppLinks property when no IAppIndexingProvider is set throws ArgumentException.
        /// Input: Application with null _appIndexProvider field.
        /// Expected: ArgumentException with message "No IAppIndexingProvider was provided".
        /// </summary>
        [Fact]
        public void AppLinks_WhenAppIndexingProviderIsNull_ThrowsArgumentException()
        {
            // Arrange
            var application = new Application();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => application.AppLinks);
            Assert.Equal("No IAppIndexingProvider was provided", exception.Message);
        }

        /// <summary>
        /// Tests that accessing AppLinks property when IAppIndexingProvider.AppLinks returns null throws ArgumentException.
        /// Input: Application with IAppIndexingProvider that has null AppLinks property.
        /// Expected: ArgumentException with message about missing AppLinks implementation.
        /// </summary>
        [Fact]
        public void AppLinks_WhenAppIndexingProviderAppLinksIsNull_ThrowsArgumentException()
        {
            // Arrange
            var application = new Application();
            var mockProvider = Substitute.For<IAppIndexingProvider>();
            mockProvider.AppLinks.Returns((IAppLinks)null);
            application.SetAppIndexingProvider(mockProvider);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => application.AppLinks);
            Assert.Equal("No AppLinks implementation was found, if in Android make sure you installed the Microsoft.Maui.Controls.AppLinks", exception.Message);
        }

        /// <summary>
        /// Tests that accessing AppLinks property when both IAppIndexingProvider and AppLinks are valid returns the AppLinks instance.
        /// Input: Application with valid IAppIndexingProvider containing valid IAppLinks.
        /// Expected: Returns the IAppLinks instance from the provider.
        /// </summary>
        [Fact]
        public void AppLinks_WhenAppIndexingProviderAndAppLinksAreValid_ReturnsAppLinks()
        {
            // Arrange
            var application = new Application();
            var mockAppLinks = Substitute.For<IAppLinks>();
            var mockProvider = Substitute.For<IAppIndexingProvider>();
            mockProvider.AppLinks.Returns(mockAppLinks);
            application.SetAppIndexingProvider(mockProvider);

            // Act
            var result = application.AppLinks;

            // Assert
            Assert.Same(mockAppLinks, result);
        }
    }


    /// <summary>
    /// Unit tests for the OnAppLinkRequestReceived method in the Application class.
    /// </summary>
    public partial class ApplicationOnAppLinkRequestReceivedTests : BaseTestFixture
    {
        /// <summary>
        /// Test application class that exposes the protected OnAppLinkRequestReceived method for testing.
        /// </summary>
        private class TestApplication : Application
        {
            public bool OnAppLinkRequestReceivedCalled { get; private set; }
            public Uri ReceivedUri { get; private set; }

            public TestApplication() : base(false)
            {
            }

            public void CallOnAppLinkRequestReceived(Uri uri)
            {
                OnAppLinkRequestReceived(uri);
            }

            protected override void OnAppLinkRequestReceived(Uri uri)
            {
                OnAppLinkRequestReceivedCalled = true;
                ReceivedUri = uri;
                base.OnAppLinkRequestReceived(uri);
            }
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a valid HTTP URI.
        /// Verifies that the method can be called without throwing exceptions and covers the basic execution path.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_ValidHttpUri_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("https://example.com/path");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a valid HTTPS URI.
        /// Verifies proper handling of secure HTTP URIs.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_ValidHttpsUri_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("https://secure.example.com/secure-path?param=value");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a custom URI scheme.
        /// Verifies proper handling of app-specific URI schemes commonly used for deep linking.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_CustomSchemeUri_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("myapp://open/page?id=123");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a file URI.
        /// Verifies proper handling of local file URIs.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_FileUri_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("file:///path/to/file.txt");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a URI containing special characters.
        /// Verifies proper handling of URIs with encoded special characters and Unicode.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_UriWithSpecialCharacters_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("https://example.com/path?query=hello%20world&param=test%26value");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a very long URI.
        /// Verifies proper handling of URIs at boundary conditions for length.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_VeryLongUri_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var longPath = new string('a', 1000);
            var uri = new Uri($"https://example.com/{longPath}");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with a URI containing fragments.
        /// Verifies proper handling of URIs with hash fragments.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_UriWithFragment_ExecutesWithoutException()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("https://example.com/page#section");

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived throws ArgumentNullException when passed a null URI.
        /// Verifies proper null parameter validation behavior.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_NullUri_ThrowsArgumentNullException()
        {
            // Arrange
            var application = new TestApplication();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => application.CallOnAppLinkRequestReceived(null));
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived can be successfully overridden in derived classes.
        /// Verifies that the virtual method pattern works correctly and derived implementations are called.
        /// </summary>
        [Fact]
        public void OnAppLinkRequestReceived_OverriddenInDerivedClass_CallsDerivedImplementation()
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri("https://example.com/test");

            // Act
            application.CallOnAppLinkRequestReceived(uri);

            // Assert
            Assert.True(application.OnAppLinkRequestReceivedCalled);
            Assert.Equal(uri, application.ReceivedUri);
        }

        /// <summary>
        /// Tests that OnAppLinkRequestReceived executes successfully with various URI schemes.
        /// Verifies proper handling of different standard and custom URI schemes using parameterized testing.
        /// </summary>
        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com")]
        [InlineData("ftp://example.com/file")]
        [InlineData("mailto:test@example.com")]
        [InlineData("tel:+1234567890")]
        [InlineData("sms:1234567890")]
        [InlineData("myapp://action")]
        [InlineData("content://provider/path")]
        public void OnAppLinkRequestReceived_VariousSchemes_ExecutesWithoutException(string uriString)
        {
            // Arrange
            var application = new TestApplication();
            var uri = new Uri(uriString);

            // Act & Assert
            var exception = Record.Exception(() => application.CallOnAppLinkRequestReceived(uri));

            Assert.Null(exception);
        }
    }


    public partial class ApplicationActivateWindowTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that ActivateWindow does not throw when Handler is null.
        /// Input: null Handler, valid Window.
        /// Expected: No exception thrown, no method invocation.
        /// </summary>
        [Fact]
        public void ActivateWindow_HandlerIsNull_DoesNotThrow()
        {
            // Arrange
            var application = new Application();
            var window = new Window { Page = new ContentPage() };

            // Ensure Handler is null
            Assert.Null(application.Handler);

            // Act & Assert
            var exception = Record.Exception(() => application.ActivateWindow(window));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ActivateWindow calls Handler.Invoke with correct parameters when Handler is not null.
        /// Input: valid Handler, valid Window.
        /// Expected: Handler.Invoke called with "ActivateWindow" command and window parameter.
        /// </summary>
        [Fact]
        public void ActivateWindow_HandlerNotNull_CallsInvokeWithCorrectParameters()
        {
            // Arrange
            var application = new Application();
            var window = new Window { Page = new ContentPage() };
            var mockHandler = Substitute.For<IElementHandler>();

            // Set the handler
            application.Handler = mockHandler;

            // Act
            application.ActivateWindow(window);

            // Assert
            mockHandler.Received(1).Invoke("ActivateWindow", window);
        }

        /// <summary>
        /// Tests that ActivateWindow handles null window parameter correctly when Handler is null.
        /// Input: null Handler, null Window.
        /// Expected: No exception thrown, no method invocation.
        /// </summary>
        [Fact]
        public void ActivateWindow_HandlerIsNullAndWindowIsNull_DoesNotThrow()
        {
            // Arrange
            var application = new Application();

            // Ensure Handler is null
            Assert.Null(application.Handler);

            // Act & Assert
            var exception = Record.Exception(() => application.ActivateWindow(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ActivateWindow handles null window parameter correctly when Handler is not null.
        /// Input: valid Handler, null Window.
        /// Expected: Handler.Invoke called with "ActivateWindow" command and null window parameter.
        /// </summary>
        [Fact]
        public void ActivateWindow_HandlerNotNullAndWindowIsNull_CallsInvokeWithNullWindow()
        {
            // Arrange
            var application = new Application();
            var mockHandler = Substitute.For<IElementHandler>();

            // Set the handler
            application.Handler = mockHandler;

            // Act
            application.ActivateWindow(null);

            // Assert
            mockHandler.Received(1).Invoke("ActivateWindow", null);
        }

        /// <summary>
        /// Tests that ActivateWindow uses the correct command name from nameof.
        /// Input: valid Handler, valid Window.
        /// Expected: Handler.Invoke called with exact "ActivateWindow" string.
        /// </summary>
        [Fact]
        public void ActivateWindow_UsesCorrectCommandName()
        {
            // Arrange
            var application = new Application();
            var window = new Window { Page = new ContentPage() };
            var mockHandler = Substitute.For<IElementHandler>();

            // Set the handler
            application.Handler = mockHandler;

            // Act
            application.ActivateWindow(window);

            // Assert
            mockHandler.Received(1).Invoke(Arg.Is<string>(s => s == "ActivateWindow"), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that ActivateWindow can be called multiple times with same window.
        /// Input: valid Handler, same Window called multiple times.
        /// Expected: Handler.Invoke called each time with correct parameters.
        /// </summary>
        [Fact]
        public void ActivateWindow_CalledMultipleTimes_InvokesHandlerEachTime()
        {
            // Arrange
            var application = new Application();
            var window = new Window { Page = new ContentPage() };
            var mockHandler = Substitute.For<IElementHandler>();

            // Set the handler
            application.Handler = mockHandler;

            // Act
            application.ActivateWindow(window);
            application.ActivateWindow(window);

            // Assert
            mockHandler.Received(2).Invoke("ActivateWindow", window);
        }

        /// <summary>
        /// Tests that ActivateWindow works with different window instances.
        /// Input: valid Handler, different Window instances.
        /// Expected: Handler.Invoke called with each window instance.
        /// </summary>
        [Fact]
        public void ActivateWindow_DifferentWindows_CallsInvokeWithEachWindow()
        {
            // Arrange
            var application = new Application();
            var window1 = new Window { Page = new ContentPage() };
            var window2 = new Window { Page = new ContentPage() };
            var mockHandler = Substitute.For<IElementHandler>();

            // Set the handler
            application.Handler = mockHandler;

            // Act
            application.ActivateWindow(window1);
            application.ActivateWindow(window2);

            // Assert
            mockHandler.Received(1).Invoke("ActivateWindow", window1);
            mockHandler.Received(1).Invoke("ActivateWindow", window2);
        }
    }


    public partial class ApplicationMainPageTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that setting MainPage to the same value returns early without triggering property change operations.
        /// This test covers the early return condition on line 97 that was not covered by existing tests.
        /// </summary>
        [Fact]
        public void MainPage_SetSameValue_ReturnsEarly()
        {
            // Arrange
            var app = new TestableApplication();
            var page = new ContentPage();

            // Set initial value
            app.MainPage = page;

            // Reset counters to track if property change methods are called again
            app.ResetCounters();

            // Act - set the same value again
            app.MainPage = page;

            // Assert - verify no property change operations occurred
            Assert.Equal(0, app.OnPropertyChangingCallCount);
            Assert.Equal(0, app.OnPropertyChangedCallCount);
            Assert.Equal(page, app.MainPage);
        }

        /// <summary>
        /// Tests that setting MainPage to the same null value returns early.
        /// This covers the edge case where both current and new values are null.
        /// </summary>
        [Fact]
        public void MainPage_SetSameNullValue_ReturnsEarly()
        {
            // Arrange
            var app = new TestableApplication();

            // Ensure MainPage is null initially
            Assert.Null(app.MainPage);

            // Act - set to null again
            app.MainPage = null;

            // Assert - verify no property change operations occurred
            Assert.Equal(0, app.OnPropertyChangingCallCount);
            Assert.Equal(0, app.OnPropertyChangedCallCount);
            Assert.Null(app.MainPage);
        }

        /// <summary>
        /// Tests the MainPage getter when no windows exist.
        /// Should return the internally stored _singleWindowMainPage value.
        /// </summary>
        [Fact]
        public void MainPage_GetWithNoWindows_ReturnsStoredValue()
        {
            // Arrange
            var app = new Application();
            var page = new ContentPage();

            // Act - set MainPage when no windows exist
            app.MainPage = page;

            // Assert
            Assert.Empty(app.Windows);
            Assert.Equal(page, app.MainPage);
        }

        /// <summary>
        /// Tests the MainPage getter when windows exist.
        /// Should return the Page property of the first window.
        /// </summary>
        [Fact]
        public void MainPage_GetWithWindows_ReturnsFirstWindowPage()
        {
            // Arrange
            var app = new Application();
            var initialPage = new ContentPage();
            var windowPage = new ContentPage();

            // Set initial MainPage
            app.MainPage = initialPage;

            // Create window and set its page directly
            var iapp = app as IApplication;
            var window = iapp.CreateWindow(null);
            window.Page = windowPage;

            // Act & Assert
            Assert.NotEmpty(app.Windows);
            Assert.Equal(windowPage, app.MainPage);
        }

        /// <summary>
        /// Tests setting MainPage to null when it previously had a value.
        /// Should handle resource listener removal and property notifications correctly.
        /// </summary>
        [Fact]
        public void MainPage_SetToNull_UpdatesCorrectly()
        {
            // Arrange
            var app = new TestableApplication();
            var page = new ContentPage();

            // Set initial non-null value
            app.MainPage = page;
            app.ResetCounters();

            // Act - set to null
            app.MainPage = null;

            // Assert
            Assert.Null(app.MainPage);
            Assert.Equal(1, app.OnPropertyChangingCallCount);
            Assert.Equal(1, app.OnPropertyChangedCallCount);
        }

        /// <summary>
        /// Tests setting MainPage when exactly one window exists.
        /// Should update the window's Page property to match the new MainPage.
        /// </summary>
        [Fact]
        public void MainPage_SetWithSingleWindow_UpdatesWindowPage()
        {
            // Arrange
            var app = new Application();
            var initialPage = new ContentPage();
            var newPage = new ContentPage();

            // Set initial MainPage and create window
            app.MainPage = initialPage;
            var iapp = app as IApplication;
            var window = iapp.CreateWindow(null);

            // Verify setup
            Assert.Single(app.Windows);
            Assert.Equal(initialPage, window.Page);

            // Act - set new MainPage
            app.MainPage = newPage;

            // Assert - window's page should be updated
            Assert.Equal(newPage, app.MainPage);
            Assert.Equal(newPage, window.Page);
        }

        /// <summary>
        /// Tests setting MainPage when multiple windows exist.
        /// Should only update the first window's page.
        /// </summary>
        [Fact]
        public void MainPage_SetWithMultipleWindows_UpdatesOnlyFirstWindow()
        {
            // Arrange
            var app = new Application();
            var initialPage = new ContentPage();
            var newPage = new ContentPage();
            var secondWindowPage = new ContentPage();

            // Set initial MainPage and create first window
            app.MainPage = initialPage;
            var iapp = app as IApplication;
            var window1 = iapp.CreateWindow(null);

            // Create second window with different page
            var window2 = iapp.CreateWindow(null);
            window2.Page = secondWindowPage;

            // Verify setup
            Assert.Equal(2, app.Windows.Count);

            // Act - set new MainPage
            app.MainPage = newPage;

            // Assert - only first window should be updated
            Assert.Equal(newPage, app.MainPage);
            Assert.Equal(newPage, app.Windows[0].Page);
            Assert.Equal(secondWindowPage, app.Windows[1].Page); // Second window unchanged
        }

        /// <summary>
        /// Tests MainPage property change notifications are properly triggered.
        /// Verifies OnPropertyChanging is called before the change and OnPropertyChanged after.
        /// </summary>
        [Theory]
        [InlineData(null, "TestPage")]
        [InlineData("InitialPage", null)]
        [InlineData("InitialPage", "NewPage")]
        public void MainPage_PropertyChangeNotifications_TriggeredCorrectly(string initialPageTitle, string newPageTitle)
        {
            // Arrange
            var app = new TestableApplication();
            var initialPage = initialPageTitle != null ? new ContentPage { Title = initialPageTitle } : null;
            var newPage = newPageTitle != null ? new ContentPage { Title = newPageTitle } : null;

            if (initialPage != null)
            {
                app.MainPage = initialPage;
                app.ResetCounters();
            }

            // Act
            app.MainPage = newPage;

            // Assert
            Assert.Equal(1, app.OnPropertyChangingCallCount);
            Assert.Equal(1, app.OnPropertyChangedCallCount);
            Assert.Equal(newPage, app.MainPage);
        }

        /// <summary>
        /// Helper class to expose protected methods for testing property change notifications.
        /// </summary>
        private class TestableApplication : Application
        {
            public int OnPropertyChangingCallCount { get; private set; }
            public int OnPropertyChangedCallCount { get; private set; }

            public TestableApplication() : base(false) { }

            protected override void OnPropertyChanging(string propertyName = null)
            {
                OnPropertyChangingCallCount++;
                base.OnPropertyChanging(propertyName);
            }

            protected override void OnPropertyChanged(string propertyName = null)
            {
                OnPropertyChangedCallCount++;
                base.OnPropertyChanged(propertyName);
            }

            public void ResetCounters()
            {
                OnPropertyChangingCallCount = 0;
                OnPropertyChangedCallCount = 0;
            }
        }
    }


    public partial class ApplicationSendStartTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that SendStart calls OnStart on the first invocation.
        /// Verifies that the application start process is properly initiated.
        /// </summary>
        [Fact]
        public void SendStart_FirstCall_CallsOnStart()
        {
            // Arrange
            var app = new TestApplication();

            // Act
            app.SendStart();

            // Assert
            Assert.Equal(1, app.OnStartCount);
        }

        /// <summary>
        /// Tests that SendStart does not call OnStart on subsequent invocations.
        /// Verifies that the early return mechanism works when _isStarted is true.
        /// </summary>
        [Fact]
        public void SendStart_SubsequentCall_DoesNotCallOnStart()
        {
            // Arrange
            var app = new TestApplication();
            app.SendStart(); // First call

            // Act
            app.SendStart(); // Second call

            // Assert
            Assert.Equal(1, app.OnStartCount);
        }

        /// <summary>
        /// Tests that multiple calls to SendStart after the first call do not trigger OnStart.
        /// Verifies consistent behavior across multiple invocations.
        /// </summary>
        [Fact]
        public void SendStart_MultipleCalls_OnlyCallsOnStartOnce()
        {
            // Arrange
            var app = new TestApplication();

            // Act
            app.SendStart(); // First call
            app.SendStart(); // Second call
            app.SendStart(); // Third call
            app.SendStart(); // Fourth call

            // Assert
            Assert.Equal(1, app.OnStartCount);
        }

        /// <summary>
        /// Tests that SendStart works correctly when called concurrently.
        /// Verifies thread safety and that OnStart is only called once.
        /// </summary>
        [Fact]
        public void SendStart_ConcurrentCalls_OnlyCallsOnStartOnce()
        {
            // Arrange
            var app = new TestApplication();
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => app.SendStart());
            }
            Task.WaitAll(tasks);

            // Assert
            Assert.Equal(1, app.OnStartCount);
        }

        class TestApplication : Application
        {
            public int OnStartCount { get; private set; }

            public TestApplication() : base(false)
            {
            }

            protected override void OnStart()
            {
                base.OnStart();
                OnStartCount++;
            }
        }
    }


    /// <summary>
    /// Tests for the Application.Resources property
    /// </summary>
    public partial class ApplicationResourcesTests
    {
        /// <summary>
        /// Tests that getting Resources property creates and returns a new ResourceDictionary when null
        /// </summary>
        [Fact]
        public void Resources_Get_WhenNull_CreatesNewResourceDictionary()
        {
            // Arrange
            var application = new TestableApplication();

            // Act
            var resources = application.Resources;

            // Assert
            Assert.NotNull(resources);
            Assert.IsType<ResourceDictionary>(resources);
        }

        /// <summary>
        /// Tests that getting Resources property returns the same instance on subsequent calls
        /// </summary>
        [Fact]
        public void Resources_Get_WhenAlreadyExists_ReturnsSameInstance()
        {
            // Arrange
            var application = new TestableApplication();
            var firstAccess = application.Resources;

            // Act
            var secondAccess = application.Resources;

            // Assert
            Assert.Same(firstAccess, secondAccess);
        }

        /// <summary>
        /// Tests that setting Resources property to a new ResourceDictionary works correctly
        /// </summary>
        [Fact]
        public void Resources_Set_WithNewResourceDictionary_SetsValue()
        {
            // Arrange
            var application = new TestableApplication();
            var newResources = new ResourceDictionary();

            // Act
            application.Resources = newResources;

            // Assert
            Assert.Same(newResources, application.Resources);
        }

        /// <summary>
        /// Tests that setting Resources property to null works correctly
        /// </summary>
        [Fact]
        public void Resources_Set_WithNull_SetsValueToNull()
        {
            // Arrange
            var application = new TestableApplication();
            var initialResources = application.Resources; // Force creation first

            // Act
            application.Resources = null;

            // Assert
            Assert.Null(application.Resources);
        }

        /// <summary>
        /// Tests that setting Resources property to the same value early returns without triggering property change events
        /// This test specifically targets the uncovered line 119: return;
        /// </summary>
        [Fact]
        public void Resources_Set_WithSameValue_EarlyReturnsWithoutPropertyChange()
        {
            // Arrange
            var application = new TestableApplication();
            var resources = new ResourceDictionary();
            application.Resources = resources;

            // Reset counters after initial set
            application.ResetEventCounts();

            // Act
            application.Resources = resources; // Setting to same value

            // Assert
            Assert.Same(resources, application.Resources);
            Assert.Equal(0, application.PropertyChangingCallCount);
            Assert.Equal(0, application.PropertyChangedCallCount);
            Assert.Equal(0, application.ResourcesChangedCallCount);
        }

        /// <summary>
        /// Tests that setting Resources property to the same null value early returns without triggering property change events
        /// </summary>
        [Fact]
        public void Resources_Set_WithSameNullValue_EarlyReturnsWithoutPropertyChange()
        {
            // Arrange
            var application = new TestableApplication();
            application.Resources = null;

            // Reset counters after initial set
            application.ResetEventCounts();

            // Act
            application.Resources = null; // Setting to same null value

            // Assert
            Assert.Null(application.Resources);
            Assert.Equal(0, application.PropertyChangingCallCount);
            Assert.Equal(0, application.PropertyChangedCallCount);
            Assert.Equal(0, application.ResourcesChangedCallCount);
        }

        /// <summary>
        /// Tests that setting Resources property to a different value triggers all property change events
        /// </summary>
        [Fact]
        public void Resources_Set_WithDifferentValue_TriggersPropertyChangeEvents()
        {
            // Arrange
            var application = new TestableApplication();
            var initialResources = new ResourceDictionary();
            var newResources = new ResourceDictionary();
            application.Resources = initialResources;

            // Reset counters after initial set
            application.ResetEventCounts();

            // Act
            application.Resources = newResources;

            // Assert
            Assert.Same(newResources, application.Resources);
            Assert.Equal(1, application.PropertyChangingCallCount);
            Assert.Equal(1, application.PropertyChangedCallCount);
            Assert.Equal(1, application.ResourcesChangedCallCount);
        }

        /// <summary>
        /// Tests that setting Resources from null to a ResourceDictionary triggers property change events
        /// </summary>
        [Fact]
        public void Resources_Set_FromNullToResourceDictionary_TriggersPropertyChangeEvents()
        {
            // Arrange
            var application = new TestableApplication();
            var newResources = new ResourceDictionary();
            // Start with null resources (default state)

            // Act
            application.Resources = newResources;

            // Assert
            Assert.Same(newResources, application.Resources);
            Assert.Equal(1, application.PropertyChangingCallCount);
            Assert.Equal(1, application.PropertyChangedCallCount);
            Assert.Equal(1, application.ResourcesChangedCallCount);
        }

        /// <summary>
        /// Tests that setting Resources from a ResourceDictionary to null triggers property change events
        /// </summary>
        [Fact]
        public void Resources_Set_FromResourceDictionaryToNull_TriggersPropertyChangeEvents()
        {
            // Arrange
            var application = new TestableApplication();
            var initialResources = new ResourceDictionary();
            application.Resources = initialResources;

            // Reset counters after initial set
            application.ResetEventCounts();

            // Act
            application.Resources = null;

            // Assert
            Assert.Null(application.Resources);
            Assert.Equal(1, application.PropertyChangingCallCount);
            Assert.Equal(1, application.PropertyChangedCallCount);
            Assert.Equal(1, application.ResourcesChangedCallCount);
        }

        /// <summary>
        /// Testable Application class that exposes protected members and tracks method calls
        /// </summary>
        private class TestableApplication : Application
        {
            public int PropertyChangingCallCount { get; private set; }
            public int PropertyChangedCallCount { get; private set; }
            public int ResourcesChangedCallCount { get; private set; }

            public TestableApplication() : base(false)
            {
            }

            protected override void OnPropertyChanging(string propertyName = null)
            {
                PropertyChangingCallCount++;
                base.OnPropertyChanging(propertyName);
            }

            protected override void OnPropertyChanged(string propertyName = null)
            {
                PropertyChangedCallCount++;
                base.OnPropertyChanged(propertyName);
            }

            internal override void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
            {
                ResourcesChangedCallCount++;
                base.OnResourcesChanged(sender, e);
            }

            public void ResetEventCounts()
            {
                PropertyChangingCallCount = 0;
                PropertyChangedCallCount = 0;
                ResourcesChangedCallCount = 0;
            }
        }
    }


    public partial class ApplicationCloseWindowTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CloseWindow invokes Handler.Invoke with correct parameters when Handler is not null.
        /// Input: Valid Window instance with non-null Handler.
        /// Expected: Handler.Invoke is called with "CloseWindow" command and window parameter.
        /// </summary>
        [Fact]
        public void CloseWindow_WithValidHandlerAndWindow_InvokesHandlerWithCorrectParameters()
        {
            // Arrange
            var application = new Application();
            var mockHandler = Substitute.For<IElementHandler>();
            var window = new Window { Page = new ContentPage() };
            application.Handler = mockHandler;

            // Act
            application.CloseWindow(window);

            // Assert
            mockHandler.Received(1).Invoke("CloseWindow", window);
        }

        /// <summary>
        /// Tests that CloseWindow does not throw when Handler is null.
        /// Input: Valid Window instance with null Handler.
        /// Expected: No exception is thrown and no Handler.Invoke call occurs.
        /// </summary>
        [Fact]
        public void CloseWindow_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            var application = new Application();
            var window = new Window { Page = new ContentPage() };
            application.Handler = null;

            // Act & Assert
            var exception = Record.Exception(() => application.CloseWindow(window));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CloseWindow passes null window parameter to Handler.Invoke when window is null.
        /// Input: Null window parameter with non-null Handler.
        /// Expected: Handler.Invoke is called with "CloseWindow" command and null window parameter.
        /// </summary>
        [Fact]
        public void CloseWindow_WithNullWindow_PassesNullToHandler()
        {
            // Arrange
            var application = new Application();
            var mockHandler = Substitute.For<IElementHandler>();
            application.Handler = mockHandler;

            // Act
            application.CloseWindow(null);

            // Assert
            mockHandler.Received(1).Invoke("CloseWindow", null);
        }

        /// <summary>
        /// Tests that CloseWindow does not throw when both Handler and window are null.
        /// Input: Null window parameter with null Handler.
        /// Expected: No exception is thrown and no Handler.Invoke call occurs.
        /// </summary>
        [Fact]
        public void CloseWindow_WithNullHandlerAndNullWindow_DoesNotThrow()
        {
            // Arrange
            var application = new Application();
            application.Handler = null;

            // Act & Assert
            var exception = Record.Exception(() => application.CloseWindow(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CloseWindow can be called multiple times without issues.
        /// Input: Valid Window instance with non-null Handler, called multiple times.
        /// Expected: Handler.Invoke is called the correct number of times with proper parameters.
        /// </summary>
        [Fact]
        public void CloseWindow_CalledMultipleTimes_InvokesHandlerEachTime()
        {
            // Arrange
            var application = new Application();
            var mockHandler = Substitute.For<IElementHandler>();
            var window1 = new Window { Page = new ContentPage() };
            var window2 = new Window { Page = new ContentPage() };
            application.Handler = mockHandler;

            // Act
            application.CloseWindow(window1);
            application.CloseWindow(window2);
            application.CloseWindow(window1);

            // Assert
            mockHandler.Received(3).Invoke("CloseWindow", Arg.Any<Window>());
            mockHandler.Received(2).Invoke("CloseWindow", window1);
            mockHandler.Received(1).Invoke("CloseWindow", window2);
        }

        /// <summary>
        /// Tests that CloseWindow uses the exact string "CloseWindow" as command parameter.
        /// Input: Valid Window instance with non-null Handler.
        /// Expected: Handler.Invoke is called with exact string "CloseWindow" (not case-sensitive variations).
        /// </summary>
        [Fact]
        public void CloseWindow_UsesCorrectCommandString()
        {
            // Arrange
            var application = new Application();
            var mockHandler = Substitute.For<IElementHandler>();
            var window = new Window { Page = new ContentPage() };
            application.Handler = mockHandler;

            // Act
            application.CloseWindow(window);

            // Assert
            mockHandler.Received(1).Invoke(Arg.Is<string>(s => s == "CloseWindow"), Arg.Any<object>());
            mockHandler.DidNotReceive().Invoke(Arg.Is<string>(s => s != "CloseWindow"), Arg.Any<object>());
        }
    }


    public partial class ApplicationUserAppThemeTests
    {
        /// <summary>
        /// Tests that UserAppTheme getter returns the current theme value.
        /// </summary>
        [Theory]
        [InlineData(AppTheme.Unspecified)]
        [InlineData(AppTheme.Light)]
        [InlineData(AppTheme.Dark)]
        public void UserAppTheme_Get_ReturnsCurrentValue(AppTheme expectedTheme)
        {
            // Arrange
            var app = new TestableApplication();
            app.SetUserAppThemeDirectly(expectedTheme);

            // Act
            var actualTheme = app.UserAppTheme;

            // Assert
            Assert.Equal(expectedTheme, actualTheme);
        }

        /// <summary>
        /// Tests that UserAppTheme setter updates the value and triggers change notifications when value is different.
        /// </summary>
        [Theory]
        [InlineData(AppTheme.Unspecified, AppTheme.Light)]
        [InlineData(AppTheme.Light, AppTheme.Dark)]
        [InlineData(AppTheme.Dark, AppTheme.Unspecified)]
        [InlineData(AppTheme.Unspecified, AppTheme.Dark)]
        [InlineData(AppTheme.Light, AppTheme.Unspecified)]
        [InlineData(AppTheme.Dark, AppTheme.Light)]
        public void UserAppTheme_SetDifferentValue_UpdatesValueAndTriggersChangeNotifications(AppTheme initialTheme, AppTheme newTheme)
        {
            // Arrange
            var app = new TestableApplication();
            app.SetUserAppThemeDirectly(initialTheme);
            var propertyChangedEventFired = false;

            app.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Application.UserAppTheme))
                    propertyChangedEventFired = true;
            };

            // Act
            app.UserAppTheme = newTheme;

            // Assert
            Assert.Equal(newTheme, app.UserAppTheme);
            Assert.True(propertyChangedEventFired, "PropertyChanged event should be fired when UserAppTheme value changes");
            Assert.Equal(1, app.TriggerThemeChangedActualCallCount);
        }

        /// <summary>
        /// Tests that UserAppTheme setter returns early without triggering notifications when value is the same.
        /// This test specifically covers the uncovered early return path (line 117).
        /// </summary>
        [Theory]
        [InlineData(AppTheme.Unspecified)]
        [InlineData(AppTheme.Light)]
        [InlineData(AppTheme.Dark)]
        public void UserAppTheme_SetSameValue_ReturnsEarlyWithoutTriggeringChangeNotifications(AppTheme theme)
        {
            // Arrange
            var app = new TestableApplication();
            app.UserAppTheme = theme; // Set initial value
            app.ResetCallCount(); // Reset call count after initial set

            var propertyChangedEventFired = false;
            app.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Application.UserAppTheme))
                    propertyChangedEventFired = true;
            };

            // Act
            app.UserAppTheme = theme; // Set to same value

            // Assert
            Assert.Equal(theme, app.UserAppTheme);
            Assert.False(propertyChangedEventFired, "PropertyChanged event should not be fired when UserAppTheme value is the same");
            Assert.Equal(0, app.TriggerThemeChangedActualCallCount);
        }

        /// <summary>
        /// Tests UserAppTheme behavior with all valid enum values including boundary conditions.
        /// </summary>
        [Theory]
        [InlineData((AppTheme)(-1))] // Below minimum
        [InlineData((AppTheme)0)]    // Unspecified
        [InlineData((AppTheme)1)]    // Light
        [InlineData((AppTheme)2)]    // Dark
        [InlineData((AppTheme)3)]    // Above maximum
        [InlineData((AppTheme)int.MaxValue)] // Extreme value
        [InlineData((AppTheme)int.MinValue)] // Extreme value
        public void UserAppTheme_SetBoundaryValues_HandlesAllValues(AppTheme theme)
        {
            // Arrange
            var app = new TestableApplication();
            var initialTheme = AppTheme.Light; // Different from test value
            app.UserAppTheme = initialTheme;
            app.ResetCallCount();

            // Act & Assert - Should not throw
            app.UserAppTheme = theme;
            Assert.Equal(theme, app.UserAppTheme);

            // Should trigger change notification since value is different
            Assert.Equal(1, app.TriggerThemeChangedActualCallCount);
        }

        /// <summary>
        /// Tests multiple consecutive sets with same and different values to ensure consistent behavior.
        /// </summary>
        [Fact]
        public void UserAppTheme_MultipleConsecutiveSets_BehavesConsistently()
        {
            // Arrange
            var app = new TestableApplication();

            // Act & Assert
            app.UserAppTheme = AppTheme.Light;
            Assert.Equal(1, app.TriggerThemeChangedActualCallCount);

            app.UserAppTheme = AppTheme.Light; // Same value
            Assert.Equal(1, app.TriggerThemeChangedActualCallCount); // Should not increment

            app.UserAppTheme = AppTheme.Dark; // Different value
            Assert.Equal(2, app.TriggerThemeChangedActualCallCount); // Should increment

            app.UserAppTheme = AppTheme.Dark; // Same value again
            Assert.Equal(2, app.TriggerThemeChangedActualCallCount); // Should not increment

            app.UserAppTheme = AppTheme.Unspecified; // Different value
            Assert.Equal(3, app.TriggerThemeChangedActualCallCount); // Should increment
        }

        /// <summary>
        /// Helper class that extends Application to allow testing of UserAppTheme property.
        /// Tracks calls to TriggerThemeChangedActual to verify early return behavior.
        /// </summary>
        private class TestableApplication : Application
        {
            public int TriggerThemeChangedActualCallCount { get; private set; }

            public TestableApplication() : base(false)
            {
            }

            public void SetUserAppThemeDirectly(AppTheme theme)
            {
                _userAppTheme = theme;
            }

            public void ResetCallCount()
            {
                TriggerThemeChangedActualCallCount = 0;
            }

            // Override to track calls and prevent actual theme change processing during tests
            protected virtual void TriggerThemeChangedActual()
            {
                TriggerThemeChangedActualCallCount++;

                // Call OnPropertyChanged to simulate the behavior without side effects
                OnPropertyChanged(nameof(UserAppTheme));
            }
        }
    }


    /// <summary>
    /// Tests for the RequestedThemeChanged event in the Application class.
    /// </summary>
    public partial class ApplicationRequestedThemeChangedEventTests
    {
        /// <summary>
        /// Tests that a single event handler can be successfully added to the RequestedThemeChanged event.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddSingleHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handlerCalled = false;
            EventHandler<AppThemeChangedEventArgs> handler = (sender, args) => handlerCalled = true;

            // Act & Assert
            var exception = Record.Exception(() => application.RequestedThemeChanged += handler);

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that a single event handler can be successfully removed from the RequestedThemeChanged event.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_RemoveSingleHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handlerCalled = false;
            EventHandler<AppThemeChangedEventArgs> handler = (sender, args) => handlerCalled = true;
            application.RequestedThemeChanged += handler;

            // Act & Assert
            var exception = Record.Exception(() => application.RequestedThemeChanged -= handler);

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple event handlers can be successfully added to the RequestedThemeChanged event.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddMultipleHandlers_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handler1Called = false;
            bool handler2Called = false;
            bool handler3Called = false;

            EventHandler<AppThemeChangedEventArgs> handler1 = (sender, args) => handler1Called = true;
            EventHandler<AppThemeChangedEventArgs> handler2 = (sender, args) => handler2Called = true;
            EventHandler<AppThemeChangedEventArgs> handler3 = (sender, args) => handler3Called = true;

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                application.RequestedThemeChanged += handler1;
                application.RequestedThemeChanged += handler2;
                application.RequestedThemeChanged += handler3;
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that specific event handlers can be removed from multiple handlers in the RequestedThemeChanged event.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_RemoveSpecificHandlerFromMultiple_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handler1Called = false;
            bool handler2Called = false;
            bool handler3Called = false;

            EventHandler<AppThemeChangedEventArgs> handler1 = (sender, args) => handler1Called = true;
            EventHandler<AppThemeChangedEventArgs> handler2 = (sender, args) => handler2Called = true;
            EventHandler<AppThemeChangedEventArgs> handler3 = (sender, args) => handler3Called = true;

            application.RequestedThemeChanged += handler1;
            application.RequestedThemeChanged += handler2;
            application.RequestedThemeChanged += handler3;

            // Act & Assert
            var exception = Record.Exception(() => application.RequestedThemeChanged -= handler2);

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the same event handler can be added and removed multiple times without exception.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddAndRemoveSameHandlerMultipleTimes_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handlerCalled = false;
            EventHandler<AppThemeChangedEventArgs> handler = (sender, args) => handlerCalled = true;

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                application.RequestedThemeChanged += handler;
                application.RequestedThemeChanged -= handler;
                application.RequestedThemeChanged += handler;
                application.RequestedThemeChanged -= handler;
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that removing a handler that was never added does not throw an exception.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_RemoveNeverAddedHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handlerCalled = false;
            EventHandler<AppThemeChangedEventArgs> handler = (sender, args) => handlerCalled = true;

            // Act & Assert
            var exception = Record.Exception(() => application.RequestedThemeChanged -= handler);

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding the same handler multiple times succeeds without exception.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddSameHandlerMultipleTimes_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            bool handlerCalled = false;
            EventHandler<AppThemeChangedEventArgs> handler = (sender, args) => handlerCalled = true;

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                application.RequestedThemeChanged += handler;
                application.RequestedThemeChanged += handler;
                application.RequestedThemeChanged += handler;
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding a null handler to the RequestedThemeChanged event throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var application = new Application(false);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => application.RequestedThemeChanged += null);

            Assert.Equal("handler", exception.ParamName);
        }

        /// <summary>
        /// Tests that removing a null handler from the RequestedThemeChanged event throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_RemoveNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var application = new Application(false);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => application.RequestedThemeChanged -= null);

            Assert.Equal("handler", exception.ParamName);
        }

        /// <summary>
        /// Tests that lambda expression handlers can be added and removed successfully.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddAndRemoveLambdaHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            EventHandler<AppThemeChangedEventArgs> lambdaHandler = (sender, args) => { /* Lambda handler */ };

            // Act & Assert
            var addException = Record.Exception(() => application.RequestedThemeChanged += lambdaHandler);
            var removeException = Record.Exception(() => application.RequestedThemeChanged -= lambdaHandler);

            Assert.Null(addException);
            Assert.Null(removeException);
        }

        /// <summary>
        /// Tests that method group handlers can be added and removed successfully.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddAndRemoveMethodGroupHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);
            var testInstance = new TestEventHandlerClass();

            // Act & Assert
            var addException = Record.Exception(() => application.RequestedThemeChanged += testInstance.HandleThemeChanged);
            var removeException = Record.Exception(() => application.RequestedThemeChanged -= testInstance.HandleThemeChanged);

            Assert.Null(addException);
            Assert.Null(removeException);
        }

        /// <summary>
        /// Tests that static method handlers can be added and removed successfully.
        /// </summary>
        [Fact]
        public void RequestedThemeChanged_AddAndRemoveStaticMethodHandler_SucceedsWithoutException()
        {
            // Arrange
            var application = new Application(false);

            // Act & Assert
            var addException = Record.Exception(() => application.RequestedThemeChanged += TestEventHandlerClass.StaticHandleThemeChanged);
            var removeException = Record.Exception(() => application.RequestedThemeChanged -= TestEventHandlerClass.StaticHandleThemeChanged);

            Assert.Null(addException);
            Assert.Null(removeException);
        }

        /// <summary>
        /// Helper class for testing different types of event handlers.
        /// </summary>
        private class TestEventHandlerClass
        {
            public void HandleThemeChanged(object sender, AppThemeChangedEventArgs args)
            {
                // Instance method handler
            }

            public static void StaticHandleThemeChanged(object sender, AppThemeChangedEventArgs args)
            {
                // Static method handler
            }
        }
    }


    public partial class ApplicationCleanUpTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CleanUp sets NavigationProxy to null when it was previously null.
        /// Verifies idempotent behavior when NavigationProxy is already null.
        /// </summary>
        [Fact]
        public void CleanUp_WhenNavigationProxyIsNull_RemainsNull()
        {
            // Arrange
            var application = new TestApplication();
            Assert.Null(application.NavigationProxy);

            // Act
            application.TestCleanUp();

            // Assert
            Assert.Null(application.NavigationProxy);
        }

        /// <summary>
        /// Tests that CleanUp sets NavigationProxy to null when it was previously non-null.
        /// Verifies the main functionality of cleaning up the NavigationProxy reference.
        /// </summary>
        [Fact]
        public void CleanUp_WhenNavigationProxyIsNotNull_SetsToNull()
        {
            // Arrange
            var application = new TestApplication();
            var navigationProxy = Substitute.For<NavigationProxy>();

            // Use reflection to set NavigationProxy since it has a private setter
            var propertyInfo = typeof(Application).GetProperty("NavigationProxy");
            propertyInfo.SetValue(application, navigationProxy);

            Assert.NotNull(application.NavigationProxy);

            // Act
            application.TestCleanUp();

            // Assert
            Assert.Null(application.NavigationProxy);
        }

        /// <summary>
        /// Tests that CleanUp can be called multiple times without issues.
        /// Verifies idempotent behavior when called repeatedly.
        /// </summary>
        [Fact]
        public void CleanUp_CalledMultipleTimes_RemainsIdempotent()
        {
            // Arrange
            var application = new TestApplication();
            var navigationProxy = Substitute.For<NavigationProxy>();

            // Use reflection to set NavigationProxy since it has a private setter
            var propertyInfo = typeof(Application).GetProperty("NavigationProxy");
            propertyInfo.SetValue(application, navigationProxy);

            Assert.NotNull(application.NavigationProxy);

            // Act
            application.TestCleanUp();
            application.TestCleanUp();
            application.TestCleanUp();

            // Assert
            Assert.Null(application.NavigationProxy);
        }

        /// <summary>
        /// Test application class that exposes the protected CleanUp method for testing.
        /// </summary>
        private class TestApplication : Application
        {
            public TestApplication() : base(false)
            {
            }

            public void TestCleanUp()
            {
                CleanUp();
            }
        }
    }


    public partial class ApplicationAccentColorTests
    {
        /// <summary>
        /// Tests that the AccentColor getter returns a cached value on subsequent calls.
        /// This verifies the null-coalescing assignment behavior where GetAccentColor() 
        /// is called only once and the result is cached.
        /// </summary>
        [Fact]
        public void AccentColor_Get_CachesValueOnSubsequentCalls()
        {
            // Arrange - Reset static state
            Application.AccentColor = null;

            // Act - Get the accent color twice
            var firstCall = Application.AccentColor;
            var secondCall = Application.AccentColor;

            // Assert - Both calls should return the same value (cached result)
            Assert.Equal(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the AccentColor setter stores the provided Color value.
        /// Verifies that the backing field is properly updated with the set value.
        /// </summary>
        [Fact]
        public void AccentColor_Set_StoresProvidedValue()
        {
            // Arrange
            var testColor = Color.FromRgba(255, 0, 0, 255); // Red color

            // Act
            Application.AccentColor = testColor;

            // Assert
            Assert.Equal(testColor, Application.AccentColor);
        }

        /// <summary>
        /// Tests that the AccentColor setter can store null values.
        /// Verifies that nullable Color values are handled correctly.
        /// </summary>
        [Fact]
        public void AccentColor_Set_AllowsNullValue()
        {
            // Arrange - First set to a non-null value
            Application.AccentColor = Color.FromRgba(0, 255, 0, 255);

            // Act - Set to null
            Application.AccentColor = null;

            // Assert
            Assert.Null(Application.AccentColor);
        }

        /// <summary>
        /// Tests that the AccentColor getter returns the set value after assignment.
        /// Verifies that the cached value is properly overridden when a new value is set.
        /// </summary>
        [Fact]
        public void AccentColor_Get_ReturnsSetValue()
        {
            // Arrange
            var testColor = Color.FromRgba(0, 0, 255, 255); // Blue color

            // Act
            Application.AccentColor = testColor;
            var retrievedColor = Application.AccentColor;

            // Assert
            Assert.Equal(testColor, retrievedColor);
        }

        /// <summary>
        /// Tests AccentColor with various Color values including edge cases.
        /// Verifies that different Color values are properly stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]           // Transparent black
        [InlineData(255, 255, 255, 255)]   // White
        [InlineData(128, 128, 128, 128)]   // Semi-transparent gray
        [InlineData(255, 0, 0, 255)]       // Red
        [InlineData(0, 255, 0, 255)]       // Green
        [InlineData(0, 0, 255, 255)]       // Blue
        public void AccentColor_SetGet_HandlesVariousColorValues(int red, int green, int blue, int alpha)
        {
            // Arrange
            var testColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            Application.AccentColor = testColor;
            var retrievedColor = Application.AccentColor;

            // Assert
            Assert.Equal(testColor, retrievedColor);
        }

        /// <summary>
        /// Tests that setting AccentColor to a new value overrides the previously cached value.
        /// Verifies that the caching mechanism properly updates when a new value is explicitly set.
        /// </summary>
        [Fact]
        public void AccentColor_Set_OverridesCachedValue()
        {
            // Arrange - Reset and get initial cached value
            Application.AccentColor = null;
            var initialValue = Application.AccentColor;

            // Act - Set to a different value
            var newColor = Color.FromRgba(255, 128, 64, 200);
            Application.AccentColor = newColor;

            // Assert
            Assert.Equal(newColor, Application.AccentColor);
            Assert.NotEqual(initialValue, Application.AccentColor);
        }

        /// <summary>
        /// Tests that AccentColor getter works correctly after multiple set operations.
        /// Verifies that the property maintains consistency across multiple assignments.
        /// </summary>
        [Fact]
        public void AccentColor_MultipleSetOperations_MaintainsCorrectValue()
        {
            // Arrange
            var color1 = Color.FromRgba(255, 0, 0, 255);
            var color2 = Color.FromRgba(0, 255, 0, 255);
            var color3 = Color.FromRgba(0, 0, 255, 255);

            // Act & Assert - Set and verify each color
            Application.AccentColor = color1;
            Assert.Equal(color1, Application.AccentColor);

            Application.AccentColor = color2;
            Assert.Equal(color2, Application.AccentColor);

            Application.AccentColor = color3;
            Assert.Equal(color3, Application.AccentColor);
        }

        /// <summary>
        /// Tests that AccentColor handles null assignment after having a cached value.
        /// Verifies that the null-coalescing assignment logic works correctly when 
        /// transitioning from cached value to null.
        /// </summary>
        [Fact]
        public void AccentColor_SetNull_AfterCachedValue()
        {
            // Arrange - First trigger caching by getting the value
            Application.AccentColor = null;
            var cachedValue = Application.AccentColor; // This caches GetAccentColor() result

            // Act - Set to null to override the cached value
            Application.AccentColor = null;

            // Assert
            Assert.Null(Application.AccentColor);
        }
    }


    public partial class ApplicationSendSleepTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that SendSleep method properly calls the OnSleep virtual method.
        /// Verifies that the internal SendSleep method correctly delegates to the protected OnSleep method.
        /// </summary>
        [Fact]
        public void SendSleep_WhenCalled_CallsOnSleepMethod()
        {
            // Arrange
            var testApp = new TestApplication();

            // Act
            testApp.SendSleep();

            // Assert
            Assert.True(testApp.OnSleepCalled);
            Assert.Equal(1, testApp.OnSleepCallCount);
        }

        /// <summary>
        /// Tests that SendSleep can be called multiple times and properly invokes OnSleep each time.
        /// Verifies that the method behaves consistently across multiple invocations.
        /// </summary>
        [Fact]
        public void SendSleep_WhenCalledMultipleTimes_CallsOnSleepEachTime()
        {
            // Arrange
            var testApp = new TestApplication();

            // Act
            testApp.SendSleep();
            testApp.SendSleep();
            testApp.SendSleep();

            // Assert
            Assert.True(testApp.OnSleepCalled);
            Assert.Equal(3, testApp.OnSleepCallCount);
        }

        private class TestApplication : Application
        {
            public bool OnSleepCalled { get; private set; }
            public int OnSleepCallCount { get; private set; }

            public TestApplication() : base(false)
            {
            }

            protected override void OnSleep()
            {
                OnSleepCalled = true;
                OnSleepCallCount++;
                base.OnSleep();
            }
        }
    }
}