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
                nfloat[] dashArray;
                double[] array;

                if (doubleCollection.Count % 2 == 0)
                {
                    array = new double[doubleCollection.Count];
                    dashArray = new nfloat[doubleCollection.Count];
                    doubleCollection.CopyTo(array, 0);
                }
                else
                {
                    array = new double[2 * doubleCollection.Count];
                    dashArray = new nfloat[2 * doubleCollection.Count];
                    doubleCollection.CopyTo(array, 0);
                    doubleCollection.CopyTo(array, doubleCollection.Count);
                }

                for (int i = 0; i < array.Length; i++)
                {
                    dashArray[i] = new nfloat(array[i]);
                }

                return dashArray;
            }
        }
    }
}