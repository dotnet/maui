#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	internal interface IElementDefinition
	{
		Element Parent { get; set; }

		//Use these 2 instead of an event to avoid cloning way too much multicastdelegates on mono

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void RemoveResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged);
	}
}