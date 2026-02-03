using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Xaml)]
public class StaticResourceTests : IDisposable
{
    public void Dispose()
    {
        Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
    }

    [Fact("Issue #23903: Missing ControlTemplate with exception handler should throw")]
    [RequiresUnreferencedCode("XAML parsing may require unreferenced code")]
    public void MissingControlTemplate_WithExceptionHandler_ShouldThrow()
    {
        // Issue #23903: When an exception handler is present (like in debug mode or hot reload),
        // the StaticResourceExtension should still throw when a resource is not found.
        // The old behavior was to return null, causing the Page to be null and crashing later
        // when IWindow.Content was accessed.

        bool handlerCalled = false;
        Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
        {
            var (exception, filepath) = ex;
            handlerCalled = true;
        };

        var xaml = """
			<?xml version="1.0" encoding="UTF-8"?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             ControlTemplate="{StaticResource InvalidTemplate}">
			</ContentPage>
			""";

        var page = new ContentPage();

        // The exception should be thrown even though a handler is present
        var exception = Assert.Throws<XamlParseException>(() => page.LoadFromXaml(xaml));

        // Verify the handler was called (for logging purposes)
        Assert.True(handlerCalled, "Exception handler should be called before throwing");

        // Verify the exception message
        Assert.Contains("StaticResource not found for key InvalidTemplate", exception.Message, StringComparison.Ordinal);
    }
}

