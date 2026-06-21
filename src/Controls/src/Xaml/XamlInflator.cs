namespace Microsoft.Maui.Controls.Xaml;

//used for unit testing switching, and internal use of the sourcegen
enum XamlInflator
{
	Runtime = 1 << 0,
	XamlC = 1 << 1,
	SourceGen = 1 << 2,
}