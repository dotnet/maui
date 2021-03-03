namespace Microsoft.Maui.Hosting
{
	public static class FontCollectionExtensions
	{
		public static IFontCollection AddFont(this IFontCollection fontCollection, string filename, string? alias = null)
		{
			fontCollection.Add(new FontDescriptor(filename, alias));
			return fontCollection;
		}
	}
}
