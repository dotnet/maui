﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestWindow : Window
	{
		TestApp _testApp;
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
				_testApp = new TestApp(this);
				_ = (_testApp as IApplication).CreateWindow(null);
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
