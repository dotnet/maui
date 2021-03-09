using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplateController
	{
		int Id { get; }
		string IdString { get; }
	}
}
