using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NAbsoluteLayout = Tizen.NUI.AbsoluteLayout;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class NavigationView : NView, INavigationView
	{
		NView? _header;
		NView? _content;
		NView? _footer;

		public NavigationView() : base()
		{
			WidthSpecification = LayoutParamPolicies.MatchParent;
			HeightSpecification = LayoutParamPolicies.MatchParent;

			Layout = new NavigationViewLayout
			{
				LayoutRequest = () => LayoutUpdated(),
			};
		}

		public NView? TargetView => this;

		public NView? Header
		{
			get => _header;
			set
			{
				RemoveView(_header);
				_header = value;
				AddView(_header);
			}
		}

		public NView? Footer
		{
			get => _footer;
			set
			{
				RemoveView(_footer);
				_footer = value;
				AddView(_footer);
			}
		}

		public NView? Content
		{
			get => _content;
			set
			{
				RemoveView(_content);

				_content = value;

				if (_content != null)
				{
					_content.WidthSpecification = LayoutParamPolicies.MatchParent;
					_content.HeightSpecification = LayoutParamPolicies.MatchParent;
					Add(_content);
				}
			}
		}

		void RemoveView(NView? view)
		{
			if (view != null)
				base.Remove(view);
		}

		void AddView(NView? view)
		{
			if (view != null)
			{
				view.WidthSpecification = LayoutParamPolicies.MatchParent;
				view.HeightSpecification = LayoutParamPolicies.WrapContent;
				Add(view);
			}
		}

		void LayoutUpdated()
		{
			var x = Position.X;
			var y = Position.Y;
			_header?.UpdatePosition(new Point(x, Position.Y));
			_content?.UpdatePosition(new Point(x, (_header != null) ? y + _header.Size.Height : y));
			_footer?.UpdatePosition(new Point(x, Size.Height - _footer.Size.Height));
		}

		class NavigationViewLayout : NAbsoluteLayout
		{
			public Action? LayoutRequest { get; set; }

			protected override void OnLayout(bool changed, LayoutLength left, LayoutLength top, LayoutLength right, LayoutLength bottom)
			{
				LayoutRequest?.Invoke();
				base.OnLayout(changed, left, top, right, bottom);
			}
		}
	}
}
