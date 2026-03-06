#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	public partial class Element : IStyleSelectable
	{
		/// <inheritdoc/>
		IEnumerable<IStyleSelectable> IStyleSelectable.Children => LogicalChildrenInternal;

		/// <inheritdoc/>
		IList<string> IStyleSelectable.Classes => null;

		/// <inheritdoc/>
		string IStyleSelectable.Id => StyleId;

		string[] _styleSelectableNameAndBaseNames;

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		IStyleSelectable IStyleSelectable.Parent => Parent;

		//on parent set, or on parent stylesheet changed, reapply all
		internal void ApplyStyleSheets()
		{
			var sheets = CollectStyleSheets();
			ApplyStyleSheets(sheets, this);
		}

		/// <summary>
		/// Re-evaluates @media conditions for all stylesheets in scope.
		/// Called when window size changes or theme changes.
		/// If any condition changed state, re-applies stylesheets.
		/// </summary>
		internal void EvaluateMediaQueriesAndReapply(double windowWidth, double windowHeight, AppTheme appTheme)
		{
			var sheets = CollectStyleSheets();
			bool anyChanged = false;
			foreach (var sheet in sheets)
			{
				if (sheet.EvaluateMediaQueries(windowWidth, windowHeight, appTheme))
					anyChanged = true;
			}
			if (anyChanged)
				ApplyStyleSheets(sheets, this);
		}

		List<StyleSheet> CollectStyleSheets()
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
			return sheets;
		}

		void ApplyStyleSheets(List<StyleSheet> sheets, IVisualTreeElement element)
		{
			// Merge CSS custom properties from all sheets (parent sheets first, child sheets override)
			Dictionary<string, string> mergedVariables = null;
			if (sheets != null)
			{
				for (var i = sheets.Count - 1; i >= 0; i--)
				{
					var vars = sheets[i].Variables;
					if (vars != null && vars.Count > 0)
					{
						if (mergedVariables == null)
							mergedVariables = new Dictionary<string, string>(StringComparer.Ordinal);
						foreach (var kvp in vars)
							mergedVariables[kvp.Key] = kvp.Value;
					}
				}
			}

			for (var i = (sheets?.Count ?? 0) - 1; i >= 0; i--)
			{
				if (element is Element el)
					sheets[i].Apply(el, mergedVariables);
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