namespace Xamarin.Forms.Platform.Android
{
	public static class DoubleCollectionExtensions
	{
		public static float[] ToArray(this DoubleCollection doubleCollection)
		{
			float[] array = new float[doubleCollection.Count];

			for (int i = 0; i < doubleCollection.Count; i++)
			{
				array[i] = (float)doubleCollection[i];
			}

			return array;
		}
	}
}