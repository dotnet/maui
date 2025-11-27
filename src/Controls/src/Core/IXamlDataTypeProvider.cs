using System.ComponentModel;

namespace Microsoft.Maui.Controls.Xaml;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXamlDataTypeProvider
{
	string BindingDataType { get; }
}
