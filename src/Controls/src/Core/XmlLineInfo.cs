#nullable disable
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>Provides line and position information for XAML parsing.</summary>
	public class XmlLineInfo : IXmlLineInfo
	{
		readonly bool _hasLineInfo;

		/// <summary>Creates a new <see cref="XmlLineInfo"/> with no line information.</summary>
		public XmlLineInfo()
		{
		}

		/// <summary>Creates a new <see cref="XmlLineInfo"/> with the specified line and position.</summary>
		/// <param name="linenumber">The line number.</param>
		/// <param name="lineposition">The position within the line.</param>
		public XmlLineInfo(int linenumber, int lineposition)
		{
			_hasLineInfo = true;
			LineNumber = linenumber;
			LinePosition = lineposition;
		}

		/// <summary>Returns whether line information is available.</summary>
		public bool HasLineInfo()
		{
			return _hasLineInfo;
		}

		/// <summary>Gets the line number.</summary>
		public int LineNumber { get; }

		/// <summary>Gets the position within the line.</summary>
		public int LinePosition { get; }
	}
}