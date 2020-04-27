using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static async Task PlatformExecute(HapticFeedbackType type)
        {
            await Task.FromResult(0);
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }
    }
}
