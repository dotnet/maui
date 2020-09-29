namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.Entry;

	public static class Entry
	{
		public static readonly BindableProperty ImeOptionsProperty = BindableProperty.Create(nameof(ImeOptions), typeof(ImeFlags), typeof(Entry), ImeFlags.Default);

		public static ImeFlags GetImeOptions(BindableObject element)
		{
			return (ImeFlags)element.GetValue(ImeOptionsProperty);
		}

		public static void SetImeOptions(BindableObject element, ImeFlags value)
		{
			element.SetValue(ImeOptionsProperty, value);
		}

		public static ImeFlags ImeOptions(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetImeOptions(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetImeOptions(this IPlatformElementConfiguration<Xamarin.Forms.PlatformConfiguration.Android, FormsElement> config, ImeFlags value)
		{
			SetImeOptions(config.Element, value);
			return config;
		}
	}

	public enum ImeFlags
	{
		Default = 0,
		None = 1,
		Go = 2,
		Search = 3,
		Send = 4,
		Next = 5,
		Done = 6,
		Previous = 7,
		ImeMaskAction = 255,
		NoPersonalizedLearning = 16777216,
		NoFullscreen = 33554432,
		NoExtractUi = 268435456,
		NoAccessoryAction = 536870912,
	}
}