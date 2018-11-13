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
			if (!Device.Flags.Contains(CollectionViewExperimental))
			{
				if (!String.IsNullOrEmpty(memberName))
				{
					if (!String.IsNullOrEmpty(constructorHint))
					{
						constructorHint = constructorHint + " ";
					}

					var call = $"('{constructorHint}{memberName}')";

					var errorMessage = $"The class, property, or method you are attempting to use {call} is part of "
										+ "CollectionView; to use it, you must opt-in by calling "
										+  $"Forms.SetFlags(\"{CollectionViewExperimental}\") before calling Forms.Init().";
					throw new InvalidOperationException(errorMessage);
				}

				var genericErrorMessage = 
					$"To use CollectionView or associated classes, you must opt-in by calling " 
					+ $"Forms.SetFlags(\"{CollectionViewExperimental}\") before calling Forms.Init().";
				throw new InvalidOperationException(genericErrorMessage);
			}
		}
	}
}
