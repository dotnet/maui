using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestWindow : Window
	{
		// Because the relationship from Window => Application is a weakreference
		// we need to retain a reference to the Application so it doesn't get GC'd
		TestApp _app;
		public TestWindow()
		{

		}

		public TestWindow(Page page) : base(page)
		{
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == PageProperty.PropertyName &&
				Parent == null)
			{
				_app = new TestApp(this);
				_ = (_app as IApplication).CreateWindow(null);
			}
		}
	}

	public static class TestWindowExtensions
	{
		public static T AddToTestWindow<T>(this T page)
			where T : Page
		{
			return (T)new TestWindow(page).Page;
		}
	}
}
