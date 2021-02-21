using System.Collections.Generic;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[ContentProperty("Platforms")]
	public class OnPlatform<T>
	{
		public OnPlatform()
		{
			Platforms = new List<On>();
		}

		public IList<On> Platforms { get; private set; }

		bool hasDefault;
		T @default;
		public T Default
		{
			get { return @default; }
			set
			{
				hasDefault = true;
				@default = value;
			}
		}

#pragma warning disable RECS0108 // Warns about static fields in generic types
		static readonly IValueConverterProvider s_valueConverter = DependencyService.Get<IValueConverterProvider>();
#pragma warning restore RECS0108 // Warns about static fields in generic types

		public static implicit operator T(OnPlatform<T> onPlatform)
		{
			foreach (var onPlat in onPlatform.Platforms)
			{
				if (onPlat.Platform == null)
					continue;
				if (!onPlat.Platform.Contains(Device.RuntimePlatform))
					continue;
				if (s_valueConverter == null)
					continue;
				return (T)s_valueConverter.Convert(onPlat.Value, typeof(T), null, null);
			}

			return onPlatform.hasDefault ? onPlatform.@default : default(T);
		}
	}

	[ContentProperty("Value")]
	public class On
	{
		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> Platform { get; set; }
		public object Value { get; set; }
	}
}
