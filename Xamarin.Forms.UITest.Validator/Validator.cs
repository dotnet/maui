using System;
using System.IO;
using System.Linq;

namespace Xamarin.Forms.UITest.Validator
{
	//[UITest (typeof(Button), "Clicked")]
	internal class Validator
	{
		// TODO - Add support for muliple attributes (i.e. Fonts or Image.Aspect)
		// TODO - Add analysis of test duplication using duplicated tags
		// TODO - Support Single class analysis by passing a class name into the program
		// TODO - Stats of public surface area coverage

		static int Main(string[] args)
		{
			Console.WriteLine("Generating UITest code coverage");

			// Currently scanning for public events, public properties and public methods.

			var loaderActions = new LoaderActions();

			var domTypes =
				(from formsType in loaderActions.FormsTypes
					where formsType.Members().Count() > 1
					select
						new DomTypeModel(formsType, loaderActions.TypeiOsuiTestDictionary, loaderActions.TypeAndroidUiTestDictionary))
					.OrderByDescending(domType => domType.Rank);

			var typeMemberHtml = "";
			int numberOfTestedMembers = 0;
			int numberOfMembers = 0;

			foreach (var domType in domTypes)
			{
				numberOfTestedMembers += (from member in domType.Children where member.IsTested select member).Count();
				numberOfMembers += domType.Children.Count;
				typeMemberHtml += domType.Html();
			}

			var html =
				"<html>" +
				"<head>" +
				"<link rel=\"stylesheet\" type=\"text/css\" href=\"Css/styles.css\">" +
				"<script type=\"text/javascript\" src=\"https://code.jquery.com/jquery-2.1.3.min.js\"></script>" +
				"<script type=\"text/javascript\" src=\"Js/script.js\"></script>" +
				"</head>" +
				"<body>" +
				"<div id=\"totalStats\">" +
				string.Format("<div id=\"coverage\">{0:N2}%<span class=\"totalStatsLarge\"> Covered!!</span></div>",
					(numberOfTestedMembers / (double)numberOfMembers) * 100) +
				string.Format("<div class=\"coverageDetail\">{0}<span class=\"totalStatsSmall\"> Members</span></div>",
					numberOfMembers) +
				string.Format("<div class=\"coverageDetail\">{0}<span class=\"totalStatsSmall\"> Tested Members</span></div>",
					numberOfTestedMembers) +
				"</div>" +
				"<div id=\"container\">" +
				typeMemberHtml +
				"</div>" +
				"</body>" +
				"</html>";

			File.WriteAllText("../../UITestCoverage/index.html", html);

			return 0;
		}
	}
}