namespace Xamarin.Forms.Internals
{
	public class DynamicResource
	{
		public DynamicResource(string key)
		{
			Key = key;
		}

		public string Key { get; private set; }
	}
}