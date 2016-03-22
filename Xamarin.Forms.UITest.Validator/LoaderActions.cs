using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Loader;

namespace Xamarin.Forms.UITest.Validator
{
	internal class LoaderActions
	{
		readonly IEnumerable<TestType> _androidTestTypes;
		readonly IEnumerable<TestType> _iOsTestTypes;

		public LoaderActions()
		{
			var formsLoader = new FormsLoader();
			FormsTypes = formsLoader.FormsTypes();
			_iOsTestTypes = formsLoader.IOSTestTypes();
			_androidTestTypes = formsLoader.AndroidTestTypes();

			TypeiOsuiTestDictionary = BuildTypeUiTestDictionary(_iOsTestTypes);
			TypeAndroidUiTestDictionary = BuildTypeUiTestDictionary(_androidTestTypes);

			Debug.WriteLine("HI");
		}

		internal IEnumerable<FormsType> FormsTypes { get; }

		internal Dictionary<Type, List<FormsUiTest>> TypeiOsuiTestDictionary { get; }

		internal Dictionary<Type, List<FormsUiTest>> TypeAndroidUiTestDictionary { get; }

		Dictionary<Type, List<FormsUiTest>> BuildTypeUiTestDictionary(IEnumerable<TestType> testTypes)
		{
			var result = new Dictionary<Type, List<FormsUiTest>>();

			foreach (TestType testType in testTypes)
			{
				foreach (TestMember testMember in testType.Members())
				{
					IEnumerable<UiTestAttribute> testAttrs = testMember.UiTestAttributes();
					foreach (UiTestAttribute testAttr in testAttrs)
					{
						Type type = testAttr.Type;
						string memberName = testAttr.MemberName;
						string testName = testMember.MemberInfo.Name;

						if (!result.ContainsKey(type))
							result.Add(type, new List<FormsUiTest> { new FormsUiTest(memberName, testName) });
						else
							result[type].Add(new FormsUiTest(memberName, testName));
					}
				}
			}

			return result;
		}
	}
}