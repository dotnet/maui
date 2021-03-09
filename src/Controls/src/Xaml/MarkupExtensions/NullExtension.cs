using System;

namespace Microsoft.Maui.Controls.Xaml
{
	[ProvideCompiled("Microsoft.Maui.Controls.Build.Tasks.NullExtension")]
	[AcceptEmptyServiceProvider]
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}
