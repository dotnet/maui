// Copyright (c) 2010-2014 SharpDX - SharpDX Team
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

// currently it is working correctly only on Desktop platform

using System;
using System.Runtime.InteropServices;

namespace SharpDX.Desktop.Diagnostics
{
    /// <summary>
    /// Helper class to allow programmatic capturing of graphics information that can be loaded later in Visual Studio.
    /// This is a managed implementation of the VsDbg class (http://msdn.microsoft.com/en-us/library/vstudio/dn440549.aspx).
    /// Requires to have installed VS Remote Tools.
    /// http://msdn.microsoft.com/en-us/library/vstudio/hh708963.aspx
    /// http://msdn.microsoft.com/en-us/library/vstudio/hh780905.aspx
    /// </summary>
    public sealed class VSGraphicsDebugger : DisposeBase
    {
        /// <summary>
        /// Helper structure to ease the begin/end graphics capturing
        /// </summary>
        /// <example>
        /// var debugger = new VSGraphicsDebugger();
        /// using(debugger.BeginCapture())
        /// {
        ///     ...
        /// }
        /// </example>
        public struct CaptureToken : IDisposable
        {
            private readonly VSGraphicsDebugger debugger;

            /// <summary>
            /// Creates a new instance of the <see cref="CaptureToken"/> structure.
            /// </summary>
            /// <param name="debugger">The attanched graphics debugger.</param>
            internal CaptureToken(VSGraphicsDebugger debugger)
                : this()
            {
                this.debugger = debugger;
            }

            /// <summary>
            /// Ends the capture by calling <see cref="VSGraphicsDebugger.EndCapture"/>.
            /// </summary>
            public void Dispose()
            {
                debugger.EndCapture();
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VSGraphicsDebugger"/> class and prepares the in-app component of graphics diagnostics to actively capture and record graphics information..
        /// </summary>
        /// <param name="logFileName">The destination filename for log writing.</param>
        public VSGraphicsDebugger(string logFileName)
        {
            VsgDbgInit(logFileName);
        }

        /// <summary>
        /// Copies the contents of the active graphics log (.vsglog) file into a new file.
        /// </summary>
        /// <param name="destinationFileName">The new log file name.</param>
        public void CopyLogFile(string destinationFileName)
        {
            VsgDbgCopy(destinationFileName);
        }

        /// <summary>
        /// Toggles the graphics diagnostics HUD overlay on or off.
        /// </summary>
        public void ToggleHUD()
        {
            VsgDbgToggleHUD();
        }

        /// <summary>
        /// Adds a custom message to the graphics diagnostics HUD (Head-Up Display).
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void AddHUDMessage(string message)
        {
            VsgDbgAddHUDMessage(message);
        }

        /// <summary>
        /// Captures the remainder of the current frame to the graphics log file.
        /// </summary>
        public void CaptureCurrentFrame()
        {
            VsgDbgCaptureCurrentFrame();
        }

        /// <summary>
        /// Begins a capture interval that will end with <see cref="EndCapture"/>.
        /// </summary>
        /// <returns>A <see cref="CaptureToken"/> instance that once disposed, calls automatically the <see cref="EndCapture"/> method.</returns>
        public CaptureToken BeginCapture()
        {
            VsgDbgBeginCapture();

            return new CaptureToken(this);
        }

        /// <summary>
        /// Ends a capture interval that was started with <see cref="BeginCapture"/>.
        /// </summary>
        public void EndCapture()
        {
            VsgDbgEndCapture();
        }

        /// <summary>
        /// Finalizes the graphics log file, closes it, and frees resources that were used while the app was actively recording graphics information.
        /// </summary>
        /// <param name="disposing">Ignored.</param>
        protected override void Dispose(bool disposing)
        {
            VsgDbgUnInit();
        }

        #region vsgcapture.h imports:

        // extern "C" void __stdcall VsgDbgInit(_In_z_ wchar_t const * szVSGLog);
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgInit", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgInit(string logName);

        // extern "C" void __stdcall VsgDbgUnInit();
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgUnInit", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgUnInit();

        // extern "C" void __stdcall VsgDbgToggleHUD();
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgToggleHUD", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgToggleHUD();

        // extern "C" void __stdcall VsgDbgCaptureCurrentFrame();
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgCaptureCurrentFrame", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgCaptureCurrentFrame();

        // extern "C" void __stdcall VsgDbgBeginCapture();
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgBeginCapture", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgBeginCapture();

        // extern "C" void __stdcall VsgDbgEndCapture();
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgEndCapture", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgEndCapture();

        // extern "C" void __stdcall VsgDbgCopy(_In_z_ wchar_t const * szNewVSGLog);
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgCopy", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgCopy(string newLogName);

        // extern "C" void __stdcall VsgDbgAddHUDMessage(_In_z_ wchar_t const * szMessage);
        [DllImport("VsGraphicsHelper.dll", EntryPoint = "VsgDbgAddHUDMessage", CharSet = CharSet.Unicode)]
        private static extern void VsgDbgAddHUDMessage(string message);

        #endregion
    }
}
