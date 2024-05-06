using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal abstract partial class BaseViewContainerRemote
	{
		protected IApp App { get; private set; }

		public string ViewQuery { get; private set; }

		public string EventLabelQuery { get; set; }

		public string StateLabelQuery { get; private set; }

		public string StateButtonQuery { get; private set; }

		public string LayeredHiddenButtonQuery { get; private set; }

		public string LayeredLabelQuery { get; private set; }

		protected IUIClientContext _uiTestContext;

		protected BaseViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: this(testContext, formsType.ToString())
		{
		}

		protected BaseViewContainerRemote(IUIClientContext? testContext, string formsType)
		{
			_uiTestContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
			App = testContext.App;

			ViewQuery = string.Format("{0}VisualElement", formsType);
			EventLabelQuery = string.Format("{0}EventLabel", formsType);
			StateLabelQuery = string.Format("{0}StateLabel", formsType);
			StateButtonQuery = string.Format("{0}StateButton", formsType);
			LayeredHiddenButtonQuery = string.Format("{0}LayeredHiddenButton", formsType);
			LayeredLabelQuery = string.Format("{0}LayeredLabel", formsType);
		}

		public IUIElement GetView()
		{
			return App.FindElement(ViewQuery);
		}

		public IReadOnlyCollection<IUIElement> GetViews()
		{
			return App.FindElements(ViewQuery);
		}

		public virtual void GoTo([CallerMemberName] string callerMemberName = "")
		{
			App.WaitForElement("TargetViewContainer");
			App.Click("TargetViewContainer");
			App.EnterText("TargetViewContainer", callerMemberName.Replace("_", "", StringComparison.OrdinalIgnoreCase) + "VisualElement");
			App.Click("GoButton");
		}

		public void TapView()
		{
			App.Click(ViewQuery);
		}

		public T? GetProperty<T>(BindableProperty formProperty)
		{
			T? returnValue = GetPropertyFromBindableProperty<T>(formProperty);
			int loopCount = 0;
			while (loopCount < 5)
			{
				Thread.Sleep(100);
				T? newValue = GetPropertyFromBindableProperty<T>(formProperty);

				if (newValue == null || newValue.Equals(returnValue))
					break;
				else
					returnValue = newValue;

				loopCount++;
			}

			return returnValue;
		}

		T? GetPropertyFromBindableProperty<T>(BindableProperty formProperty)
		{
			Tuple<string[], bool> property = formProperty.GetPlatformPropertyQuery(_uiTestContext.Config.GetProperty<TestDevice>("TestDevice"));
			string[] propertyPath = property.Item1;

			var query = ViewQuery;

			App.WaitForElement(query);

			bool found = MaybeGetProperty<string>(App, query, propertyPath, out var prop) ||
						 MaybeGetProperty<float>(App, query, propertyPath, out prop) ||
						 MaybeGetProperty<bool>(App, query, propertyPath, out prop) ||
						 MaybeGetProperty<object>(App, query, propertyPath, out prop);

			if (!found || prop == null)
			{
				throw new NullReferenceException("null property");
			}

			if (prop.GetType() == typeof(T))
			{
				return (T)prop;
			}

			if (TryConvertMatrix(prop, out T? result) ||
				TryConvertColor(prop, out result) ||
				TryConvertBool(prop, out result))
			{
				return result;
			}

			return result;
		}

		static bool TryConvertMatrix<T>(object prop, out T? result)
		{
			result = default;

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Matrix))
			{
				Matrix matrix = ParsingUtils.ParseCATransform3D((string)prop);
				result = (T)(object)matrix;
				return true;
			}

			return false;
		}

		static bool TryConvertColor<T>(object prop, out T? result)
		{
			result = default;

			if (typeof(T) == typeof(Color))
			{
				uint intColor = (uint)((float)prop);
				Color color = Color.FromUint(intColor);
				result = (T)((object)color);
				return true;
			}

			return false;
		}

		static bool TryConvertBool<T>(object prop, out T? result)
		{
			result = default;

			var stringToBoolConverter = new StringToBoolConverter();
			var floatToBoolConverter = new FloatToBoolConverter();

			if (stringToBoolConverter.CanConvertTo(prop, typeof(bool)))
			{
				result = (T)stringToBoolConverter.ConvertTo(prop, typeof(bool));
				return true;
			}

			if (floatToBoolConverter.CanConvertTo(prop, typeof(bool)))
			{
				result = (T)floatToBoolConverter.ConvertTo(prop, typeof(bool));
				return true;
			}

			return false;
		}

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{ "getAlpha", "Opacity" },
			{ "isEnabled", "IsEnabled" },
		};

		bool MaybeGetProperty<T>(IApp app, string query, string[] propertyPath, out object? result)
		{
			string attribute = _translatePropertyAccessor.TryGetValue(propertyPath[0], out var value) ? value : propertyPath[0];

			try
			{
				result = app.FindElement(query).GetAttribute<T>(attribute);
			}
			catch
			{
				result = null;
				return false;
			}

			return true;
		}
	}
}