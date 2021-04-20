namespace Microsoft.Maui.Graphics.Android
{
	public class FontInfo
	{
		public FontInfo(string path, string family, string style, string fullName)
		{
			Path = path;
			Family = family;
			Style = style;
			FullName = fullName;
		}

		public string Path { get; }
		public string Family { get; }
		public string Style { get; }
		public string FullName { get; }
	}
}
