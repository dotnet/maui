using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplate
	{
		Func<object> LoadTemplate { get; set; }
	}
}