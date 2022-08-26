using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NLinearLayout = Tizen.NUI.LinearLayout;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui
{
	public class NaviPage : NView, IContainable<NView>
	{
		TitleView? _titleView;
		NView? _content;

		ObservableCollection<NView> _children = new ObservableCollection<NView>();

		public NaviPage()
		{
			HeightSpecification = LayoutParamPolicies.MatchParent;
			WidthSpecification = LayoutParamPolicies.MatchParent;

			Layout = new NLinearLayout
			{
				LinearOrientation = NLinearLayout.Orientation.Vertical
			};

			_children.CollectionChanged += OnCollectionChanged;
		}

		public TitleView? TitleView
		{
			get => _titleView;
			set
			{
				if (_titleView != null)
				{
					_titleView.Unparent();
					_titleView.Dispose();
					_titleView = null;
				}

				_titleView = value;

				if (_titleView != null)
				{
					Add(_titleView);
					(_titleView.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
				}
			}
		}

		public NView? Content
		{
			get => _content;
			set
			{
				if (_content != null)
				{
					_content.Unparent();
					_content.Dispose();
					_content = null;
				}
				_content = value;

				if (_content != null)
				{
					_content.HeightSpecification = LayoutParamPolicies.MatchParent;
					_content.WidthSpecification = LayoutParamPolicies.MatchParent;
					Add(_content);
				}
			}
		}

		IList<NView> IContainable<NView>.Children => _children;

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
			{
				Content = e.NewItems[0] as NView;
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
			{
				Content = null;
			}
		}
	}
}
