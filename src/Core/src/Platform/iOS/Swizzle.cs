using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Platform.iOS
{
	internal class Swizzle<TDelegate> : IDisposable where TDelegate : class
	{
		protected IntPtr originalMethod;
		protected IntPtr originalImpl;
		protected IntPtr swizzleSel;
		protected IntPtr newImpl;
		protected bool isClassMethod;

		protected TDelegate dlg; // Your delegate

		public Swizzle(Class classToSwizzle, IntPtr selector, TDelegate del, bool isClassMethod = false)
		{
			dlg = del;
			swizzleSel = selector;

			originalMethod = isClassMethod ? LibObjc.class_getClassMethod(classToSwizzle.Handle, swizzleSel) : LibObjc.class_getInstanceMethod(classToSwizzle.Handle, swizzleSel);
			originalImpl = LibObjc.method_getImplementation(originalMethod);

			newImpl = Marshal.GetFunctionPointerForDelegate((del as System.Delegate)!);
			LibObjc.method_setImplementation(originalMethod, newImpl);
		}

		public Swizzle(Type classToSwizzle, string selector, TDelegate del, bool isClassMethod = false)
			: this(classToSwizzle.GetClass()!, Selector.GetHandle(selector), del, isClassMethod)
		{
		}

		public Swizzle(NSObject classToSwizzle, string selector, TDelegate del, bool isClassMethod = false)
			: this(classToSwizzle.Class, Selector.GetHandle(selector), del, isClassMethod)
		{
		}

		public Unswizzle Restore()
		{
			return new Unswizzle(this);
		}

		public virtual void Dispose()
		{
			LibObjc.method_setImplementation(originalMethod, originalImpl);
		}

		// Use this class to call the original implementation
		public class Unswizzle : IDisposable
		{
			Swizzle<TDelegate>? swizzle;
			public Unswizzle(Swizzle<TDelegate> swizzle)
			{
				this.swizzle = swizzle;
				LibObjc.method_setImplementation(swizzle.originalMethod, swizzle.originalImpl);
			}

			public TDelegate? Delegate
			{
				get
				{
					return Marshal.GetDelegateForFunctionPointer(swizzle!.originalImpl, swizzle.dlg.GetType()) as TDelegate;
				}
			}

			public void Dispose()
			{
				LibObjc.method_setImplementation(swizzle!.originalMethod, swizzle.newImpl);
				swizzle = null;
			}
		}
	}
}

