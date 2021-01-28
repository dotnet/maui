using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformRequestAsync(ShareMultipleFilesRequest request) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
