#nullable enable
namespace Microsoft.Maui.Controls
{
	public class AppThemeResource
	{
		public object? Light { get; set; }

		public object? Dark { get; set; }

		public object? Default { get; set; }

		internal BindingBase GetBinding()
		{
			var binding = new AppThemeBinding();
			if (Light is not null)
				binding.Light = Light;
			if (Dark is not null)
				binding.Dark = Dark;
			if (Default is not null)
				binding.Default = Default;

			return binding;
		}
	}
}
