using System.Threading;

namespace Microsoft.Maui.Graphics
{
    public static class Fonts
    {
        private static IFontService _globalService;
        private static ThreadLocal<IFontService> _threadLocalService;

        /// <summary>
        /// Registers the global service to be used.
        /// </summary>
        /// <param name="service"></param>
        public static void RegisterGlobalService(IFontService service)
        {
            _globalService = service ?? new VirtualFontService();
        }

        /// <summary>
        /// Registers graphics service instance to be used for the current rendering thread
        /// </summary>
        /// <param name="service"></param>
        public static void RegisterThreadLocalContext(IFontService service)
        {
            if (_threadLocalService == null)
                _threadLocalService = new ThreadLocal<IFontService>();

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

        public static IFontService GlobalService
        {
            get
            {
                if (_globalService == null)
                {
                    _globalService = new VirtualFontService();
                    Logger.Warn("No font service was registered.  Falling back to the virtual implementation.");
                }

                return _globalService;
            }
        }

        public static IFontService CurrentService
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
                    _globalService = new VirtualFontService();
                    Logger.Warn("No font service was registered.  Falling back to the virtual implementation.");
                }

                return _globalService;
            }
        }

        public static void Register(IFontService service)
        {
            _globalService = service;
        }
    }
}
