using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Maui.StyleSheets;

namespace System.Maui
{
	public partial class Element : IStyleSelectable
	{
		IEnumerable<IStyleSelectable> IStyleSelectable.Children => LogicalChildrenInternal;

		IList<string> IStyleSelectable.Classes => null;

		string IStyleSelectable.Id => StyleId;

		internal string _cssFallbackTypeName;

		string[] _styleSelectableNameAndBaseNames;
		string[] IStyleSelectable.NameAndBases
		{
			get
			{
				if (_styleSelectableNameAndBaseNames == null)
				{
					var list = new List<string>();
					if (_cssFallbackTypeName != null)
						list.Add(_cssFallbackTypeName);
					var t = GetType();
					while (t != typeof(BindableObject))
					{
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
		internal void ApplyStyleSheets()
		{
			var sheets = new List<StyleSheet>();
			Element parent = this;
			while (parent != null)
			{
				var resourceProvider = parent as IResourcesProvider;
				var vpSheets = resourceProvider?.GetStyleSheets();
				if (vpSheets != null)
					sheets.AddRange(vpSheets);
				parent = parent.Parent;
			}

			ApplyStyleSheets(sheets, this);
		}

		void ApplyStyleSheets(List<StyleSheet> sheets, Element element)
		{
			if (element == null)
				return;

			for (var i = (sheets?.Count ?? 0) - 1; i >= 0; i--)
			{
				((IStyle)sheets[i]).Apply(element);
			}

			foreach (Element child in element.AllChildren)
			{
				var mergedSheets = sheets;
				var resourceProvider = child as IResourcesProvider;
				var childSheets = resourceProvider?.GetStyleSheets();
				if (childSheets?.Any() ?? false)
				{
					mergedSheets = new List<StyleSheet>(childSheets);
					mergedSheets.AddRange(sheets);
				}
				ApplyStyleSheets(mergedSheets, child);
			}
		}
	}
}