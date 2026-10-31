using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests.Handlers
{
	[Category(TestCategory.Core)]
	public class HandlerConstructorTests
	{
		public static IEnumerable<object[]> ViewHandlers =>
			typeof(ViewHandler).Assembly.ExportedTypes
				.Where(type => !type.IsAbstract && !type.ContainsGenericParameters && typeof(IViewHandler).IsAssignableFrom(type))
				// SwipeItemViewHandler exposes its mapper constructors only to derived handlers.
				.Where(type => type != typeof(SwipeItemViewHandler))
				.Select(type => new object[] { type });

		// API shape must be checked before trimming, which can legitimately remove unused overloads.
		[Theory]
		[MemberData(nameof(ViewHandlers))]
		public void HandlersHaveAllExpectedConstructors(Type handlerType)
		{
			Assert.Contains(handlerType.GetConstructors(), constructor =>
			{
				var parameters = constructor.GetParameters();
				return parameters.Length == 2 &&
					typeof(IPropertyMapper).IsAssignableFrom(parameters[0].ParameterType) &&
					typeof(CommandMapper).IsAssignableFrom(parameters[1].ParameterType);
			});
		}
	}
}
