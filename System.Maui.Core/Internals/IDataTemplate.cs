using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplate
	{
		Func<object> LoadTemplate { get; set; }
	}
}