using Windows.System.Display;

namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        static readonly object locker = new object();
        static DisplayRequest displayRequest;

        static bool PlatformIsActive
        {
            get
            {
                lock (locker)
                {
                    return displayRequest != null;
                }
            }
        }

        static void PlatformRequestActive()
        {
            lock (locker)
            {
                if (displayRequest == null)
                {
                    displayRequest = new DisplayRequest();
                    displayRequest.RequestActive();
                }
            }
        }

        static void PlatformRequestRelease()
        {
            lock (locker)
            {
                if (displayRequest != null)
                {
                    displayRequest.RequestRelease();
                    displayRequest = null;
                }
            }
        }
    }
}
