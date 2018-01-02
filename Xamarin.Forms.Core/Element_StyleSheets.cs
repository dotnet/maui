using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Xamarin.Forms.Internals;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms
{
	public partial class Element : IStyleSelectable
	{
		IEnumerable<IStyleSelectable> IStyleSelectable.Children => LogicalChildrenInternal;

		IList<string> IStyleSelectable.Classes => null;

		string IStyleSelectable.Id => StyleId;

		string[] _styleSelectableNameAndBaseNames;
		string[] IStyleSelectable.NameAndBases {
			get {
				if (_styleSelectableNameAndBaseNames == null) {
					var list = new List<string>();
					var t = GetType();
					while (t != typeof(BindableObject)) {
						list.Add(t.Name);
						t = t.GetTypeInfo().BaseType;
					}
					_styleSelectableNameAndBaseNames = list.ToArray();
				}
				return _styleSelectableNameAndBaseNames;
			}
		}

		IStyleSelectable IStyleSelectable.Parent => Parent;
	}
}