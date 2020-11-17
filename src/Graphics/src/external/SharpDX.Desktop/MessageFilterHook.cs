// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.Desktop.Collections;

namespace SharpDX.Desktop
{
    /// <summary>
    /// Provides a hook to WndProc of an existing window handle using <see cref="IMessageFilter"/>.
    /// </summary>
    public class MessageFilterHook
    {
        #region Constants and Fields

        private static readonly Dictionary<IntPtr, MessageFilterHook> RegisteredHooks = new Dictionary<IntPtr, MessageFilterHook>(EqualityComparer.DefaultIntPtr);

        private readonly IntPtr defaultWndProc;

        private readonly IntPtr hwnd;

        private readonly Win32Native.WndProc newWndProc;

        private readonly IntPtr newWndProcPtr;

        private List<IMessageFilter> currentFilters;

        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32.MessageFilterHook" /> class.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        private MessageFilterHook(IntPtr hwnd)
        {
            this.currentFilters = new List<IMessageFilter>();
            this.hwnd = hwnd;

            // Retrieve the previous WndProc associated with this window handle
            defaultWndProc = Win32Native.GetWindowLong(hwnd, Win32Native.WindowLongType.WndProc);

            // Create a pointer to the new WndProc
            newWndProc = WndProc;
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);

            // Set our own private wndproc in order to catch NCDestroy message
            Win32Native.SetWindowLong(hwnd, Win32Native.WindowLongType.WndProc, newWndProcPtr);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds a message filter to a window.
        /// </summary>
        /// <param name="hwnd">The handle of the window.</param>
        /// <param name="messageFilter">The message filter.</param>
        public static void AddMessageFilter(IntPtr hwnd, IMessageFilter messageFilter)
        {
            lock (RegisteredHooks)
            {
                hwnd = GetSafeWindowHandle(hwnd);
                if (!RegisteredHooks.TryGetValue(hwnd, out var hook))
                {
                    hook = new MessageFilterHook(hwnd);
                    RegisteredHooks.Add(hwnd, hook);
                }
                hook.AddMessageMilter(messageFilter);
            }
        }

        /// <summary>
        /// Removes a message filter associated with a window.
        /// </summary>
        /// <param name="hwnd">The handle of the window.</param>
        /// <param name="messageFilter">The message filter.</param>
        public static void RemoveMessageFilter(IntPtr hwnd, IMessageFilter messageFilter)
        {
            lock (RegisteredHooks)
            {
                hwnd = GetSafeWindowHandle(hwnd);
                if (RegisteredHooks.TryGetValue(hwnd, out var hook))
                {
                    hook.RemoveMessageFilter(messageFilter);

                    if (hook.isDisposed)
                    {
                        RegisteredHooks.Remove(hwnd);
                        hook.RestoreWndProc();
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void AddMessageMilter(IMessageFilter filter)
        {
            // Make a copy of the filters in order to support a lightweight threadsafe
            var filters = new List<IMessageFilter>(this.currentFilters);
            if (!filters.Contains(filter))
                filters.Add(filter);
            this.currentFilters = filters;
        }

        private void RemoveMessageFilter(IMessageFilter filter)
        {
            // Make a copy of the filters in order to support a lightweight threadsafe
            var filters = new List<IMessageFilter>(this.currentFilters);
            filters.Remove(filter);

            // If there are no more filters, then we can remove the hook
            if (filters.Count == 0)
            {
                this.isDisposed = true;
                this.RestoreWndProc();
            }
            this.currentFilters = filters;
        }

        private void RestoreWndProc()
        {
            var currentProc = Win32Native.GetWindowLong(hwnd, Win32Native.WindowLongType.WndProc);
            if (currentProc == newWndProcPtr)
            {
                // Restore back default WndProc only if the previous callback is owned by this message filter hook
                Win32Native.SetWindowLong(hwnd, Win32Native.WindowLongType.WndProc, defaultWndProc);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (this.isDisposed)
                this.RestoreWndProc();
            else
            {
                var message = new Message() { HWnd = hwnd, LParam = lParam, Msg = msg, WParam = wParam };
                foreach (var messageFilter in this.currentFilters)
                {
                    if (messageFilter.PreFilterMessage(ref message))
                        return message.Result;
                }
            }

            var result = Win32Native.CallWindowProc(defaultWndProc, hWnd, msg, wParam, lParam);
            return result;
        }

        private static IntPtr GetSafeWindowHandle(IntPtr hwnd)
        {
            return hwnd == IntPtr.Zero ? Process.GetCurrentProcess().MainWindowHandle : hwnd;
        }

        #endregion
    }
}