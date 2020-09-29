using System;

namespace Xamarin.Forms.Xaml
{
	[ProvideCompiled("Xamarin.Forms.Build.Tasks.NullExtension")]
	[AcceptEmptyServiceProvider]
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}