using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SwitchCellRenderer : CellRenderer
	{
		readonly Dictionary<EvasObject, VisualElement> _cacheCandidate = new Dictionary<EvasObject, VisualElement>();

		protected SwitchCellRenderer(string style) : base(style)
		{
		}

		public SwitchCellRenderer() : this(ThemeManager.GetSwitchCellRendererStyle())
		{
			MainPart = this.GetMainPart();
			SwitchPart = this.GetSwitchPart();
		}

		protected string MainPart { get; set; }
		protected string SwitchPart { get; set; }

		protected override Span OnGetText(Cell cell, string part)
		{
			if (part == MainPart)
			{
				return new Span()
				{
					Text = (cell as SwitchCell).Text
				};
			}
			return null;
		}

		protected override EvasObject OnGetContent(Cell cell, string part)
		{
			if (part == SwitchPart)
			{
				var toggle = new Switch()
				{
					BindingContext = cell,
					Parent = cell.Parent
				};
				toggle.SetBinding(Switch.IsToggledProperty, new Binding(SwitchCell.OnProperty.PropertyName));
				toggle.SetBinding(Switch.OnColorProperty, new Binding(SwitchCell.OnColorProperty.PropertyName));
				var nativeView = Platform.GetOrCreateRenderer(toggle).NativeView;

				if (Device.Idiom == TargetIdiom.Watch)
				{
					nativeView.MinimumWidth += 8;
				}

				//It is a temporary way to prevent that the check of the Cell gets focus until the UX about views in the Cell for TV is defined.
				if (Device.Idiom == TargetIdiom.TV)
				{
					((Check)nativeView).AllowFocus(false);
				}
				else
				{
					nativeView.PropagateEvents = false;
				}

				_cacheCandidate[nativeView] = toggle;
				nativeView.Deleted += (sender, e) =>
				{
					_cacheCandidate.Remove(sender as EvasObject);
				};

				return nativeView;
			}
			return null;
		}

		protected override EvasObject OnReusableContent(Cell cell, string part, EvasObject old)
		{
			if (!_cacheCandidate.ContainsKey(old))
			{
				return null;
			}
			_cacheCandidate[old].BindingContext = cell;
			return old;
		}

		protected override bool OnCellPropertyChanged(Cell cell, string property, Dictionary<string, EvasObject> realizedView)
		{
			if (property == SwitchCell.TextProperty.PropertyName)
			{
				return true;
			}
			return base.OnCellPropertyChanged(cell, property, realizedView);
		}
	}
}
