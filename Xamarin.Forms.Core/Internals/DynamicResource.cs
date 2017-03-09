using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		public DynamicResource(string key)
		{
			Key = key;
		}

		public string Key { get; private set; }
	}
}