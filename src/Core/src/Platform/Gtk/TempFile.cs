using System;
using System.IO;

namespace Microsoft.Maui
{

	public class TempFile : IDisposable
	{

		public TempFile()
		{
			Name = Path.GetTempFileName();
		}

		public string Name { get; }

		public static implicit operator string(TempFile d) => d.Name;

		public override string ToString() => Name;

		public void Dispose()
		{
			var fi = new FileInfo(Name);

			if (fi.Exists)
				fi.Delete();
		}

	}

}