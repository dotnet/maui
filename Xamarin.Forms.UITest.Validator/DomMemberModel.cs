using System.Collections.Generic;

namespace Xamarin.Forms.UITest.Validator
{
	internal class DomMemberModel
	{
		readonly List<string> _androidtests;
		readonly List<string> _iOStests;
		readonly string _name;

		public DomMemberModel(string memberName, List<string> iOsTestsForMember, List<string> androidTestsForMember)
		{
			_name = memberName;
			_iOStests = iOsTestsForMember;
			var isiOsTested = iOsTestsForMember.Count != 0;
			NumberOfiOsTests = iOsTestsForMember.Count;

			_androidtests = androidTestsForMember;
			var isAndroidTested = androidTestsForMember.Count != 0;
			NumberOfAndroidTests = androidTestsForMember.Count;

			IsTested = isiOsTested && isAndroidTested;
		}

		public bool IsTested { get; }

		public int NumberOfiOsTests { get; }

		public int NumberOfAndroidTests { get; }

		public string Html()
		{
			var html = "";

			if (IsTested)
				html += "<div class=\"testedMember\">" + _name + "</div>";
			else
				html += "<div class=\"unTestedMember\">" + _name + "</div>";

			html += "<div class=\"platformTestContainer\">";
			html += "<div class=\"iosColumn\">";
			foreach (var test in _iOStests)
				html += "<div class=\"test ios\">" + test + "</div>";
			html += "</div>";

			html += "<div class=\"androidColumn\">";
			foreach (var test in _androidtests)
				html += "<div class=\"test android\">" + test + "</div>";
			html += "</div>";

			html += "</div>";

			return html;
		}
	}
}