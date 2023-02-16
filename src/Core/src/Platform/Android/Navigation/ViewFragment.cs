using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Platform
{
	class ViewFragment : Fragment
	{
		readonly View _aView;

		/// <summary>
		/// Do not pass an inflated view into this fragment. This fragment should only be used for very simple views that won't have any
		/// internal need to use ChildSupportFragmentManager or LayoutInflater.
		/// </summary>
		/// <param name="aView"></param>
		public ViewFragment(View aView)
		{
			_aView = aView;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
		{
			return _aView;
		}
	}
}
