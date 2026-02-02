using System;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that returns <see langword="null"/>.
	/// </summary>
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
