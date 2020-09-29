using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract partial class BaseViewContainerRemote
	{
		//bool requiresDismissal;
		protected IApp App { get; private set; }

		public string ViewQuery { get; private set; }

		public string PlatformViewType { get; set; }

		public string ContainerQuery { get; private set; }

		public string ContainerLabel { get; private set; }

		public string ContainerDescendents { get; private set; }

		public string EventLabelQuery { get; set; }

		public string StateLabelQuery { get; private set; }

		public string StateButtonQuery { get; private set; }

		public string LayeredHiddenButtonQuery { get; private set; }

		public string LayeredLabelQuery { get; private set; }

		protected BaseViewContainerRemote(IApp app, Enum formsType, string platformViewType)
		{
			App = app;
			PlatformViewType = platformViewType;

			// Currently tests are failing because the ViewInitilized is setting the renderer and control, fix and then remove index one

			ContainerQuery = string.Format("* marked:'{0}Container'", formsType);
			ContainerLabel = string.Format("{0}VisualElement_Container", formsType);
			ContainerDescendents = string.Format("* marked:'{0}Container' child *", formsType);

			ViewQuery = string.Format("* marked:'{0}VisualElement'", formsType);

			EventLabelQuery = string.Format("* marked:'{0}EventLabel'", formsType);
			StateLabelQuery = string.Format("* marked:'{0}StateLabel'", formsType);
			StateButtonQuery = string.Format("* marked:'{0}StateButton'", formsType);
			LayeredHiddenButtonQuery = string.Format("* marked:'{0}LayeredHiddenButton'", formsType);
			LayeredLabelQuery = string.Format("* marked:'{0}LayeredLabel'", formsType);

			if (platformViewType == PlatformViews.DatePicker)
			{
				//requiresDismissal = true;
			}
		}

		public virtual void GoTo([CallerMemberName] string callerMemberName = "")
		{
			// Scroll using gutter to the right of view, avoid scrolling inside of WebView
			if (PlatformViewType == PlatformViews.WebView)
			{
				var scrollBounds = App.Query(Queries.PageWithoutNavigationBar()).First().Rect;

				scrollBounds = new AppRect
				{
					X = scrollBounds.Width - 20,
					CenterX = scrollBounds.Width - 10,
					Y = scrollBounds.Y,
					CenterY = scrollBounds.CenterY,
					Width = 20,
					Height = scrollBounds.Height,
				};
			}

			App.WaitForElement("TargetViewContainer");
			App.Tap("TargetViewContainer");
			App.EnterText(callerMemberName.Replace("_", "") + "VisualElement");
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

		public void DismissPopOver()
		{
			App.Screenshot("About to dismiss pop over");
			App.Tap(q => q.Button("Done"));
			App.Screenshot("Pop over dismissed");
		}

		public AppResult GetView()
		{
			return App.Query(q => q.Raw(ViewQuery)).First();
		}

		public AppResult[] GetViews()
		{
			return App.Query(q => q.Raw(ViewQuery));
		}

		public AppResult[] GetContainerDescendants()
		{
			return App.Query(q => q.Raw(ContainerDescendents));
		}

		string UpdateQueryForParent(string query, bool isOnParentRenderer)
		{
			if (isOnParentRenderer &&
				PlatformViewType != PlatformViews.BoxView &&
				PlatformViewType != PlatformViews.Frame)
			{

#if __ANDROID__
				query = AccountForFastRenderers(query);
#else
				query = query + " parent * index:0";
#endif
			}

			return query;
		}

		public T GetProperty<T>(BindableProperty formProperty)
		{
			T returnValue = GetPropertyFromBindableProperty<T>(formProperty);
			int loopCount = 0;
			while (loopCount < 5)
			{
				Thread.Sleep(100);
				T newValue = GetPropertyFromBindableProperty<T>(formProperty);

				if (newValue.Equals(returnValue))
					break;
				else
					returnValue = newValue;

				loopCount++;
			}

			return returnValue;
		}

		T GetPropertyFromBindableProperty<T>(BindableProperty formProperty)
		{
			Tuple<string[], bool> property = formProperty.GetPlatformPropertyQuery();
			string[] propertyPath = property.Item1;
			bool isOnParentRenderer = property.Item2;

			var query = UpdateQueryForParent(ViewQuery, isOnParentRenderer);

			object prop;
			T result;

#if __ANDROID__
			if (TryConvertViewScale(formProperty, query, out result))
			{
				return result;
			}
#endif

			App.WaitForElement(q => q.Raw(query));

			bool found = MaybeGetProperty<string>(App, query, propertyPath, out prop) ||
						MaybeGetProperty<float>(App, query, propertyPath, out prop) ||
						MaybeGetProperty<bool>(App, query, propertyPath, out prop) ||
						MaybeGetProperty<object>(App, query, propertyPath, out prop);

#if __MACOS__
			if (!found)
			{
				found = CheckOtherProperties(App, formProperty, query, out prop);
			}
#endif

			if (!found || prop == null)
			{
				throw new NullReferenceException("null property");
			}

			if (prop.GetType() == typeof(T))
				return (T)prop;

			if (TryConvertMatrix(prop, out result))
			{
				return result;
			}

			if (TryConvertColor(prop, out result))
			{
				return result;
			}

#if __IOS__

			if (TryConvertFont(prop, out result))
			{
				return result;
			}
#endif

			if (TryConvertBool(prop, out result))
			{
				return result;
			}

			return result;
		}

		static bool TryConvertMatrix<T>(object prop, out T result)
		{
			result = default(T);

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Matrix))
			{
				Matrix matrix = ParsingUtils.ParseCATransform3D((string)prop);
				result = (T)((object)matrix);
				return true;
			}

			return false;
		}

		static bool TryConvertColor<T>(object prop, out T result)
		{
			result = default(T);

			if (typeof(T) == typeof(Color))
			{
#if __IOS__
				Color color = ParsingUtils.ParseUIColor((string)prop);
				result = (T)((object)color);
				return true;
#else
				uint intColor = (uint)((float)prop);
				Color color = Color.FromUint(intColor);
				result = (T)((object)color);
				return true;
#endif
			}

			return false;
		}

		static bool TryConvertBool<T>(object prop, out T result)
		{
			result = default(T);

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

		static bool MaybeGetProperty<T>(IApp app, string query, string[] propertyPath, out object result)
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
							app.Query(q => q.Raw(query).Invoke(propertyPath[0]).Invoke(propertyPath[1]).Invoke(propertyPath[2]).Value<T>())
								.First();
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

	internal class StringToBoolConverter : TypeConverter
	{
		public override bool CanConvertTo(object source, Type targetType)
		{
			if (targetType != typeof(bool) || !(source is string))
				return false;

			var str = (string)source;
			str = str.ToLowerInvariant();

			switch (str)
			{
				case "0":
				case "1":
				case "false":
				case "true":
					return true;
				default:
					return false;
			}
		}

		public override object ConvertTo(object source, Type targetType)
		{
			var str = (string)source;
			str = str.ToLowerInvariant();

			switch (str)
			{
				case "1":
				case "true":
					return true;
				default:
					return false;
			}
		}
	}

	internal class FloatToBoolConverter : TypeConverter
	{
		public override bool CanConvertTo(object source, Type targetType)
		{
			if (targetType != typeof(bool) || !(source is float))
				return false;

			var flt = (float)source;
			var epsilon = 0.0001;
			if (Math.Abs(flt - 1.0f) < epsilon || Math.Abs(flt - 0.0f) < epsilon)
				return true;
			else
				return false;
		}

		public override object ConvertTo(object source, Type targetType)
		{
			var flt = (float)source;
			var epsilon = 0.0001;
			if (Math.Abs(flt - 1.0f) < epsilon)
				return true;
			else
				return false;
		}
	}

	internal abstract class TypeConverter
	{
		public abstract bool CanConvertTo(object source, Type targetType);

		public abstract object ConvertTo(object source, Type targetType);
	}

	internal static class PlatformMethods
	{
		public static Tuple<string[], bool> GetPlatformPropertyQuery(this BindableProperty bindableProperty)
		{
			return PlatformMethodQueries.PropertyPlatformMethodDictionary[bindableProperty];
		}
	}
}