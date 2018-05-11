using System;
using ElmSharp;
using Tizen.System;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            if (EcoreMainloop.IsMainThread)
                action();
            else
                EcoreMainloop.PostAndWakeUp(action);
        }

        internal static string GetSystemInfo(string item)
        {
            Information.TryGetValue<string>($"http://tizen.org/system/{item}", out var value);
            return value;
        }

        internal static T GetSystemInfo<T>(string item)
        {
            Information.TryGetValue<T>($"http://tizen.org/system/{item}", out var value);
            return value;
        }

        internal static string GetFeatureInfo(string item)
        {
            Information.TryGetValue<string>($"http://tizen.org/feature/{item}", out var value);
            return value;
        }

        internal static T GetFeatureInfo<T>(string item)
        {
            Information.TryGetValue<T>($"http://tizen.org/feature/{item}", out var value);
            return value;
        }
    }
}
