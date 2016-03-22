using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Loader;

namespace Xamarin.Forms.UITest.Validator
{
	internal class DomTypeModel
	{
		readonly List<string> _androidTypeTests;
		readonly List<string> _iosTypeTests;
		public string TypeName;

		public DomTypeModel(FormsType formsType, Dictionary<Type, List<FormsUiTest>> typeiOsuiTestDictionary,
			Dictionary<Type, List<FormsUiTest>> typeAndroidUiTestDictionary)
		{
			TypeName = formsType.Type.Name;

			var iOsTests = new List<FormsUiTest>();
			var androidTests = new List<FormsUiTest>();

			_iosTypeTests = new List<string>();
			_androidTypeTests = new List<string>();

			Rank = 0;

			if (typeiOsuiTestDictionary.ContainsKey(formsType.Type))
			{
				iOsTests =
					(from test in typeiOsuiTestDictionary[formsType.Type]
						select test).ToList();
			}

			if (typeAndroidUiTestDictionary.ContainsKey(formsType.Type))
			{
				androidTests =
					(from test in typeAndroidUiTestDictionary[formsType.Type]
						select test).ToList();
			}

			_iosTypeTests =
				(from test in iOsTests
					where test.MemberName == ""
					select test.TestName).ToList();

			_androidTypeTests =
				(from test in androidTests
					where test.MemberName == ""
					select test.TestName).ToList();

			Rank -= _iosTypeTests.Count;
			Rank -= _androidTypeTests.Count;

			Children = new List<DomMemberModel>();

			foreach (var formsMember in formsType.Members())
			{
				var iOsMemberTests =
					(from test in iOsTests
						where test.MemberName == formsMember.MemberInfo.Name
						select test.TestName).ToList();

				Rank -= iOsMemberTests.Count;

				var androidMemberTests =
					(from test in androidTests
						where test.MemberName == formsMember.MemberInfo.Name
						select test.TestName).ToList();

				Rank -= androidMemberTests.Count;

				Children.Add(new DomMemberModel(formsMember.MemberInfo.Name, iOsMemberTests, androidMemberTests));
			}

			Rank += Children.Count;
		}

		public List<DomMemberModel> Children { get; }

		public int Rank { get; }

		public string Html()
		{
			var html =
				"<div class=\"type\" id=\"" + TypeName + "\">" +
				"<div class=\"stats\">" +
				"<h2>" + TypeName + "</h2>" +
				"<h3>Number of tests: " + TotalTests() + "</h3>" +
				string.Format("<h3>{0:N2}%</h3>", PercentageTested() * 100) +
				"<div class=\"progressBar\">" +
				"<div class=\"testedBox\" style=\"width:" + 290 * PercentageTested() + "px\"></div>" +
				"</div>" +
				"</div>" +
				"<div class=\"platform\">" +
				"<div class=\"ios\">iOS</div>" +
				"<div class=\"android\">Android</div>" +
				"</div>";

			html +=
				"<div class=\"testsForType\">" +
				"<div class=\"platformTestContainer\">";
			html += "<div class=\"iosColumn\">";
			foreach (var iosTest in _iosTypeTests)
				html += "<div class=\"test ios\">" + iosTest + "</div>";
			html += "</div>";
			html += "<div class=\"androidColumn\">";
			foreach (var androidTest in _androidTypeTests)
				html += "<div class=\"test android\">" + androidTest + "</div>";
			html += "</div>" +
			        "</div>" +
			        "</div>";

			html += "<div class=\"members\">";
			foreach (var child in Children)
			{
				html += "<div class=\"member\">";
				html += child.Html();
				html += "</div>";
			}

			html += "</div>";

			html += "</div>";
			return html;
		}

		int TotalTests()
		{
			int result = _iosTypeTests.Count + _androidTypeTests.Count;
			foreach (var child in Children)
				result += child.NumberOfiOsTests + child.NumberOfAndroidTests;
			return result;
		}

		int NumberOfTestedMembers()
		{
			int result = 0;
			foreach (var child in Children)
			{
				if (child.IsTested)
					result += 1;
			}
			return result;
		}

		double PercentageTested()
		{
			if (Children.Count > 0)
				return (NumberOfTestedMembers() / (double)Children.Count);

			return 0.0;
		}
	}
}