namespace Microsoft.Maui.Hosting
{
	public class FontDescriptor
	{
		public FontDescriptor(string filename, string? alias)
		{
			Filename = filename;
			Alias = alias;
		}

		public string Filename { get; }

		public string? Alias { get; }
	}
}
