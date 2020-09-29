using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplateController
	{
		int Id { get; }
		string IdString { get; }
	}
}