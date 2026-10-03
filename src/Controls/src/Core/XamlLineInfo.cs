namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>Provides line and position information for XAML processing.</summary>
	public sealed class XamlLineInfo
	{
		readonly bool _hasLineInfo;

		/// <summary>Creates a new <see cref="XamlLineInfo"/> with no line information.</summary>
		public XamlLineInfo()
		{
		}

		/// <summary>Creates a new <see cref="XamlLineInfo"/> with the specified line and position.</summary>
		/// <param name="lineNumber">The line number.</param>
		/// <param name="linePosition">The position within the line.</param>
		public XamlLineInfo(int lineNumber, int linePosition)
		{
			_hasLineInfo = true;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		/// <summary>Returns whether line information is available.</summary>
		public bool HasLineInfo() => _hasLineInfo;

		/// <summary>Gets the line number.</summary>
		public int LineNumber { get; }

		/// <summary>Gets the position within the line.</summary>
		public int LinePosition { get; }
	}
}
