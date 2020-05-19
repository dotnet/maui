using System.ComponentModel;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplateController
	{
		int Id { get; }
		string IdString { get; }
	}
}
