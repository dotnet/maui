using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		bool useLegacyFallback;
		T android;
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public T Android
		{
			get { return android; }
			set
			{
				useLegacyFallback = true;
				android = value;
			}
		}

		T ios;
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public T iOS
		{
			get { return ios; }
			set
			{
				useLegacyFallback = true;
				ios = value;
			}
		}

		T winPhone;
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public T WinPhone
		{
			get { return winPhone; }
			set
			{
				useLegacyFallback = true;
				winPhone = value;
			}
		}

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

		public IList<On> Platforms { get; private set; }

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

			if (!onPlatform.useLegacyFallback)
				return onPlatform.hasDefault ? onPlatform.@default : default(T);

			//legacy fallback
#pragma warning disable 0618, 0612
			return Device.OnPlatform(iOS: onPlatform.iOS, Android: onPlatform.Android, WinPhone: onPlatform.WinPhone);
#pragma warning restore 0618, 0612
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