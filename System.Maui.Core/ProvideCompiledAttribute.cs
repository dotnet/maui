using System;

namespace System.Maui.Xaml
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	sealed class ProvideCompiledAttribute : Attribute
	{
		public string CompiledVersion { get; }

		public ProvideCompiledAttribute (string compiledVersion)
		{
			CompiledVersion = compiledVersion;
		}
	}
}