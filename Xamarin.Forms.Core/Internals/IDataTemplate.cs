using System;

namespace Xamarin.Forms.Internals
{
	[Obsolete]
	public interface IDataTemplate
	{
		Func<object> LoadTemplate { get; set; }
	}
}