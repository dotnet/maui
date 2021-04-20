namespace Microsoft.Maui.Graphics
{
	public interface IFontFamily
	{
		string Name { get; }
		IFontStyle[] GetFontStyles();
	}
}
