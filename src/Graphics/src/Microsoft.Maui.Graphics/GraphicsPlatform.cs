using System.Threading;

namespace Microsoft.Maui.Graphics
{
    public static class GraphicsPlatform
    {
        private static IGraphicsService _globalService;
        private static ThreadLocal<IGraphicsService> _threadLocalService;

        /// <summary>
        /// Registers the global service to be used.
        /// </summary>
        /// <param name="service"></param>
        public static void RegisterGlobalService(IGraphicsService service)
        {
            _globalService = service ?? new VirtualGraphicsPlatform();
        }

        /// <summary>
        /// Registers graphics service instance to be used for the current rendering thread
        /// </summary>
        /// <param name="service"></param>
        public static void RegisterThreadLocalContext(IGraphicsService service)
        {
            _threadLocalService ??= new ThreadLocal<IGraphicsService>();
            _threadLocalService.Value = service;
        }

        /// <summary>
        /// Clears the context on the local thread in one has been set.
        /// </summary>
        public static void ClearThreadLocalContext()
        {
            if (_threadLocalService != null)
                _threadLocalService.Value = null;
        }

        public static IGraphicsService GlobalService
        {
            get
            {
                if (_globalService == null)
                {
                    _globalService = new VirtualGraphicsPlatform();
                    Logger.Warn("No graphics platform was registered.  Falling back to the virtual implementation.");
                }

                return _globalService;
            }
        }

        public static IGraphicsService CurrentService
        {
            get
            {
                if (_threadLocalService != null && _threadLocalService.IsValueCreated)
                {
                    var localContext = _threadLocalService.Value;
                    if (localContext != null)
                        return localContext;
                }

                if (_globalService == null)
                {
                    _globalService = new VirtualGraphicsPlatform();
                    Logger.Warn("No graphics platform was registered.  Falling back to the virtual implementation.");
                }

                return _globalService;
            }
        }

        public static void Register(IGraphicsService service)
        {
            _globalService = service;
        }
    }
}
