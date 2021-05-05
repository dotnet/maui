using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class InterceptVisualStateManager : Microsoft.UI.Xaml.VisualStateManager
	{
		static InterceptVisualStateManager s_instance;

		public static readonly DependencyProperty FormsElementProperty = DependencyProperty.RegisterAttached(
				"FormsElement", typeof(VisualElement), typeof(Control), new PropertyMetadata(null));

		public static void SetFormsElement(Control frameworkElement, VisualElement value)
		{
			frameworkElement.SetValue(FormsElementProperty, value);
		}

		public static VisualElement GetFormsElement(Control frameworkElement)
		{
			return (VisualElement)frameworkElement.GetValue(FormsElementProperty);
		}

		internal static InterceptVisualStateManager Instance
		{
			get
			{
				s_instance = s_instance ?? new InterceptVisualStateManager();
				return s_instance;
			}
		}

		// For most of the UWP controls, this custom VisualStateManager is injected to prevent the default Windows
		// VSM from handling states which the Forms VSM is already managing. 
		// The exception are the controls which are built on TextBox (SearchBar, Entry, Editor); there's a UWP
		// bug wherein the GoToStateCore override for those controls is never called (Item 10976357 in VSTS)
		// So until that's resolved, the FormsTextBox control is doing that work as best it can. 

		protected override bool GoToStateCore(Control control, FrameworkElement templateRoot, string stateName, 
			Microsoft.UI.Xaml.VisualStateGroup @group, Microsoft.UI.Xaml.VisualState state, bool useTransitions)
		{
			// If this custom VSM is in play, it's because the control is using the Forms VSM or the user has disabled
			// legacy color handling. Either way, we may need to prevent the Windows VSM from transitioning to the new
			// state. So we intercept the transition here.

			if (ShouldIntercept(control, state, stateName))
			{
				return false;
			}

			return base.GoToStateCore(control, templateRoot, stateName, @group, state, useTransitions);
		}

		static bool ShouldIntercept(Control control, Microsoft.UI.Xaml.VisualState state, string stateName)
		{
			if (state == null || state.Name == "Normal")
			{
				// We don't intercept the "Normal" state, that's the base state onto which Forms applies properties
				return false;
			}

			// Retrieve the VisualElement we're managing states for
			var visualElement = GetFormsElement(control);

			if (visualElement == null)
			{
				return false;
			}

			// Retrieve the set of VisualStateGroups for the VisualElement
			var groups = VisualStateManager.GetVisualStateGroups(visualElement);

			if (groups == null)
			{
				// No groups? 
				// Then the user disabled legacy color management through the platform specific, not by using the XFVSM
				// So our ignored states lists is effectively "Disabled" and "Focused"
				if (state.Name == "Disabled" || state.Name == "Focused")
				{
					return true;
				}
			}
			else
			{
				// Check the states the XFVSM is managing
				foreach (VisualStateGroup vsg in groups)
				{
					foreach (VisualState vs in vsg.States)
					{
						if (vs.Name == stateName)
						{
							// The XFVSM is already handling this state, so don't let the Windows VSM do it
							return true;
						}
					}
				}
			}

			return false;
		}

		internal static void Hook(FrameworkElement rootElement, Control control, VisualElement visualElement)
		{
			if (rootElement == null)
			{
				return;
			}

			// Set this as the custom VSM for that root element, 
			// and associate the VisualElement with this UWP Control 
			SetCustomVisualStateManager(rootElement, Instance);
			SetFormsElement(control, visualElement);
		}
	}
}