using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class MockServiceProvider : IServiceProvider
	{
		public MockServiceProvider(params (Type serviceType, object serviceImplementation)[] services)
		{
			if (services != null)
			{
				foreach (var s in services)
					_services[s.serviceType] = s.serviceImplementation;
			}

			if (!_services.ContainsKey(typeof(IFontRegistrar)))
				_services.Add(typeof(IFontRegistrar), new MockFontRegistrar());
			if (!_services.ContainsKey(typeof(IFontManager)))
				_services.Add(typeof(IFontManager), new MockFontManager());
		}

		Dictionary<Type, object> _services = new();

		public object GetService(Type serviceType)
			=> _services.TryGetValue(serviceType, out var service) ? service : null;
	}
}