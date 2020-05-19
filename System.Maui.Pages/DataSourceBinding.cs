using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Xamarin.Forms.Pages
{
	public class DataSourceBinding : BindingBase
	{
		internal const string SelfPath = ".";
		IValueConverter _converter;
		object _converterParameter;
		WeakReference _dataSourceRef;

		BindingExpression _expression;
		string _path;

		public DataSourceBinding()
		{
		}

		public DataSourceBinding(string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null, object converterParameter = null, string stringFormat = null)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("path can not be an empty string", nameof(path));

			AllowChaining = true;
			Path = path;
			Converter = converter;
			ConverterParameter = converterParameter;
			Mode = mode;
			StringFormat = stringFormat;
		}

		public IValueConverter Converter
		{
			get { return _converter; }
			set
			{
				ThrowIfApplied();

				_converter = value;
			}
		}

		public object ConverterParameter
		{
			get { return _converterParameter; }
			set
			{
				ThrowIfApplied();

				_converterParameter = value;
			}
		}

		public string Path
		{
			get { return _path; }
			set
			{
				ThrowIfApplied();

				_path = value;
				_expression = GetBindingExpression($"DataSource[{value}]");
			}
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);

			if (_expression == null)
				_expression = new BindingExpression(this, SelfPath);

			_expression.Apply(fromTarget);
		}

		internal override async void Apply(object newContext, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged)
		{
			var view = bindObj as VisualElement;
			if (view == null)
				throw new InvalidOperationException();

			base.Apply(newContext, bindObj, targetProperty, fromBindingContextChanged: fromBindingContextChanged);

			Element dataSourceParent = await FindDataSourceParentAsync(view);

			var dataSourceProviderer = (IDataSourceProvider)dataSourceParent;
			if (dataSourceProviderer != null)
				_dataSourceRef = new WeakReference(dataSourceProviderer);

			dataSourceProviderer?.MaskKey(_path);
			ApplyInner(dataSourceParent, bindObj, targetProperty);
		}

		internal override BindingBase Clone()
		{
			return new DataSourceBinding(Path, Mode) { Converter = Converter, ConverterParameter = ConverterParameter, StringFormat = StringFormat };
		}

		internal override object GetSourceValue(object value, Type targetPropertyType)
		{
			if (Converter != null)
				value = Converter.Convert(value, targetPropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			return base.GetSourceValue(value, targetPropertyType);
		}

		internal override object GetTargetValue(object value, Type sourcePropertyType)
		{
			if (Converter != null)
				value = Converter.ConvertBack(value, sourcePropertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			return base.GetTargetValue(value, sourcePropertyType);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			base.Unapply(fromBindingContextChanged: fromBindingContextChanged);

			var dataSourceProviderer = (IDataSourceProvider)_dataSourceRef?.Target;
			dataSourceProviderer?.UnmaskKey(_path);

			_expression?.Unapply();
		}

		void ApplyInner(Element templatedParent, BindableObject bindableObject, BindableProperty targetProperty)
		{
			if (_expression == null && templatedParent != null)
				_expression = new BindingExpression(this, SelfPath);

			_expression?.Apply(templatedParent, bindableObject, targetProperty);
		}

		static async Task<Element> FindDataSourceParentAsync(Element element)
		{
			while (!Application.IsApplicationOrNull(element))
			{
				if (element is IDataSourceProvider)
					return element;
				element = await TemplateUtilities.GetRealParentAsync(element);
			}

			return null;
		}

		BindingExpression GetBindingExpression(string path)
		{
			return new BindingExpression(this, !string.IsNullOrWhiteSpace(path) ? path : SelfPath);
		}
	}
}