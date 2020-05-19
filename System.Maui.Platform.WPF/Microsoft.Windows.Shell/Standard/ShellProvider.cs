namespace Standard
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    #region Enums and Static Property Classes

    /// <summary>ASSOCIATIONLEVEL, AL_*</summary>
    internal enum AL
    {
        MACHINE,
        EFFECTIVE,
        USER,
    }

    /// <summary>ASSOCIATIONTYPE, AT_*</summary>
    internal enum AT
    {
        FILEEXTENSION,
        URLPROTOCOL,
        STARTMENUCLIENT,
        MIMETYPE,
    }

    /// <summary>FileDialog AddPlace options.  FDAP_*</summary>
    internal enum FDAP : uint
    {
        BOTTOM = 0x00000000,
        TOP = 0x00000001,
    }

    /// <summary>IFileDialog options.  FOS_*</summary>
    [Flags]
    internal enum FOS : uint
    {
        OVERWRITEPROMPT = 0x00000002,
        STRICTFILETYPES = 0x00000004,
        NOCHANGEDIR = 0x00000008,
        PICKFOLDERS = 0x00000020,
        FORCEFILESYSTEM = 0x00000040,
        ALLNONSTORAGEITEMS = 0x00000080,
        NOVALIDATE = 0x00000100,
        ALLOWMULTISELECT = 0x00000200,
        PATHMUSTEXIST = 0x00000800,
        FILEMUSTEXIST = 0x00001000,
        CREATEPROMPT = 0x00002000,
        SHAREAWARE = 0x00004000,
        NOREADONLYRETURN = 0x00008000,
        NOTESTFILECREATE = 0x00010000,
        HIDEMRUPLACES = 0x00020000,
        HIDEPINNEDPLACES = 0x00040000,
        NODEREFERENCELINKS = 0x00100000,
        DONTADDTORECENT = 0x02000000,
        FORCESHOWHIDDEN = 0x10000000,
        DEFAULTNOMINIMODE = 0x20000000,
        FORCEPREVIEWPANEON = 0x40000000,
    }

    /// <summary>FDE_OVERWRITE_RESPONSE.  FDEOR_*</summary>
    internal enum FDEOR
    {
        DEFAULT = 0x00000000,
        ACCEPT = 0x00000001,
        REFUSE = 0x00000002,
    }

    /// <summary>FDE_SHAREVIOLATION_RESPONSE.  FDESVR_*</summary>
    internal enum FDESVR
    {
        DEFAULT = 0x00000000,
        ACCEPT = 0x00000001,
        REFUSE = 0x00000002,
    }

    /// <summary>ShellItem attribute flags.  SIATTRIBFLAGS_*</summary>
    internal enum SIATTRIBFLAGS
    {
        AND = 0x00000001,
        OR = 0x00000002,
        APPCOMPAT = 0x00000003,
    }

    internal enum APPDOCLISTTYPE
    {
        ADLT_RECENT = 0,   // The recently used documents list
        ADLT_FREQUENT,     // The frequently used documents list
    }

    /// <summary>
    /// Flags for SetTabProperties.  STPF_*
    /// </summary>
    /// <remarks>The native enum was called STPFLAG.</remarks>
    [Flags]
    internal enum STPF
    {
        NONE = 0x00000000,
        USEAPPTHUMBNAILALWAYS = 0x00000001,
        USEAPPTHUMBNAILWHENACTIVE = 0x00000002,
        USEAPPPEEKALWAYS = 0x00000004,
        USEAPPPEEKWHENACTIVE = 0x00000008,
    }

    /// <summary>
    /// Flags for Setting Taskbar Progress state.  TBPF_*
    /// </summary>
    /// <remarks>
    /// The native enum was called TBPFLAG.
    /// </remarks>
    internal enum TBPF
    {
        NOPROGRESS = 0x00000000,
        INDETERMINATE = 0x00000001,
        NORMAL = 0x00000002,
        ERROR = 0x00000004,
        PAUSED = 0x00000008,
    }

    /// <summary>
    /// THUMBBUTTON mask.  THB_*
    /// </summary>
    [Flags]
    internal enum THB : uint
    {
        BITMAP = 0x0001,
        ICON = 0x0002,
        TOOLTIP = 0x0004,
        FLAGS = 0x0008,
    }

    /// <summary>
    /// THUMBBUTTON flags.  THBF_*
    /// </summary>
    [Flags]
    internal enum THBF : uint
    {
        ENABLED = 0x0000,
        DISABLED = 0x0001,
        DISMISSONCLICK = 0x0002,
        NOBACKGROUND = 0x0004,
        HIDDEN = 0x0008,
        // Added post-beta
        NONINTERACTIVE = 0x0010,
    }

    /// <summary>
    /// GetPropertyStoreFlags.  GPS_*.
    /// </summary>
    /// <remarks>
    /// These are new for Vista, but are used in downlevel components
    /// </remarks>
    internal enum GPS
    {
        // If no flags are specified (GPS_DEFAULT), a read-only property store is returned that includes properties for the file or item.
        // In the case that the shell item is a file, the property store contains:
        //     1. properties about the file from the file system
        //     2. properties from the file itself provided by the file's property handler, unless that file is offline,
        //         see GPS_OPENSLOWITEM
        //     3. if requested by the file's property handler and supported by the file system, properties stored in the
        //     alternate property store.
        //
        // Non-file shell items should return a similar read-only store
        //
        // Specifying other GPS_ flags modifies the store that is returned
        DEFAULT = 0x00000000,
        HANDLERPROPERTIESONLY = 0x00000001,   // only include properties directly from the file's property handler
        READWRITE = 0x00000002,   // Writable stores will only include handler properties
        TEMPORARY = 0x00000004,   // A read/write store that only holds properties for the lifetime of the IShellItem object
        FASTPROPERTIESONLY = 0x00000008,   // do not include any properties from the file's property handler (because the file's property handler will hit the disk)
        OPENSLOWITEM = 0x00000010,   // include properties from a file's property handler, even if it means retrieving the file from offline storage.
        DELAYCREATION = 0x00000020,   // delay the creation of the file's property handler until those properties are read, written, or enumerated
        BESTEFFORT = 0x00000040,   // For readonly stores, succeed and return all available properties, even if one or more sources of properties fails. Not valid with GPS_READWRITE.
        NO_OPLOCK = 0x00000080,   // some data sources protect the read property store with an oplock, this disables that
        MASK_VALID = 0x000000FF,
    }

    /// <summary>
    /// KNOWNDESTCATEGORY.  KDC_*
    /// </summary>
    internal enum KDC
    {
        FREQUENT = 1,
        RECENT,
    }

    // IShellFolder::GetAttributesOf flags
    [Flags]
    internal enum SFGAO : uint
    {
        /// <summary>Objects can be copied</summary>
        /// <remarks>DROPEFFECT_COPY</remarks>
        CANCOPY = 0x1,
        /// <summary>Objects can be moved</summary>
        /// <remarks>DROPEFFECT_MOVE</remarks>
        CANMOVE = 0x2,
        /// <summary>Objects can be linked</summary>
        /// <remarks>
        /// DROPEFFECT_LINK.
        /// 
        /// If this bit is set on an item in the shell folder, a
        /// 'Create Shortcut' menu item will be added to the File
        /// menu and context menus for the item.  If the user selects
        /// that command, your IContextMenu::InvokeCommand() will be called
        /// with 'link'.
        /// That flag will also be used to determine if 'Create Shortcut'
        /// should be added when the item in your folder is dragged to another
        /// folder.
        /// </remarks>
        CANLINK = 0x4,
        /// <summary>supports BindToObject(IID_IStorage)</summary>
        STORAGE = 0x00000008,
        /// <summary>Objects can be renamed</summary>
        CANRENAME = 0x00000010,
        /// <summary>Objects can be deleted</summary>
        CANDELETE = 0x00000020,
        /// <summary>Objects have property sheets</summary>
        HASPROPSHEET = 0x00000040,

        // unused = 0x00000080,

        /// <summary>Objects are drop target</summary>
        DROPTARGET = 0x00000100,
        CAPABILITYMASK = 0x00000177,
        // unused = 0x00000200,
        // unused = 0x00000400,
        // unused = 0x00000800,
        // unused = 0x00001000,
        /// <summary>Object is encrypted (use alt color)</summary>
        ENCRYPTED = 0x00002000,
        /// <summary>'Slow' object</summary>
        ISSLOW = 0x00004000,
        /// <summary>Ghosted icon</summary>
        GHOSTED = 0x00008000,
        /// <summary>Shortcut (link)</summary>
        LINK = 0x00010000,
        /// <summary>Shared</summary>
        SHARE = 0x00020000,
        /// <summary>Read-only</summary>
        READONLY = 0x00040000,
        /// <summary> Hidden object</summary>
        HIDDEN = 0x00080000,
        DISPLAYATTRMASK = 0x000FC000,
        /// <summary> May contain children with SFGAO_FILESYSTEM</summary>
        FILESYSANCESTOR = 0x10000000,
        /// <summary>Support BindToObject(IID_IShellFolder)</summary>
        FOLDER = 0x20000000,
        /// <summary>Is a win32 file system object (file/folder/root)</summary>
        FILESYSTEM = 0x40000000,
        /// <summary>May contain children with SFGAO_FOLDER (may be slow)</summary>
        HASSUBFOLDER = 0x80000000,
        CONTENTSMASK = 0x80000000,
        /// <summary>Invalidate cached information (may be slow)</summary>
        VALIDATE = 0x01000000,
        /// <summary>Is this removeable media?</summary>
        REMOVABLE = 0x02000000,
        /// <summary> Object is compressed (use alt color)</summary>
        COMPRESSED = 0x04000000,
        /// <summary>Supports IShellFolder, but only implements CreateViewObject() (non-folder view)</summary>
        BROWSABLE = 0x08000000,
        /// <summary>Is a non-enumerated object (should be hidden)</summary>
        NONENUMERATED = 0x00100000,
        /// <summary>Should show bold in explorer tree</summary>
        NEWCONTENT = 0x00200000,
        /// <summary>Obsolete</summary>
        CANMONIKER = 0x00400000,
        /// <summary>Obsolete</summary>
        HASSTORAGE = 0x00400000,
        /// <summary>Supports BindToObject(IID_IStream)</summary>
        STREAM = 0x00400000,
        /// <summary>May contain children with SFGAO_STORAGE or SFGAO_STREAM</summary>
        STORAGEANCESTOR = 0x00800000,
        /// <summary>For determining storage capabilities, ie for open/save semantics</summary>
        STORAGECAPMASK = 0x70C50008,
        /// <summary>
        /// Attributes that are masked out for PKEY_SFGAOFlags because they are considered
        /// to cause slow calculations or lack context
        /// (SFGAO_VALIDATE | SFGAO_ISSLOW | SFGAO_HASSUBFOLDER and others)
        /// </summary>
        PKEYSFGAOMASK = 0x81044000,
    }

    /// <summary>
    /// IShellFolder::EnumObjects grfFlags bits.  Also called SHCONT
    /// </summary>
    internal enum SHCONTF
    {
        CHECKING_FOR_CHILDREN = 0x0010,   // hint that client is checking if (what) child items the folder contains - not all details (e.g. short file name) are needed
        FOLDERS = 0x0020,   // only want folders enumerated (SFGAO_FOLDER)
        NONFOLDERS = 0x0040,   // include non folders (items without SFGAO_FOLDER)
        INCLUDEHIDDEN = 0x0080,   // show items normally hidden (items with SFGAO_HIDDEN)
        INIT_ON_FIRST_NEXT = 0x0100,   // DEFUNCT - this is always assumed
        NETPRINTERSRCH = 0x0200,   // hint that client is looking for printers
        SHAREABLE = 0x0400,   // hint that client is looking sharable resources (local drives or hidden root shares)
        STORAGE = 0x0800,   // include all items with accessible storage and their ancestors
        NAVIGATION_ENUM = 0x1000,   // mark child folders to indicate that they should provide a "navigation" enumeration by default
        FASTITEMS = 0x2000,   // hint that client is only interested in items that can be enumerated quickly
        FLATLIST = 0x4000,   // enumerate items as flat list even if folder is stacked
        ENABLE_ASYNC = 0x8000,   // inform enumerator that client is listening for change notifications so enumerator does not need to be complete, items can be reported via change notifications
    }

    /// <summary>
    /// IShellFolder::GetDisplayNameOf/SetNameOf uFlags.  Also called SHGDNF.
    /// </summary>
    /// <remarks>
    /// For compatibility with SIGDN, these bits must all sit in the LOW word.
    /// </remarks>
    [Flags]
    internal enum SHGDN
    {
        SHGDN_NORMAL = 0x0000,  // default (display purpose)
        SHGDN_INFOLDER = 0x0001,  // displayed under a folder (relative)
        SHGDN_FOREDITING = 0x1000,  // for in-place editing
        SHGDN_FORADDRESSBAR = 0x4000,  // UI friendly parsing name (remove ugly stuff)
        SHGDN_FORPARSING = 0x8000,  // parsing name for ParseDisplayName()
    }

    /// <summary>
    /// SHELLITEMCOMPAREHINTF.  SICHINT_*.
    /// </summary>
    internal enum SICHINT : uint
    {
        /// <summary>iOrder based on display in a folder view</summary>
        DISPLAY = 0x00000000,
        /// <summary>exact instance compare</summary>
        ALLFIELDS = 0x80000000,
        /// <summary>iOrder based on canonical name (better performance)</summary>
        CANONICAL = 0x10000000,
        TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
    };

    /// <summary>
    /// ShellItem enum.  SIGDN_*.
    /// </summary>
    internal enum SIGDN : uint
    {                                             // lower word (& with 0xFFFF)
        NORMALDISPLAY = 0x00000000, // SHGDN_NORMAL
        PARENTRELATIVEPARSING = 0x80018001, // SHGDN_INFOLDER | SHGDN_FORPARSING
        DESKTOPABSOLUTEPARSING = 0x80028000, // SHGDN_FORPARSING
        PARENTRELATIVEEDITING = 0x80031001, // SHGDN_INFOLDER | SHGDN_FOREDITING
        DESKTOPABSOLUTEEDITING = 0x8004c000, // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        FILESYSPATH = 0x80058000, // SHGDN_FORPARSING
        URL = 0x80068000, // SHGDN_FORPARSING
        PARENTRELATIVEFORADDRESSBAR = 0x8007c001, // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        PARENTRELATIVE = 0x80080001, // SHGDN_INFOLDER
    }

    /// <summary>
    /// STR_GPS_*
    /// </summary>
    /// <remarks>
    /// When requesting a property store through IShellFolder, you can specify the equivalent of
    /// GPS_DEFAULT by passing in a null IBindCtx parameter.
    ///
    /// You can specify the equivalent of GPS_READWRITE by passing a mode of STGM_READWRITE | STGM_EXCLUSIVE
    /// in the bind context
    ///
    /// Here are the string versions of GPS_ flags, passed to IShellFolder::BindToObject() via IBindCtx::RegisterObjectParam()
    /// These flags are valid when requesting an IPropertySetStorage or IPropertyStore handler
    ///
    /// The meaning of these flags are described above.
    ///
    /// There is no STR_ equivalent for GPS_TEMPORARY because temporary property stores
    /// are provided by IShellItem2 only -- not by the underlying IShellFolder.
    /// </remarks>
    internal static class STR_GPS
    {
        public const string HANDLERPROPERTIESONLY = "GPS_HANDLERPROPERTIESONLY";
        public const string FASTPROPERTIESONLY = "GPS_FASTPROPERTIESONLY";
        public const string OPENSLOWITEM = "GPS_OPENSLOWITEM";
        public const string DELAYCREATION = "GPS_DELAYCREATION";
        public const string BESTEFFORT = "GPS_BESTEFFORT";
        public const string NO_OPLOCK = "GPS_NO_OPLOCK";
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct COMDLG_FILTERSPEC
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszSpec;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    internal struct THUMBBUTTON
    {
        /// <summary>
        /// WPARAM value for a THUMBBUTTON being clicked.
        /// </summary>
        public const int THBN_CLICKED = 0x1800;

        public THB dwMask;
        public uint iId;
        public uint iBitmap;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szTip;
        public THBF dwFlags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PKEY
    {
        /// <summary>fmtid</summary>
        private readonly Guid _fmtid;
        /// <summary>pid</summary>
        private readonly uint _pid;

        public PKEY(Guid fmtid, uint pid)
        {
            _fmtid = fmtid;
            _pid = pid;
        }

        /// <summary>PKEY_Title</summary>
        public static readonly PKEY Title = new PKEY(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        /// <summary>PKEY_AppUserModel_ID</summary>
        public static readonly PKEY AppUserModel_ID = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
        /// <summary>PKEY_AppUserModel_IsDestListSeparator</summary>
        public static readonly PKEY AppUserModel_IsDestListSeparator = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);
        /// <summary>PKEY_AppUserModel_RelaunchCommand</summary>
        public static readonly PKEY AppUserModel_RelaunchCommand = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);
        /// <summary>PKEY_AppUserModel_RelaunchDisplayNameResource</summary>
        public static readonly PKEY AppUserModel_RelaunchDisplayNameResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);
        /// <summary>PKEY_AppUserModel_RelaunchIconResource</summary>
        public static readonly PKEY AppUserModel_RelaunchIconResource = new PKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
    }

    #endregion

    #region Interfaces

    // Application File Extension and URL Protocol Registration
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ApplicationAssociationRegistration),
    ]
    internal interface IApplicationAssociationRegistration
    {
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string QueryCurrentDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
            AT atQueryType,
            AL alQueryLevel);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryAppIsDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
            AT atQueryType,
            AL alQueryLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryAppIsDefaultAll(
            AL alQueryLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName);

        void SetAppAsDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName,
            [MarshalAs(UnmanagedType.LPWStr)] string pszSet,
            AT atSetType);

        void SetAppAsDefaultAll([MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName);

        void ClearUserAssociations();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.EnumIdList),
    ]
    internal interface IEnumIDList
    {
        [PreserveSig()]
        HRESULT Next(uint celt, out IntPtr rgelt, out int pceltFetched);
        [PreserveSig()]
        HRESULT Skip(uint celt);
        void Reset();
        void Clone([Out, MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.EnumObjects),
    ]
    internal interface IEnumObjects
    {
        //[local]
        // This signature might not work... Hopefully don't need this interface though.
        void Next(uint celt, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, IidParameterIndex = 1, SizeParamIndex = 0)] object[] rgelt, [Out] out uint pceltFetched);

        /*
        [call_as(Next)] HRESULT RemoteNext(
            [in] ULONG celt,
            [in] REFIID riid,
            [out, size_is(celt), length_is(*pceltFetched), iid_is(riid)] void **rgelt,
            [out] ULONG *pceltFetched);
         */

        void Skip(uint celt);

        void Reset();

        IEnumObjects Clone();
    }

    /// <summary>Unknown Object Array</summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectArray),
    ]
    internal interface IObjectArray
    {
        uint GetCount();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetAt([In] uint uiIndex, [In] ref Guid riid);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectArray),
    ]
    interface IObjectCollection : IObjectArray
    {
        #region IObjectArray redeclarations
        new uint GetCount();
        [return: MarshalAs(UnmanagedType.IUnknown)]
        new object GetAt([In] uint uiIndex, [In] ref Guid riid);
        #endregion

        void AddObject([MarshalAs(UnmanagedType.IUnknown)] object punk);
        void AddFromArray(IObjectArray poaSource);
        void RemoveObjectAt(uint uiIndex);
        void Clear();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.PropertyStore)
    ]
    internal interface IPropertyStore
    {
        uint GetCount();
        PKEY GetAt(uint iProp);
        void GetValue([In] ref PKEY pkey, [In, Out] PROPVARIANT pv);
        void SetValue([In] ref PKEY pkey, PROPVARIANT pv);
        void Commit();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellFolder),
    ]
    internal interface IShellFolder
    {
        void ParseDisplayName(
            [In] IntPtr hwnd,
            [In] IBindCtx pbc,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            [In, Out] ref int pchEaten,
            [Out] out IntPtr ppidl,
            [In, Out] ref uint pdwAttributes);

        IEnumIDList EnumObjects(
            [In] IntPtr hwnd,
            [In] SHCONTF grfFlags);

        // returns an instance of a sub-folder which is specified by the IDList (pidl).
        // IShellFolder or derived interfaces
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToObject(
            [In] IntPtr pidl,
            [In] IBindCtx pbc,
            [In] ref Guid riid);

        // produces the same result as BindToObject()
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToStorage([In] IntPtr pidl, [In] IBindCtx pbc, [In] ref Guid riid);

        // compares two IDLists and returns the result. The shell
        // explorer always passes 0 as lParam, which indicates 'sort by name'.
        // It should return 0 (as CODE of the scode), if two id indicates the
        // same object; negative value if pidl1 should be placed before pidl2;
        // positive value if pidl2 should be placed before pidl1.
        // use the macro ResultFromShort() to extract the result comparison
        // it deals with the casting and type conversion issues for you
        [PreserveSig]
        HRESULT CompareIDs([In] IntPtr lParam, [In] IntPtr pidl1, [In] IntPtr pidl2);

        // creates a view object of the folder itself. The view
        // object is a difference instance from the shell folder object.
        // 'hwndOwner' can be used  as the owner window of its dialog box or
        // menu during the lifetime of the view object.
        // This member function should always create a new
        // instance which has only one reference count. The explorer may create
        // more than one instances of view object from one shell folder object
        // and treat them as separate instances.
        // returns IShellView derived interface
        [return: MarshalAs(UnmanagedType.Interface)]
        object CreateViewObject([In] IntPtr hwndOwner, [In] ref Guid riid);

        // returns the attributes of specified objects in that
        // folder. 'cidl' and 'apidl' specifies objects. 'apidl' contains only
        // simple IDLists. The explorer initializes *prgfInOut with a set of
        // flags to be evaluated. The shell folder may optimize the operation
        // by not returning unspecified flags.
        void GetAttributesOf(
            [In] uint cidl,
            [In] IntPtr apidl,
            [In, Out] ref SFGAO rgfInOut);

        // creates a UI object to be used for specified objects.
        // The shell explorer passes either IID_IDataObject (for transfer operation)
        // or IID_IContextMenu (for context menu operation) as riid
        // and many other interfaces
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetUIObjectOf(
            [In] IntPtr hwndOwner,
            [In] uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt, SizeParamIndex = 2)] IntPtr apidl,
            [In] ref Guid riid,
            [In, Out] ref uint rgfReserved);

        // returns the display name of the specified object.
        // If the ID contains the display name (in the locale character set),
        // it returns the offset to the name. Otherwise, it returns a pointer
        // to the display name string (UNICODE), which is allocated by the
        // task allocator, or fills in a buffer.
        // use the helper APIS StrRetToStr() or StrRetToBuf() to deal with the different
        // forms of the STRRET structure
        void GetDisplayNameOf([In] IntPtr pidl, [In] SHGDN uFlags, [Out] out IntPtr pName);

        // sets the display name of the specified object.
        // If it changes the ID as well, it returns the new ID which is
        // alocated by the task allocator.
        void SetNameOf([In] IntPtr hwnd,
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
            [In] SHGDN uFlags,
            [Out] out IntPtr ppidlOut);
    }

    /// <summary>
    /// Shell Namespace helper
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellItem),
    ]
    internal interface IShellItem
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToHandler(IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);

        IShellItem GetParent();

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetDisplayName(SIGDN sigdnName);

        SFGAO GetAttributes(SFGAO sfgaoMask);

        int Compare(IShellItem psi, SICHINT hint);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellItemArray),
    ]
    internal interface IShellItemArray
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object BindToHandler(IBindCtx pbc, [In] ref Guid rbhid, [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStore(int flags, [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyDescriptionList([In] ref PKEY keyType, [In] ref Guid riid);

        uint GetAttributes(SIATTRIBFLAGS dwAttribFlags, uint sfgaoMask);

        uint GetCount();

        IShellItem GetItemAt(uint dwIndex);

        [return: MarshalAs(UnmanagedType.Interface)]
        object EnumItems();
    }

    /// <summary>
    /// Shell Namespace helper 2
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellItem2),
    ]
    interface IShellItem2 : IShellItem
    {
        #region IShellItem redeclarations
        [return: MarshalAs(UnmanagedType.Interface)]
        new object BindToHandler([In] IBindCtx pbc, [In] ref Guid bhid, [In] ref Guid riid);
        new IShellItem GetParent();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        new string GetDisplayName(SIGDN sigdnName);
        new SFGAO GetAttributes(SFGAO sfgaoMask);
        new int Compare(IShellItem psi, SICHINT hint);
        #endregion

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStore(
            GPS flags,
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreWithCreateObject(
            GPS flags,
            [MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject,   // factory for low-rights creation of type ICreateObject
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyStoreForKeys(
            IntPtr rgKeys,
            uint cKeys,
            GPS flags,
            [In] ref Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetPropertyDescriptionList(
            IntPtr keyType,
            [In] ref Guid riid);

        // Ensures any cached information in this item is up to date, or returns __HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND) if the item does not exist.
        void Update(IBindCtx pbc);

        PROPVARIANT GetProperty(IntPtr key);

        Guid GetCLSID(IntPtr key);

        FILETIME GetFileTime(IntPtr key);

        int GetInt32(IntPtr key);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetString(IntPtr key);

        uint GetUInt32(IntPtr key);

        ulong GetUInt64(IntPtr key);

        [return: MarshalAs(UnmanagedType.Bool)]
        void GetBool(IntPtr key);
    }

    [
        ComImport,
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ShellLink),
    ]
    internal interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, [In, Out] WIN32_FIND_DATAW pfd, SLGP fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        short GetHotKey();
        void SetHotKey(short wHotKey);
        uint GetShowCmd();
        void SetShowCmd(uint iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.TaskbarList),
    ]
    internal interface ITaskbarList
    {
        /// <summary>
        /// This function must be called first to validate use of other members.
        /// </summary>
        void HrInit();

        /// <summary>
        /// This function adds a tab for hwnd to the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which to add the tab.</param>
        void AddTab(IntPtr hwnd);

        /// <summary>
        /// This function deletes a tab for hwnd from the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be deleted.</param>
        void DeleteTab(IntPtr hwnd);

        /// <summary>
        /// This function activates the tab associated with hwnd on the taskbar.
        /// </summary>
        /// <param name="hwnd">The HWND for which the tab is to be actuvated.</param>
        void ActivateTab(IntPtr hwnd);

        /// <summary>
        /// This function marks hwnd in the taskbar as the active tab.
        /// </summary>
        /// <param name="hwnd">The HWND to activate.</param>
        void SetActiveAlt(IntPtr hwnd);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.TaskbarList2),
    ]
    internal interface ITaskbarList2 : ITaskbarList
    {
        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab(IntPtr hwnd);
        new void DeleteTab(IntPtr hwnd);
        new void ActivateTab(IntPtr hwnd);
        new void SetActiveAlt(IntPtr hwnd);
        #endregion

        /// <summary>
        /// Marks a window as full-screen.
        /// </summary>
        /// <param name="hwnd">The handle of the window to be marked.</param>
        /// <param name="fFullscreen">A Boolean value marking the desired full-screen status of the window.</param>
        /// <remarks>
        /// Setting the value of fFullscreen to true, the Shell treats this window as a full-screen window, and the taskbar
        /// is moved to the bottom of the z-order when this window is active.  Setting the value of fFullscreen to false
        /// removes the full-screen marking, but <i>does not</i> cause the Shell to treat the window as though it were
        /// definitely not full-screen.  With a false fFullscreen value, the Shell depends on its automatic detection facility
        /// to specify how the window should be treated, possibly still flagging the window as full-screen.
        /// </remarks>
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
    }

    // Used to remove items from the automatic destination lists created when apps or the system call SHAddToRecentDocs to report usage of a document.
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ApplicationDestinations)
    ]
    internal interface IApplicationDestinations
    {
        // Set the App User Model ID for the application removing destinations from its list.  If an AppID is not provided 
        // via this method, the system will use a heuristically determined ID.  This method must be called before
        // RemoveDestination or RemoveAllDestinations.
        void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        // Remove an IShellItem or an IShellLink from the automatic destination list
        void RemoveDestination([MarshalAs(UnmanagedType.IUnknown)] object punk);

        // Clear the frequent and recent destination lists for this application.
        void RemoveAllDestinations();
    }

    /// <summary>
    /// Allows an application to retrieve the most recent and frequent documents opened in that app, as reported via SHAddToRecentDocs
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ApplicationDocumentLists)
    ]
    internal interface IApplicationDocumentLists
    {
        /// <summary>
        /// Set the App User Model ID for the application retrieving this list.  If an AppID is not provided via this method,
        /// the system will use a heuristically determined ID.  This method must be called before GetList. 
        /// </summary>
        /// <param name="pszAppID">App Id.</param>
        void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        /// <summary>
        /// Retrieve an IEnumObjects or IObjectArray for IShellItems and/or IShellLinks. 
        /// Items may appear in both the frequent and recent lists.  
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object GetList([In] APPDOCLISTTYPE listtype, [In] uint cItemsDesired, [In] ref Guid riid);
    }

    // Custom Destination List
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.CustomDestinationList)
    ]
    internal interface ICustomDestinationList
    {
        void SetAppID([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

        // Retrieve IObjectArray of IShellItems or IShellLinks that represent removed destinations
        [return: MarshalAs(UnmanagedType.Interface)]
        object BeginList(out uint pcMaxSlots, [In] ref Guid riid);

        // PreserveSig because this will return custom errors when attempting to add unregistered ShellItems.
        // Can't readily detect that case without just trying to append it.
        [PreserveSig]
        HRESULT AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string pszCategory, IObjectArray poa);
        void AppendKnownCategory(KDC category);
        [PreserveSig]
        HRESULT AddUserTasks(IObjectArray poa);
        void CommitList();

        // Retrieve IObjectCollection of IShellItems
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetRemovedDestinations([In] ref Guid riid);
        void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
        void AbortList();
    }

    /// <summary>
    /// Provides access to the App User Model ID on objects supporting this value.
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectWithAppUserModelId)
    ]
    internal interface IObjectWithAppUserModelId
    {
        void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetAppID();
    };

    /// <summary>
    /// Provides access to the ProgID associated with an object 
    /// </summary>
    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ObjectWithProgId)
    ]
    internal interface IObjectWithProgId
    {
        void SetProgID([MarshalAs(UnmanagedType.LPWStr)] string pszProgID);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetProgID();
    };

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.TaskbarList3),
    ]
    internal interface ITaskbarList3 : ITaskbarList2
    {
        #region ITaskbarList2 redeclaration

        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab(IntPtr hwnd);
        new void DeleteTab(IntPtr hwnd);
        new void ActivateTab(IntPtr hwnd);
        new void SetActiveAlt(IntPtr hwnd);
        #endregion

        new void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        #endregion

        [PreserveSig]
        HRESULT SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

        [PreserveSig]
        HRESULT SetProgressState(IntPtr hwnd, TBPF tbpFlags);

        [PreserveSig]
        HRESULT RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

        [PreserveSig]
        HRESULT UnregisterTab(IntPtr hwndTab);

        [PreserveSig]
        HRESULT SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);

        [PreserveSig]
        HRESULT SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);

        [PreserveSig]
        HRESULT ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

        [PreserveSig]
        HRESULT ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);

        [PreserveSig]
        HRESULT ThumbBarSetImageList(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);

        [PreserveSig]
        HRESULT SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

        [PreserveSig]
        HRESULT SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

        // Using RefRECT to making passing NULL possible.  Removes clipping from the HWND.
        [PreserveSig]
        HRESULT SetThumbnailClip(IntPtr hwnd, RefRECT prcClip);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.TaskbarList3),
    ]
    internal interface ITaskbarList4 : ITaskbarList3
    {
        #region ITaskbarList3 redeclaration

        #region ITaskbarList2 redeclaration

        #region ITaskbarList redeclaration
        new void HrInit();
        new void AddTab(IntPtr hwnd);
        new void DeleteTab(IntPtr hwnd);
        new void ActivateTab(IntPtr hwnd);
        new void SetActiveAlt(IntPtr hwnd);
        #endregion

        new void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        #endregion

        [PreserveSig] new  HRESULT SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        [PreserveSig] new HRESULT SetProgressState(IntPtr hwnd, TBPF tbpFlags);
        [PreserveSig] new HRESULT RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        [PreserveSig] new HRESULT UnregisterTab(IntPtr hwndTab);
        [PreserveSig] new HRESULT SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        [PreserveSig] new HRESULT SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);
        [PreserveSig] new HRESULT ThumbBarAddButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);
        [PreserveSig] new HRESULT ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] THUMBBUTTON[] pButtons);
        [PreserveSig] new HRESULT ThumbBarSetImageList(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object himl);
        [PreserveSig] new HRESULT SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        [PreserveSig] new HRESULT SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        // Using RefRECT to making passing NULL possible.  Removes clipping from the HWND.
        [PreserveSig] new HRESULT SetThumbnailClip(IntPtr hwnd, RefRECT prcClip);

        #endregion

        void SetTabProperties(IntPtr hwndTab, STPF stpFlags);
    }

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid(IID.FileDialogEvents),
]
    internal interface IFileDialogEvents
    {
        [PreserveSig]
        HRESULT OnFileOk(IFileDialog pfd);

        [PreserveSig]
        HRESULT OnFolderChanging(IFileDialog pfd, IShellItem psiFolder);

        [PreserveSig]
        HRESULT OnFolderChange(IFileDialog pfd);

        [PreserveSig]
        HRESULT OnSelectionChange(IFileDialog pfd);

        [PreserveSig]
        HRESULT OnShareViolation(IFileDialog pfd, IShellItem psi, out FDESVR pResponse);

        [PreserveSig]
        HRESULT OnTypeChange(IFileDialog pfd);

        [PreserveSig]
        HRESULT OnOverwrite(IFileDialog pfd, IShellItem psi, out FDEOR pResponse);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.ModalWindow),
    ]
    internal interface IModalWindow
    {
        [PreserveSig]
        HRESULT Show(IntPtr parent);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.FileDialog),
    ]
    internal interface IFileDialog : IModalWindow
    {
        #region IModalWindow redeclarations
        [PreserveSig]
        new HRESULT Show(IntPtr parent);
        #endregion

        void SetFileTypes(uint cFileTypes, [In] ref COMDLG_FILTERSPEC rgFilterSpec);

        void SetFileTypeIndex(uint iFileType);

        uint GetFileTypeIndex();

        uint Advise(IFileDialogEvents pfde);

        void Unadvise(uint dwCookie);

        void SetOptions(FOS fos);

        FOS GetOptions();

        void SetDefaultFolder(IShellItem psi);

        void SetFolder(IShellItem psi);

        IShellItem GetFolder();

        IShellItem GetCurrentSelection();

        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetFileName();

        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);

        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        IShellItem GetResult();

        void AddPlace(IShellItem psi, FDAP alignment);

        void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

        void Close([MarshalAs(UnmanagedType.Error)] int hr);

        void SetClientGuid([In] ref Guid guid);

        void ClearClientData();

        void SetFilter([MarshalAs(UnmanagedType.Interface)] object pFilter);
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.FileOpenDialog),
    ]
    internal interface IFileOpenDialog : IFileDialog
    {
        #region IFileDialog redeclarations

        #region IModalDialog redeclarations
        [PreserveSig]
        new HRESULT Show(IntPtr parent);
        #endregion

        new void SetFileTypes(uint cFileTypes, [In] ref COMDLG_FILTERSPEC rgFilterSpec);
        new void SetFileTypeIndex(uint iFileType);
        new uint GetFileTypeIndex();
        new uint Advise(IFileDialogEvents pfde);
        new void Unadvise(uint dwCookie);
        new void SetOptions(FOS fos);
        new FOS GetOptions();
        new void SetDefaultFolder(IShellItem psi);
        new void SetFolder(IShellItem psi);
        new IShellItem GetFolder();
        new IShellItem GetCurrentSelection();
        new void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        new string GetFileName();
        new void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        new void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        new void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        new IShellItem GetResult();
        new void AddPlace(IShellItem psi, FDAP fdcp);
        new void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        new void Close([MarshalAs(UnmanagedType.Error)] int hr);
        new void SetClientGuid([In] ref Guid guid);
        new void ClearClientData();
        new void SetFilter([MarshalAs(UnmanagedType.Interface)] object pFilter);

        #endregion

        IShellItemArray GetResults();

        IShellItemArray GetSelectedItems();
    }

    [
        ComImport,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID.FileSaveDialog),
    ]
    internal interface IFileSaveDialog : IFileDialog
    {
        #region IFileDialog redeclarations

        #region IModalDialog redeclarations
        [PreserveSig]
        new HRESULT Show(IntPtr parent);
        #endregion

        new void SetFileTypes(uint cFileTypes, [In] ref COMDLG_FILTERSPEC rgFilterSpec);
        new void SetFileTypeIndex(uint iFileType);
        new uint GetFileTypeIndex();
        new uint Advise(IFileDialogEvents pfde);
        new void Unadvise(uint dwCookie);
        new void SetOptions(FOS fos);
        new FOS GetOptions();
        new void SetDefaultFolder(IShellItem psi);
        new void SetFolder(IShellItem psi);
        new IShellItem GetFolder();
        new IShellItem GetCurrentSelection();
        new void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        new string GetFileName();
        new void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        new void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        new void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        new IShellItem GetResult();
        new void AddPlace(IShellItem psi, FDAP fdcp);
        new void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        new void Close([MarshalAs(UnmanagedType.Error)] int hr);
        new void SetClientGuid([In] ref Guid guid);
        new void ClearClientData();
        new void SetFilter([MarshalAs(UnmanagedType.Interface)] object pFilter);

        #endregion

        void SetSaveAsItem(IShellItem psi);

        void SetProperties([In, MarshalAs(UnmanagedType.Interface)] object pStore);

        void SetCollectedProperties([In, MarshalAs(UnmanagedType.Interface)] object pList, [In] int fAppendDefault);

        [return: MarshalAs(UnmanagedType.Interface)]
        object GetProperties();

        void ApplyProperties(IShellItem psi, [MarshalAs(UnmanagedType.Interface)] object pStore, [In] ref IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] object pSink);
    }

    internal static class ShellUtil
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetPathFromShellItem(IShellItem item)
        {
            return item.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IShellItem2 GetShellItemForPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // Internal function.  Should have verified this before calling if we cared.
                return null;
            }

            Guid iidShellItem2 = new Guid(IID.ShellItem2);
            object unk;
            HRESULT hr = NativeMethods.SHCreateItemFromParsingName(path, null, ref iidShellItem2, out unk);

            // Silently absorb errors such as ERROR_FILE_NOT_FOUND, ERROR_PATH_NOT_FOUND.
            // Let others pass through
            if (hr == (HRESULT)Win32Error.ERROR_FILE_NOT_FOUND || hr == (HRESULT)Win32Error.ERROR_PATH_NOT_FOUND)
            {
                hr = HRESULT.S_OK;
                unk = null;
            }

            hr.ThrowIfFailed();

            return (IShellItem2)unk;
        }
    }

    #endregion
}
