using UITest.Core;

namespace UITest.Appium
{
    public class AppiumUIClientContext : IUIClientContext
    {
        bool _disposed;
        readonly IApp _app;
        readonly IConfig _config;

        public AppiumUIClientContext(IApp app, IConfig config)
        {
            _app = app;
            _config = config;
        }

        public IApp App { get { return _disposed ? throw new ObjectDisposedException("Accessing IApp that has been disposed") : _app; } }

        public IConfig Config => _config;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            App.Dispose();
            _disposed = true;
        }
    }
}
