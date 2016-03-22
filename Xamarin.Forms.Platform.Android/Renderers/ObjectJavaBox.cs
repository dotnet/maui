using Java.Lang;

namespace Xamarin.Forms.Platform.Android
{
	internal sealed class ObjectJavaBox<T> : Object
	{
		public ObjectJavaBox(T instance)
		{
			Instance = instance;
		}

		public T Instance { get; set; }
	}
}