using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Core.UnitTests.TestClasses
{
	class ElementHandlerStub : ElementHandler<Microsoft.Maui.Controls.Cell, object>
	{
		public ElementHandlerStub(IPropertyMapper mapper, CommandMapper commandMapper = null) : base(mapper, commandMapper)
		{
		}

		public ElementHandlerStub() : base(new PropertyMapper<IView>())
		{

		}

		protected override object CreatePlatformElement()
		{
			return new object();
		}
	}
}
