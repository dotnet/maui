using System;

namespace Microsoft.Maui.Controls
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class ParameterAttribute : Attribute
	{
		public ParameterAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}