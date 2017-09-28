using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	internal abstract partial class BaseViewContainerRemote
	{
		public bool CheckOtherProperties(IApp app, BindableProperty formProperty, string query, out object prop)
		{
			bool found = false;
			prop = null;

			if (formProperty == View.IsEnabledProperty)
			{
				var view = app.Query((arg) => arg.Raw(query)).FirstOrDefault();
				found = view != null;
				prop = view.Enabled;
			}

			if (formProperty == Button.TextProperty)
			{
				var view = app.Query((arg) => arg.Raw(query)).FirstOrDefault();
				found = view != null;
				prop = view.Text;
			}

			return found;
		}
	}
}
