using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using TestUtils.Appium.UITests;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal abstract partial class BaseViewContainerRemote
	{
		protected IApp App { get; private set; }

		public string ViewQuery { get; private set; }

		public string ContainerDescendants { get; private set; }

		public string EventLabelQuery { get; set; }

		public string StateLabelQuery { get; private set; }

		public string StateButtonQuery { get; private set; }

		public string LayeredHiddenButtonQuery { get; private set; }

		public string LayeredLabelQuery { get; private set; }

		protected IUITestContext _uiTestContext;

		protected BaseViewContainerRemote(IUITestContext? testContext, Enum formsType)
			: this(testContext, formsType.ToString())
		{
		}

		protected BaseViewContainerRemote(IUITestContext? testContext, string formsType)
		{
			_uiTestContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
			App = testContext.App;

			ContainerDescendants = string.Format("* marked:'{0}Container' child *", formsType);
			ViewQuery = string.Format("* marked:'{0}VisualElement'", formsType);
			EventLabelQuery = string.Format("* marked:'{0}EventLabel'", formsType);
			StateLabelQuery = string.Format("* marked:'{0}StateLabel'", formsType);
			StateButtonQuery = string.Format("* marked:'{0}StateButton'", formsType);
			LayeredHiddenButtonQuery = string.Format("* marked:'{0}LayeredHiddenButton'", formsType);
			LayeredLabelQuery = string.Format("* marked:'{0}LayeredLabel'", formsType);
		}

		public AppResult GetView()
		{
			return App.Query(q => q.Raw(ViewQuery)).First();
		}

		public AppResult[] GetViews()
		{
			return App.Query(q => q.Raw(ViewQuery));
		}

		public virtual void GoTo([CallerMemberName] string callerMemberName = "")
		{
			App.WaitForElement("TargetViewContainer");
			App.Tap("TargetViewContainer");
			App.EnterText("TargetViewContainer", callerMemberName.Replace("_", "", StringComparison.OrdinalIgnoreCase) + "VisualElement");
			App.Tap("GoButton");
		}

		public void TapView()
		{
			App.Tap(q => q.Raw(ViewQuery));
		}

		public void TouchAndHoldView()
		{
			App.TouchAndHold(q => q.Raw(ViewQuery));
		}

		public AppResult[] GetContainerDescendants()
		{
			return App.Query(q => q.Raw(ContainerDescendants));
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
			Tuple<string[], bool> property = formProperty.GetPlatformPropertyQuery(_uiTestContext.TestConfig.TestDevice);
			string[] propertyPath = property.Item1;
			bool isOnParentRenderer = property.Item2;

			var query = UpdateQueryForParent(ViewQuery, isOnParentRenderer);

			App.WaitForElement(q => q.Raw(query));

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

		string UpdateQueryForParent(string query, bool isOnParentRenderer)
		{
			if (isOnParentRenderer)
			{
				query += " parent * index:0";
			}

			return query;
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

		static bool MaybeGetProperty<T>(IApp app, string query, string[] propertyPath, out object? result)
		{
			try
			{
				switch (propertyPath.Length)
				{
					case 1:
						result = app.Query(q => q.Raw(query).Invoke(propertyPath[0]).Value<T>()).First();
						break;
					case 2:
						result = app.Query(q => q.Raw(query).Invoke(propertyPath[0]).Invoke(propertyPath[1]).Value<T>()).First();
						break;
					case 3:
						result =
							app.Query(q => q.Raw(query).Invoke(propertyPath[0]).Invoke(propertyPath[1]).Invoke(propertyPath[2]).Value<T>()).First();
						break;
					case 4:
						result =
							app.Query(
								q =>
									q.Raw(query)
										.Invoke(propertyPath[0])
										.Invoke(propertyPath[1])
										.Invoke(propertyPath[2])
										.Invoke(propertyPath[3])
										.Value<T>()).First();
						break;
					default:
						result = null;
						return false;
				}
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