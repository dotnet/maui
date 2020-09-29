using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

[assembly: Dependency(typeof(MockDispatcherProvider))]
namespace Xamarin.Forms.Core.UnitTests
{
	public class MockDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetDispatcher(object context)
		{
			return new MockDispatcher();
		}
	}
}