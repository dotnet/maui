// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Embedding.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ShowAlertsAndActionSheets { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ShowWebView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView UIViewController { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ShowAlertsAndActionSheets != null) {
                ShowAlertsAndActionSheets.Dispose ();
                ShowAlertsAndActionSheets = null;
            }

            if (ShowWebView != null) {
                ShowWebView.Dispose ();
                ShowWebView = null;
            }

            if (UIViewController != null) {
                UIViewController.Dispose ();
                UIViewController = null;
            }
        }
    }
}