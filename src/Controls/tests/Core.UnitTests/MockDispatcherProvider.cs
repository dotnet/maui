using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

[assembly: Dependency(typeof(MockDispatcherProvider))]
namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MockDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetDispatcher(object context)
		{
			return new MockDispatcher();
		}
	}
}
