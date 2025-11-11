using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maui.Controls.Sample;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(string))]
internal partial class HybridWebViewTypes : JsonSerializerContext {
    // This type's attributes specify JSON serialization info to preserve type structure
    // for trimmed builds.  
}

public partial class App : Application
{
    public App()
    {
        //InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MyMainPage());
    }
}

public class MyMainPage: ContentPage {
    HybridWebView hybridWebView;
    public MyMainPage() {
        AbsoluteLayout abs = new();
        this.Content = abs;

        hybridWebView = new();
        hybridWebView.HybridRoot = "test";
        hybridWebView.DefaultFile= "index.html";
        abs.Add(hybridWebView);
        hybridWebView.RawMessageReceived += async delegate (object? sender, HybridWebViewRawMessageReceivedEventArgs e) {
            Debug.WriteLine("C# RAW MESSAGE RECEIVED: " + e?.Message);
        };

        this.SizeChanged += delegate (object? o, EventArgs e) {
            if (this.Width > 0 && this.Height > 0) {
                double screenWidth = this.Width;
                double screenHeight = this.Height;
                hybridWebView.HeightRequest = screenHeight;
                hybridWebView.WidthRequest = screenWidth;
            }
        };

        var timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.IsRepeating = true;
        timer.Start();
        timer.Tick += async delegate {
            Debug.WriteLine("Timer tick");

            // create test object 
            Dictionary<string, object> contextArg = new() {
                { "userId", "userIdValue" },
            };

            // can't pass in dictionary<string,object> to hybridwebview - must convert to string first
            string contextArgString = JsonSerializer.Serialize(contextArg);

            //====================================================================
            // COMMENT THIS LINE OUT AND IT FAILS IN WINDOWS BUT NOT ANDROID
            //====================================================================
            // windows glitches and times out when you feed it a json string as argument - must use base64
            // contextArgString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(contextArgString));
            //====================================================================

            TimeSpan timeout = TimeSpan.FromSeconds(5);
            using CancellationTokenSource cts = new CancellationTokenSource(timeout);
            try {
                Debug.WriteLine("START SET TEST FUNCTION with arg: " + contextArgString);
                // must await or can't catch the error
                var reply = await hybridWebView.InvokeJavaScriptAsync<string>(
                    "window.testFunctionName",
                    HybridWebViewTypes.Default.String, // return type
                    new object[] { contextArgString }, // arguments as []
                    new[] { HybridWebViewTypes.Default.String } // argument types as []
                );

                Debug.WriteLine("Set test function, reply: " + reply?.ToString());
            }
            catch (OperationCanceledException) { Debug.WriteLine($"testFunctionName timed out"); }
            catch (Exception ex) { Debug.WriteLine("EXCEPTION WRITING testFunctionName: " + ex.Message); }

        };

    }

}