using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class NavigationContentView : NView, INavigationContentView
	{
		NView? _titleView;
		NView? _content;

		public NavigationContentView() : base()
		{
			WidthSpecification = LayoutParamPolicies.MatchParent;
			HeightSpecification = LayoutParamPolicies.MatchParent;

			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Vertical
			};
		}

		public NView? TargetView => this;

		public NView? TitleView
		{
			get => _titleView;
			set
			{
				if (_titleView != null)
					Remove(_titleView);

				_titleView = value;

				if (_titleView != null)
				{
					Add(_titleView);
					(_titleView.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
					_titleView.RaiseToTop();
				}
			}
		}

		public NView? Content
		{
			get => _content;
			set
			{
				if (_content != null)
					Remove(_content);

				_content = value;

				if (_content != null)
				{
					_content.HeightSpecification = LayoutParamPolicies.MatchParent;
					_content.WidthSpecification = LayoutParamPolicies.MatchParent;
					Add(_content);
					if (_titleView != null)
						_content.LowerBelow(_titleView);
				}
			}
		}
	}
}
