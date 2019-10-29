using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		public string Key { get; private set; }
		public DynamicResource(string key) => Key = key;
	}
}