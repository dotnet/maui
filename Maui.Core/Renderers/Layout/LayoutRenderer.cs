using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Platform
{
	public partial class LayoutRenderer 
	{
		public static PropertyMapper<ILayout> LayoutMapper = new PropertyMapper<ILayout>(ViewRenderer.ViewMapper) 
		{
			// Maps here
		};

		public LayoutRenderer() : base(LayoutMapper)
		{

		}

		public LayoutRenderer(PropertyMapper mapper) : base(mapper ?? LayoutMapper)
		{

		}
	}
}
