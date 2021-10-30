#nullable enable
using Android.Content;
using Bumptech.Glide;

namespace Microsoft.Maui.BumptechGlide
{
	public static class RequestManagerExtensions
	{
		public static RequestBuilder Load(this RequestManager requestManager, string drawableIdOrFilename, Context context)
		{
			if (context.GetDrawableId(drawableIdOrFilename) is int id && id != 0)
				return requestManager.Load(id);

			return requestManager.Load(drawableIdOrFilename);
		}
	}
}