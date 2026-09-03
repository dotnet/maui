using Java.Lang;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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