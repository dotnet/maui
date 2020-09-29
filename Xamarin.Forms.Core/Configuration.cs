
namespace Xamarin.Forms
{
	public class Configuration<TPlatform, TElement> : IPlatformElementConfiguration<TPlatform, TElement>
			where TPlatform : IConfigPlatform
			where TElement : Element

	{
		public Configuration(TElement element)
		{
			Element = element;
		}

		public TElement Element { get; }

		public static Configuration<TPlatform, TElement> Create(TElement element)
		{
			return new Configuration<TPlatform, TElement>(element);
		}
	}
}