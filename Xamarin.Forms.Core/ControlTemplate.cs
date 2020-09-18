using System;

namespace Xamarin.Forms
{
	public class ControlTemplate : ElementTemplate
	{
		public ControlTemplate()
		{
		}

		public ControlTemplate(Type type) : base(type)
		{
		}

		public ControlTemplate(Func<object> createTemplate) : base(createTemplate) 
		{ 
		}
	}
}