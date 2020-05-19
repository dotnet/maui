using System;
using System.ComponentModel;

namespace System.Maui.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplate
	{
		Func<object> LoadTemplate { get; set; }
	}
}