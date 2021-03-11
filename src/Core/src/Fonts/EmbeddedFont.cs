using System;
using System.IO;

namespace Microsoft.Maui
{
	public class EmbeddedFont
	{
		public string? FontName { get; set; }
		public Stream? ResourceStream { get; set; }
	}
}
