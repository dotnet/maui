#nullable disable

#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required

using System;
using System.Collections.Generic;
using Android.Runtime;
using Java.Interop;

namespace Bumptech.Glide
{
	delegate IntPtr _JniMarshal_PP_L(IntPtr jnienv, IntPtr klass);

	// Metadata.xml XPath class reference: path="/api/package[@name='com.bumptech.glide']/class[@name='GeneratedAppGlideModule']"
	[global::Android.Runtime.Register("com/bumptech/glide/GeneratedAppGlideModule", DoNotGenerateAcw = true)]
	public abstract partial class GeneratedAppGlideModule : global::Bumptech.Glide.Module.AppGlideModule
	{
		static readonly JniPeerMembers _members = new XAPeerMembers("com/bumptech/glide/GeneratedAppGlideModule", typeof(GeneratedAppGlideModule));

		internal static new IntPtr class_ref
		{
			get { return _members.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers
		{
			get { return _members; }
		}

		[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override IntPtr ThresholdClass
		{
			get { return _members.JniPeerType.PeerReference.Handle; }
		}

		[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType
		{
			get { return _members.ManagedPeerType; }
		}

		protected GeneratedAppGlideModule(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		// Metadata.xml XPath constructor reference: path="/api/package[@name='com.bumptech.glide']/class[@name='GeneratedAppGlideModule']/constructor[@name='GeneratedAppGlideModule' and count(parameter)=0]"
		[Register(".ctor", "()V", "")]
		public unsafe GeneratedAppGlideModule() : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
		{
			const string __id = "()V";

			if (((global::Java.Lang.Object)this).Handle != IntPtr.Zero)
				return;

			try
			{
				var __r = _members.InstanceMethods.StartCreateInstance(__id, ((object)this).GetType(), null);
				SetHandle(__r.Handle, JniHandleOwnership.TransferLocalRef);
				_members.InstanceMethods.FinishCreateInstance(__id, this, null);
			}
			finally
			{
			}
		}

		static Delegate cb_getExcludedModuleClasses;
#pragma warning disable 0169
		static Delegate GetGetExcludedModuleClassesHandler()
		{
			if (cb_getExcludedModuleClasses == null)
				cb_getExcludedModuleClasses = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_L)n_GetExcludedModuleClasses);
			return cb_getExcludedModuleClasses;
		}

		static IntPtr n_GetExcludedModuleClasses(IntPtr jnienv, IntPtr native__this)
		{
			var __this = global::Java.Lang.Object.GetObject<global::Bumptech.Glide.GeneratedAppGlideModule>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
			return global::Android.Runtime.JavaSet<global::Java.Lang.Class>.ToLocalJniHandle(__this.ExcludedModuleClasses);
		}
#pragma warning restore 0169

		public abstract global::System.Collections.Generic.ICollection<global::Java.Lang.Class> ExcludedModuleClasses
		{
			// Metadata.xml XPath method reference: path="/api/package[@name='com.bumptech.glide']/class[@name='GeneratedAppGlideModule']/method[@name='getExcludedModuleClasses' and count(parameter)=0]"
			[Register("getExcludedModuleClasses", "()Ljava/util/Set;", "GetGetExcludedModuleClassesHandler")]
			get;
		}

		static Delegate cb_getRequestManagerFactory;
#pragma warning disable 0169
		static Delegate GetGetRequestManagerFactoryHandler()
		{
			if (cb_getRequestManagerFactory == null)
				cb_getRequestManagerFactory = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_L)n_GetRequestManagerFactory);
			return cb_getRequestManagerFactory;
		}

		static IntPtr n_GetRequestManagerFactory(IntPtr jnienv, IntPtr native__this)
		{
			var __this = global::Java.Lang.Object.GetObject<global::Bumptech.Glide.GeneratedAppGlideModule>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
			return JNIEnv.ToLocalJniHandle(__this.RequestManagerFactory);
		}
#pragma warning restore 0169

		public virtual unsafe global::Bumptech.Glide.Manager.RequestManagerRetriever.IRequestManagerFactory RequestManagerFactory
		{
			// Metadata.xml XPath method reference: path="/api/package[@name='com.bumptech.glide']/class[@name='GeneratedAppGlideModule']/method[@name='getRequestManagerFactory' and count(parameter)=0]"
			[Register("getRequestManagerFactory", "()Lcom/bumptech/glide/manager/RequestManagerRetriever$RequestManagerFactory;", "GetGetRequestManagerFactoryHandler")]
			get
			{
				const string __id = "getRequestManagerFactory.()Lcom/bumptech/glide/manager/RequestManagerRetriever$RequestManagerFactory;";
				try
				{
					var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
					return global::Java.Lang.Object.GetObject<global::Bumptech.Glide.Manager.RequestManagerRetriever.IRequestManagerFactory>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
				}
				finally
				{
				}
			}
		}

	}

	[global::Android.Runtime.Register("com/bumptech/glide/GeneratedAppGlideModule", DoNotGenerateAcw = true)]
	internal partial class GeneratedAppGlideModuleInvoker : GeneratedAppGlideModule
	{
		public GeneratedAppGlideModuleInvoker(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
		{
		}

		static readonly JniPeerMembers _members = new XAPeerMembers("com/bumptech/glide/GeneratedAppGlideModule", typeof(GeneratedAppGlideModuleInvoker));

		[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		public override global::Java.Interop.JniPeerMembers JniPeerMembers
		{
			get { return _members; }
		}

		[global::System.Diagnostics.DebuggerBrowsable(global::System.Diagnostics.DebuggerBrowsableState.Never)]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		protected override global::System.Type ThresholdType
		{
			get { return _members.ManagedPeerType; }
		}

		public override unsafe global::System.Collections.Generic.ICollection<global::Java.Lang.Class> ExcludedModuleClasses
		{
			// Metadata.xml XPath method reference: path="/api/package[@name='com.bumptech.glide']/class[@name='GeneratedAppGlideModule']/method[@name='getExcludedModuleClasses' and count(parameter)=0]"
			[Register("getExcludedModuleClasses", "()Ljava/util/Set;", "GetGetExcludedModuleClassesHandler")]
			get
			{
				const string __id = "getExcludedModuleClasses.()Ljava/util/Set;";
				try
				{
					var __rm = _members.InstanceMethods.InvokeAbstractObjectMethod(__id, this, null);
					return global::Android.Runtime.JavaSet<global::Java.Lang.Class>.FromJniHandle(__rm.Handle, JniHandleOwnership.TransferLocalRef);
				}
				finally
				{
				}
			}
		}

	}
}
