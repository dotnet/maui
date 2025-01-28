#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Platforms")]
	public class OnPlatform<T> : IWrappedValue
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
			if (s_valueConverter != null)
			{
				On explicitDefault = null;

				foreach (var onPlat in onPlatform.Platforms)
				{
					if (onPlat.Platform == null)
						continue;
					if (onPlat.Platform.Contains("Default"))
					{
						explicitDefault = onPlat;
						continue;
					}
					if (!onPlat.Platform.Contains(DeviceInfo.Platform.ToString()))
						continue;
					return (T)s_valueConverter.Convert(onPlat.Value, typeof(T), null, null);
				}

				// fallback for UWP
				foreach (var onPlat in onPlatform.Platforms)
				{
					if (onPlat.Platform != null && onPlat.Platform.Contains("UWP") && DeviceInfo.Platform == DevicePlatform.WinUI)
						return (T)s_valueConverter.Convert(onPlat.Value, typeof(T), null, null);
				}

				// fallback for explicit default
				if (explicitDefault != null)
					return (T)s_valueConverter.Convert(explicitDefault.Value, typeof(T), null, null);
			}

			return onPlatform.hasDefault ? onPlatform.@default : default(T);
		}

		object IWrappedValue.Value => (T)this;
		System.Type IWrappedValue.ValueType => typeof(T);
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/On.xml" path="Type[@FullName='Microsoft.Maui.Controls.On']/Docs/*" />
	[ContentProperty("Value")]
	public class On
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/On.xml" path="//Member[@MemberName='Platform']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> Platform { get; set; }
		/// <include file="../../docs/Microsoft.Maui.Controls/On.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public object Value { get; set; }
	}
}
