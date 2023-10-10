using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Service;
using UITest.Core;

namespace UITest.Appium
{
    public class AppiumServerContext : IServerContext
    {
        const int Port = 4723;
        readonly int _serverStartWaitDelay = 1000;
        readonly List<AppiumUIClientContext> _contexts = new(); // Since tests don't know when they are done, we need to keep track of all the contexts we create so we can dispose them
        readonly TimeSpan _maxServerWaitTime = TimeSpan.FromSeconds(15);
        readonly object _serverLock = new object();
        AppiumLocalService? _server;

        public IUIClientContext CreateUIClientContext(IConfig config)
        {
            lock (_serverLock)
            {
                if (_server == null || !_server.IsRunning)
                {
                    Debug.WriteLine($">>>>> Server was not running when calling {nameof(CreateUIClientContext)}, starting it ourselves...");
                    CreateAndStartServer();
                }
            }

            int retries = 0;

            var testDevice = config.GetProperty<TestDevice>("TestDevice");
            var driverUri = new Uri($"http://localhost:{Port}/wd/hub");

            while (true)
            {
                try
                {
                    // TODO: Create these IApp instances should not be hardcoded types
                    IApp app = testDevice switch
                    {
                        TestDevice.Mac => new AppiumCatalystApp(driverUri, config),
                        TestDevice.Windows => new AppiumWindowsApp(driverUri, config),
                        TestDevice.Android => AppiumAndroidApp.CreateAndroidApp(driverUri, config),
                        TestDevice.iOS => new AppiumIOSApp(driverUri, config),
                        _ => throw new InvalidOperationException("Unknown test device"),
                    };

                    var newContext = new AppiumUIClientContext(app, config);
                    _contexts.Add(newContext);
                    return newContext;
                }
                catch (WebDriverException)
                {
                    // Default command timeout is 60 seconds when executing the NewSessionCommand
                    if (retries++ < 10)
                    {
                        Debug.WriteLine($">>>>> Retrying to create the driver, attempt #{retries}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public void CreateAndStartServer(int port = Port)
        {
            lock (_serverLock)
            {
                _server?.Dispose();

                var arguments = new OpenQA.Selenium.Appium.Service.Options.OptionCollector();
                arguments.AddArguments(new KeyValuePair<string, string>("--base-path", "/wd/hub"));

                var logFile = Environment.GetEnvironmentVariable("APPIUM_LOG_FILE") ?? "appium.log";

                var service = new AppiumServiceBuilder()
                    .WithArguments(arguments)
                    .UsingPort(port)
                    .WithLogFile(new FileInfo(logFile))
                    .Build();

                service.OutputDataReceived += (s, e) => Debug.WriteLine($"Appium {e.Data}");
                service.Start();
                _server = service;

                DateTime start = DateTime.Now;

                while (!_server.IsRunning)
                {
                    long elapsed = DateTime.Now.Subtract(start).Ticks;
                    if (elapsed >= _maxServerWaitTime.Ticks)
                    {
                        Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {_maxServerWaitTime.Ticks}");

                        throw new TimeoutException($"Timed out waiting for Appium server to start after waiting for {_maxServerWaitTime.Seconds}s");
                    }

                    Task.Delay(_serverStartWaitDelay).Wait();
                }
            }
        }

        public void Dispose()
        {
            foreach (var context in _contexts)
            {
                context.Dispose();
            }

            _contexts.Clear();

            lock (_serverLock)
            {
                _server?.Dispose();
                _server = null;
            }
        }
    }
}
