namespace Microsoft.Maui.Controls.Xaml;

//used for unit testing switching, and internal use of the sourcegen
enum XamlInflator
{
	Runtime = 1 << 0,
	SourceGen = 1 << 2,
}