namespace Microsoft.Maui
{
	
	/// <summary>
	/// Provides information about whether the ILayout is compressed.
	/// </summary>
	/// <remarks>
	/// A compressed layout is not included in the platform tree.
	/// </remarks>
	internal interface ICompressedLayout
	{
		/// <summary>
		/// Specifies whether the ILayout is compressed.
		/// </summary>
		bool IsHeadless { get; }
	}
}
