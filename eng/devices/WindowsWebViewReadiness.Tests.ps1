#Requires -Version 7.2
#Requires -Modules Pester

Describe 'Windows Blazor WebView initialization diagnostics' {
    BeforeAll {
        $stubsPath = Join-Path $TestDrive 'WebViewDependencies.cs'
        @'
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Web.WebView2.Core
{
    public class CoreWebView2
    {
        public event EventHandler DOMContentLoaded { add { } remove { } }
        public Task<string> ExecuteScriptAsync(string javaScript) => Task.FromResult("true");
    }
    public static class CoreWebView2Environment
    {
        public static string GetAvailableBrowserVersionString(string folder) => "test-runtime";
    }
}
namespace Microsoft.UI.Xaml.Controls
{
    using Microsoft.Web.WebView2.Core;
    public class CoreWebView2InitializedEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
    public class WebView2
    {
        public CoreWebView2 CoreWebView2 { get; set; }
        public bool FailInitialization { get; set; }
        public bool ThrowDuringInitialization { get; set; }
        public int EnsureCalls { get; private set; }
        public int InitializationSubscribers => CoreWebView2Initialized?.GetInvocationList().Length ?? 0;
        public event Action<WebView2, CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

        public Task EnsureCoreWebView2Async()
        {
            EnsureCalls++;
            if (ThrowDuringInitialization)
                throw new InvalidOperationException("Ensure failed");
            var error = FailInitialization
                ? new COMException("Native initialization failed", unchecked((int)0x80070005))
                : null;
            if (error == null)
                CoreWebView2 = new CoreWebView2();
            CoreWebView2Initialized?.Invoke(this, new CoreWebView2InitializedEventArgs { Exception = error });
            return Task.CompletedTask;
        }
    }
}
namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
    public static partial class WebViewHelpers
    {
        private static async Task Retry(Func<Task<bool>> action, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
        {
            if (!await action())
                throw await createExceptionWithTimeoutMS(30000);
        }
    }
}
'@ | Set-Content -LiteralPath $stubsPath
        $sourcePath = Join-Path $PSScriptRoot '../../src/BlazorWebView/tests/DeviceTests/WebViewHelpers.Windows.cs'
        Add-Type -Path $stubsPath, $sourcePath
    }

    It 'Reports the native initialization failure instead of a readiness timeout' {
        $webView = [Microsoft.UI.Xaml.Controls.WebView2]::new()
        $webView.FailInitialization = $true

        { [Microsoft.Maui.MauiBlazorWebView.DeviceTests.WebViewHelpers]::WaitForWebViewReady($webView).GetAwaiter().GetResult() } |
            Should -Throw '*WebView2 initialization failed*0x80070005*'
        $webView.InitializationSubscribers | Should -Be 0
    }

    It 'Preserves an exception thrown by EnsureCoreWebView2Async and removes the observer' {
        $webView = [Microsoft.UI.Xaml.Controls.WebView2]::new()
        $webView.ThrowDuringInitialization = $true

        { [Microsoft.Maui.MauiBlazorWebView.DeviceTests.WebViewHelpers]::WaitForWebViewReady($webView).GetAwaiter().GetResult() } |
            Should -Throw '*Ensure failed*'
        $webView.InitializationSubscribers | Should -Be 0
    }

    It 'Initializes a new WebView and removes the observer' {
        $webView = [Microsoft.UI.Xaml.Controls.WebView2]::new()

        [Microsoft.Maui.MauiBlazorWebView.DeviceTests.WebViewHelpers]::WaitForWebViewReady($webView).GetAwaiter().GetResult()

        $webView.EnsureCalls | Should -Be 1
        $webView.CoreWebView2 | Should -Not -BeNullOrEmpty
        $webView.InitializationSubscribers | Should -Be 0
    }

    It 'Does not initialize an already ready WebView again' {
        $webView = [Microsoft.UI.Xaml.Controls.WebView2]::new()
        $webView.CoreWebView2 = [Microsoft.Web.WebView2.Core.CoreWebView2]::new()

        [Microsoft.Maui.MauiBlazorWebView.DeviceTests.WebViewHelpers]::WaitForWebViewReady($webView).GetAwaiter().GetResult()

        $webView.EnsureCalls | Should -Be 0
        $webView.InitializationSubscribers | Should -Be 0
    }
}
