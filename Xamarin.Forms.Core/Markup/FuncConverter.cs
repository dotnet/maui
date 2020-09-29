using System;
using System.Globalization;
using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public class FuncConverter<TSource, TDest, TParam> : IValueConverter
	{
		readonly Func<TSource, TDest> convert;
		readonly Func<TDest, TSource> convertBack;

		readonly Func<TSource, TParam, TDest> convertWithParam;
		readonly Func<TDest, TParam, TSource> convertBackWithParam;

		readonly Func<TSource, TParam, CultureInfo, TDest> convertWithParamAndCulture;
		readonly Func<TDest, TParam, CultureInfo, TSource> convertBackWithParamAndCulture;

		public FuncConverter(Func<TSource, TParam, CultureInfo, TDest> convertWithParamAndCulture = null, Func<TDest, TParam, CultureInfo, TSource> convertBackWithParamAndCulture = null)
		{ VerifyExperimental(constructorHint: nameof(FuncConverter)); this.convertWithParamAndCulture = convertWithParamAndCulture; this.convertBackWithParamAndCulture = convertBackWithParamAndCulture; }

		public FuncConverter(Func<TSource, TParam, TDest> convertWithParam = null, Func<TDest, TParam, TSource> convertBackWithParam = null)
		{ VerifyExperimental(constructorHint: nameof(FuncConverter)); this.convertWithParam = convertWithParam; this.convertBackWithParam = convertBackWithParam; }

		public FuncConverter(Func<TSource, TDest> convert = null, Func<TDest, TSource> convertBack = null)
		{ VerifyExperimental(constructorHint: nameof(FuncConverter)); this.convert = convert; this.convertBack = convertBack; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (convert != null)
				return convert.Invoke(
					value != null ? (TSource)value : default(TSource));

			if (convertWithParam != null)
				return convertWithParam.Invoke(
					value != null ? (TSource)value : default(TSource),
					parameter != null ? (TParam)parameter : default(TParam));

			if (convertWithParamAndCulture != null)
				return convertWithParamAndCulture.Invoke(
					value != null ? (TSource)value : default(TSource),
					parameter != null ? (TParam)parameter : default(TParam),
					culture);

			return default(TDest);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (convertBack != null)
				return convertBack.Invoke(
					value != null ? (TDest)value : default(TDest));

			if (convertBackWithParam != null)
				return convertBackWithParam.Invoke(
					value != null ? (TDest)value : default(TDest),
					parameter != null ? (TParam)parameter : default(TParam));

			if (convertBackWithParamAndCulture != null)
				return convertBackWithParamAndCulture.Invoke(
					value != null ? (TDest)value : default(TDest),
					parameter != null ? (TParam)parameter : default(TParam),
					culture);

			return default(TSource);
		}
	}

	public class FuncConverter<TSource, TDest> : FuncConverter<TSource, TDest, object>
	{
		public FuncConverter(Func<TSource, TDest> convert = null, Func<TDest, TSource> convertBack = null)
			: base(convert, convertBack) { }
	}

	public class FuncConverter<TSource> : FuncConverter<TSource, object, object>
	{
		public FuncConverter(Func<TSource, object> convert = null, Func<object, TSource> convertBack = null)
			: base(convert, convertBack) { }
	}

	public class FuncConverter : FuncConverter<object, object, object>
	{
		public FuncConverter(Func<object, object> convert = null, Func<object, object> convertBack = null)
			: base(convert, convertBack) { }
	}

	public class ToStringConverter : FuncConverter<object, string>
	{
		public ToStringConverter(string format = "{0}")
			: base(o => string.Format(CultureInfo.InvariantCulture, format, o)) { }
	}

	public class NotConverter : FuncConverter<bool, bool>
	{
		static readonly Lazy<NotConverter> instance = new Lazy<NotConverter>(() => new NotConverter());
		public static NotConverter Instance => instance.Value;
		public NotConverter() : base(t => !t, t => !t) { }
	}
}