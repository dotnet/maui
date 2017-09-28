using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract partial class BaseViewContainerRemote
	{
		bool TryConvertViewScale<T>(BindableProperty formProperty, string query, out T result)
		{
			result = default(T);

			if (formProperty == View.ScaleProperty) {

				Tuple<string[], bool> property = formProperty.GetPlatformPropertyQuery();
				string[] propertyPath = property.Item1;

				var matrix = new Matrix ();
				matrix.M00 = App.Query (q => q.Raw (query).Invoke (propertyPath[0]).Value<float> ()).First ();
				matrix.M11 = App.Query (q => q.Raw (query).Invoke (propertyPath[1]).Value<float> ()).First ();
				matrix.M22 = 0.5f;
				matrix.M33 = 1.0f;
				result = (T)((object)matrix);
				return true;
			}

			return false;
		}

		string AccountForFastRenderers(string query)
		{
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

			return query;
		}
	}
}
