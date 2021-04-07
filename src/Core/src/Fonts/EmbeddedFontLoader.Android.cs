using System.IO;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : FileSystemEmbeddedFontLoader
	{
		public EmbeddedFontLoader()
			: base(Path.GetTempPath())
		{
		}
	}
}