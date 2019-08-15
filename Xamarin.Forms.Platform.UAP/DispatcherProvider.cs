using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: Dependency(typeof(DispatcherProvider))]
namespace Xamarin.Forms.Platform.UWP
{
	internal class DispatcherProvider : IDispatcherProvider
	{
		[ThreadStatic]
		static Dispatcher s_current;

		public IDispatcher GetDispatcher(object context)
		{
			return s_current = s_current ?? new Dispatcher();
		}
	}
}
