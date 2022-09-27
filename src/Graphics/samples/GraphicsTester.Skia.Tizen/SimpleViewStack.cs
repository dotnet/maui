using System.Collections.Generic;
using System.Linq;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace GraphicsTester.Skia.Tizen
{
	public class SimpleViewStack : View
	{
		View _lastTop;

		public SimpleViewStack()
		{
			Layout = new AbsoluteLayout();
			WidthSpecification = LayoutParamPolicies.MatchParent;
			HeightSpecification = LayoutParamPolicies.MatchParent;
			InternalStack = new List<View>();
		}

		List<View> InternalStack { get; set; }

		public IReadOnlyList<View> Stack => InternalStack;

		public void Push(View view)
		{
			view.WidthSpecification = LayoutParamPolicies.MatchParent;
			view.HeightSpecification = LayoutParamPolicies.MatchParent;

			InternalStack.Add(view);
			Add(view);
			UpdateTopView();
		}

		public void Pop()
		{
			if (_lastTop != null)
			{
				var tobeRemoved = _lastTop;
				InternalStack.Remove(tobeRemoved);
				Remove(tobeRemoved);
				UpdateTopView();
				// if Pop was called by removed page,
				// Unrealize cause deletation of NativeCallback, it could be a cause of crash
				tobeRemoved.Dispose();
			}
		}

		public void PopToRoot()
		{
			while (InternalStack.Count > 1)
			{
				Pop();
			}
		}

		public void Insert(View before, View view)
		{
			view.Hide();
			var idx = InternalStack.IndexOf(before);
			InternalStack.Insert(idx, view);
			Add(view);
			UpdateTopView();
		}

		private void UpdateTopView()
		{
			if (_lastTop != InternalStack.LastOrDefault())
			{
				_lastTop?.Hide();
				_lastTop = InternalStack.LastOrDefault();
				_lastTop.Show();
			}
		}
	}
}
