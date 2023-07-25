#nullable disable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="Type[@FullName='Microsoft.Maui.Controls.Element']/Docs/*" />
	public partial class Element : IStyleSelectable
	{
		IEnumerable<IStyleSelectable> IStyleSelectable.Children => LogicalChildrenInternal;

		IList<string> IStyleSelectable.Classes => null;

		string IStyleSelectable.Id => StyleId;

		string[] _styleSelectableNameAndBaseNames;
		string[] IStyleSelectable.NameAndBases
		{
			get
			{
				if (_styleSelectableNameAndBaseNames == null)
				{
					var list = new List<string>();
					var t = GetType();
					while (t != typeof(BindableObject))
					{
						list.Add(t.Name);
						t = t.BaseType;
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

		void ApplyStyleSheets(List<StyleSheet> sheets, IVisualTreeElement element)
		{
			for (var i = (sheets?.Count ?? 0) - 1; i >= 0; i--)
			{
				if (element is BindableObject bo)
					//FIXME: is it ok to ignore specificty here ?
					((IStyle)sheets[i]).Apply(bo, new SetterSpecificity());
			}

			foreach (var child in element.GetVisualChildren())
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