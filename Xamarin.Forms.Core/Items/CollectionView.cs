using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CollectionViewRenderer))]
	public class CollectionView : ItemsView
	{
		internal const string CollectionViewExperimental = "CollectionView_Experimental";

		public CollectionView()
		{
			VerifyCollectionViewFlagEnabled(constructorHint: nameof(CollectionView));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void VerifyCollectionViewFlagEnabled(
			string constructorHint = null,
			[CallerMemberName] string memberName = "")
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(CollectionView), ExperimentalFlags.CollectionViewExperimental);
		}
	}
}
