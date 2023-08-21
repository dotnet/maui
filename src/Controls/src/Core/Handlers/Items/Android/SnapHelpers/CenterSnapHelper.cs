// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class CenterSnapHelper : NongreedySnapHelper
	{
		public override AView FindSnapView(RecyclerView.LayoutManager layoutManager)
		{
			if (!CanSnap)
			{
				return null;
			}

			return base.FindSnapView(layoutManager);
		}
	}
}
