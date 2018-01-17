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

		//on parent set, or on parent stylesheet changed, reapply all
		void ApplyStyleSheetsOnParentSet()
		{
			var parent = Parent;
			if (parent == null)
				return;
			var sheets = new List<StyleSheet>();
			while (parent != null) {
				var resourceProvider = parent as IResourcesProvider;
				var vpSheets = resourceProvider?.GetStyleSheets();
				if (vpSheets != null)
					sheets.AddRange(vpSheets);
				parent = parent.Parent;
			}
			for (var i = sheets.Count - 1; i >= 0; i--)
				((IStyle)sheets[i]).Apply(this);
		}
	}
}