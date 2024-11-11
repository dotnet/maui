namespace Microsoft.Maui.Controls.Xaml
{
	public enum XamlInflator
	{
		/// <summary>
		/// Picks the best Inflator available, don't change it unless you know what you're doing.
		/// </summary>
		Default = 0,
		Runtime = 1 << 0,
		XamlC = 1 << 1,
		SourceGen = 1 << 2,
	}
}