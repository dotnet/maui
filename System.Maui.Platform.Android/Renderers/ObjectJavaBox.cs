using Java.Lang;

namespace System.Maui.Platform.Android
{
	internal sealed class ObjectJavaBox<T> : Java.Lang.Object
	{
		public ObjectJavaBox(T instance)
		{
			Instance = instance;
		}

		public T Instance { get; set; }
	}
}