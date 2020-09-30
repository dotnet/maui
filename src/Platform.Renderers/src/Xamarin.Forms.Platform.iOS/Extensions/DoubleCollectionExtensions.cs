using System;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public static class DoubleCollectionExtensions
    {
        public static nfloat[] ToArray(this DoubleCollection doubleCollection)
        {
            if (doubleCollection == null || doubleCollection.Count == 0)
                return new nfloat[0];
            else
            {

                nfloat[] array = new nfloat[doubleCollection.Count];

                for (int i = 0; i < doubleCollection.Count; i++)
                    array[i] = (nfloat)doubleCollection[i];

                return array;
            }
        }
    }
}