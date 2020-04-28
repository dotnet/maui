using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformExecute(HapticFeedbackType type) => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
