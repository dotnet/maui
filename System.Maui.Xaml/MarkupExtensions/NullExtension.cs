using System;

namespace System.Maui.Xaml
{
	[ProvideCompiled("System.Maui.Build.Tasks.NullExtension")]
	[AcceptEmptyServiceProvider]
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}
