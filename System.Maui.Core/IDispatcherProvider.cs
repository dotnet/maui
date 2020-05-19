using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDispatcherProvider
	{
		IDispatcher GetDispatcher(object context);
	}
}
