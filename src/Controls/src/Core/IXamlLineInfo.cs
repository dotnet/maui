namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>Provides line and position information for XAML processing without depending on an XML reader.</summary>
	public interface IXamlLineInfo
	{
		/// <summary>Returns whether line information is available.</summary>
		bool HasLineInfo();

		/// <summary>Gets the line number.</summary>
		int LineNumber { get; }

		/// <summary>Gets the position within the line.</summary>
		int LinePosition { get; }
	}
}
