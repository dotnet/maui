using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform.Handlers
{
	public partial class LayoutHandler
	{
		public static PropertyMapper<ILayout> LayoutMapper = new PropertyMapper<ILayout>(ViewHandler.ViewMapper)
		{
		};

		public LayoutHandler() : base(LayoutMapper)
		{

		}

		public LayoutHandler(PropertyMapper mapper) : base(mapper ?? LayoutMapper)
		{

		}
	}
}
