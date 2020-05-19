using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Maui;
using System.Maui.Core.UnitTests;

[assembly: Dependency(typeof(MockDispatcherProvider))]
namespace System.Maui.Core.UnitTests
{
	public class MockDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetDispatcher(object context)
		{
			return new MockDispatcher();
		}
	}
}
