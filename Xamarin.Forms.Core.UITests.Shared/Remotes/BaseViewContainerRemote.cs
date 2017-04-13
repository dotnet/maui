using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract class BaseViewContainerRemote
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
			var scrollBounds = App.Query(Queries.PageWithoutNavigationBar()).First().Rect;

			// Scroll using gutter to the right of view, avoid scrolling inside of WebView
			if (PlatformViewType == PlatformViews.WebView)
			{
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

			while (true)
			{
#if __MACOS__
				var result = App.Query(o => o.Raw(ViewQuery));
#else
				var result = App.Query (o => o.Raw(ContainerQuery));
#endif

				if (result.Any())
					break;
				App.Tap(o => o.Raw("* marked:'MoveNextButton'"));
			}

			//Assert.True (App.ScrollForElement (
			//	ContainerQuery, new Drag (scrollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium)
			//), "Failed to find element in: " + callerMemberName);

			App.Screenshot("Go to element");
		}

		public void TapView()
		{
			App.Tap(q => q.Raw(ViewQuery));
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

		public T GetProperty<T>(BindableProperty formProperty)
		{
#if __ANDROID__
			
#endif

			Tuple<string[], bool> property = formProperty.GetPlatformPropertyQuery();
			string[] propertyPath = property.Item1;
			bool isOnParentRenderer = property.Item2;

			var query = ViewQuery;
			if (isOnParentRenderer && 
				PlatformViewType != PlatformViews.BoxView && 
				PlatformViewType != PlatformViews.Frame)
			{

#if __ANDROID__
				// If we're testing the fast renderers, we don't want to check the parent control for
				// this property (despite `isOnParentRenderer` being true); if we're testing a legacy
				// renderer, then we *do* need to check the parent control for the property
				// So we query the control's parent and see if it's a Container (legacy); if so, 
				// we adjust the query to look at the parent of the current control
				var parent = App.Query(appQuery => appQuery.Raw(ViewQuery + " parent * index:0"));
				if (parent.Length > 0 && parent[0].Label.EndsWith(ContainerLabel))
				{
					query = query + " parent * index:0";
				}
#else
				query = query + " parent * index:0";
#endif
			}

			object prop = null;
			bool found = false;

			bool isEdgeCase = false;
#if __ANDROID__
			isEdgeCase = (formProperty == View.ScaleProperty);
#endif
			if (!isEdgeCase)
			{
				found =
					MaybeGetProperty<string>(App, query, propertyPath, out prop) ||
					MaybeGetProperty<float>(App, query, propertyPath, out prop) ||
					MaybeGetProperty<bool>(App, query, propertyPath, out prop) ||
					MaybeGetProperty<object>(App, query, propertyPath, out prop);
			}
#if __MACOS__
			if (!found)
			{

				if (formProperty == View.IsEnabledProperty)
				{
					var view = App.Query((arg) => arg.Raw(query)).FirstOrDefault();
					found = view != null;
					prop = view.Enabled;
				}

				if (formProperty == Button.TextProperty)
				{
					var view = App.Query((arg) => arg.Raw(query)).FirstOrDefault();
					found = view != null;
					prop = view.Text;
				}
			}
#endif

#if __ANDROID__
			if (formProperty == View.ScaleProperty) {
				var matrix = new Matrix ();
				matrix.M00 = App.Query (q => q.Raw (query).Invoke (propertyPath[0]).Value<float> ()).First ();
				matrix.M11 = App.Query (q => q.Raw (query).Invoke (propertyPath[1]).Value<float> ()).First ();
				matrix.M22 = 0.5f;
				matrix.M33 = 1.0f;
				return (T)((object)matrix);
			}
#endif

			if (!found || prop == null)
			{
				throw new NullReferenceException("null property");
			}

			if (prop.GetType() == typeof(T))
				return (T)prop;

			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Matrix))
			{
				Matrix matrix = ParsingUtils.ParseCATransform3D((string)prop);
				return (T)((object)matrix);
			}

			if (typeof(T) == typeof(Color))
			{
#if __IOS__
				Color color = ParsingUtils.ParseUIColor((string)prop);
				return (T)((object)color);
#else
				uint intColor = (uint)((float)prop);
				Color color = Color.FromUint (intColor);
				return (T)((object)color);
#endif
			}

#if __IOS__
			if (prop.GetType() == typeof(string) && typeof(T) == typeof(Font))
			{
				Font font = ParsingUtils.ParseUIFont((string)prop);
				return (T)((object)font);
			}
#endif

			T result = default(T);

			var stringToBoolConverter = new StringToBoolConverter();
			var floatToBoolConverter = new FloatToBoolConverter();

			if (stringToBoolConverter.CanConvertTo(prop, typeof(bool)))
			{
				result = (T)stringToBoolConverter.ConvertTo(prop, typeof(bool));
			}
			else if (floatToBoolConverter.CanConvertTo(prop, typeof(bool)))
			{
				result = (T)floatToBoolConverter.ConvertTo(prop, typeof(bool));
			}

			return result;
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