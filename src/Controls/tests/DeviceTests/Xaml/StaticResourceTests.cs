using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Xaml)]
public class StaticResourceTests : IDisposable
{
    [Fact("Issue #23903: Missing ControlTemplate with exception handler should throw")]
    [RequiresUnreferencedCode("XAML parsing may require unreferenced code")]
    public void MissingControlTemplate_WithExceptionHandler_ShouldThrow()
    {
        // Issue #23903: StaticResourceExtension should always throw when resource is not found,
        // even when an exception handler is present (for debug/hot reload scenarios).
        // This prevents the app from crashing when relaunching.

        Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) => { };

        var xaml = """
			<?xml version="1.0" encoding="UTF-8"?>
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             ControlTemplate="{StaticResource InvalidTemplate}">
			</ContentPage>
			""";

        var page = new ContentPage();

        // Should throw an exception even with handler present
        bool exceptionThrown = false;
        try
        {
            page.LoadFromXaml(xaml);
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }

        Assert.True(exceptionThrown, "Expected an exception to be thrown for missing ControlTemplate");
    }

    public void Dispose()
    {
        Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
    }
}