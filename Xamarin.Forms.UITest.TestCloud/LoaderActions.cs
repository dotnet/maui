using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Loader;

namespace Xamarin.Forms.UITest.TestCloud
{
	internal class LoaderActions
	{
		readonly IEnumerable<string> _testCategories;

		public LoaderActions()
		{
			var formsLoader = new FormsLoader();

			var categoriesOnTypes =
				from type in formsLoader.IOSTestTypes()
				from categoryAttribute in type.Categories()
				select categoryAttribute.Name;

			var categoriesOnMembers =
				from type in formsLoader.IOSTestTypes()
				from members in type.Members()
				from categoryAttribute in members.CategoryAttributes()
				select categoryAttribute.Name;

			_testCategories = categoriesOnTypes.Union(categoriesOnMembers);
		}

		public void ListCategories()
		{
			foreach (string category in _testCategories)
				Console.WriteLine(category);
		}

		public bool ValidateCategory(string category)
		{
			return _testCategories.Any(k => k == category) || category == "All";
		}
	}
}