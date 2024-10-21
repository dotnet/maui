﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Mapper)]
	public class MapperTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(MapperGenericTypeCases))]
		public void ValidateMapperGenerics(IPropertyMapper propertyMapper, Type viewType, Type handlerType)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(h => h.AddMauiControlsHandlers()));
			var generics = propertyMapper.GetType().GenericTypeArguments;
			Assert.Equal(viewType, generics[0]);
			Assert.Equal(handlerType, generics[1]);
		}

		class MapperGenericTypeCases : IEnumerable<object[]>
		{
			private readonly List<object[]> _data = new()
			{
				new object[] { ViewHandler.ViewMapper, typeof(IView), typeof(IViewHandler) },
				new object[] { ElementHandler.ElementMapper, typeof(IElement), typeof(IElementHandler) },
			};

			public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
