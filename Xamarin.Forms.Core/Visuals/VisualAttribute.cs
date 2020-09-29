using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class VisualAttribute : Attribute
	{
		public VisualAttribute(string key, Type visual)
		{
			this.Key = key;
			this.Visual = visual;
		}

		internal string Key { get; }
		internal Type Visual { get; }
	}
}