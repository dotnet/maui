namespace Standard
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for common Win32 status codes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Win32Error
    {
        [FieldOffset(0)]
        private readonly int _value;

        // NOTE: These public static field declarations are automatically
        // picked up by (HRESULT's) ToString through reflection.

        /// <summary>The operation completed successfully.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_SUCCESS = new Win32Error(0);
        /// <summary>Incorrect function.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_FUNCTION = new Win32Error(1);
        /// <summary>The system cannot find the file specified.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_FILE_NOT_FOUND = new Win32Error(2);
        /// <summary>The system cannot find the path specified.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_PATH_NOT_FOUND = new Win32Error(3);
        /// <summary>The system cannot open the file.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_TOO_MANY_OPEN_FILES = new Win32Error(4);
        /// <summary>Access is denied.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_ACCESS_DENIED = new Win32Error(5);
        /// <summary>The handle is invalid.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_HANDLE = new Win32Error(6);
        /// <summary>Not enough storage is available to complete this operation.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_OUTOFMEMORY = new Win32Error(14);
        /// <summary>There are no more files.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NO_MORE_FILES = new Win32Error(18);
        /// <summary>The process cannot access the file because it is being used by another process.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_SHARING_VIOLATION = new Win32Error(32);
        /// <summary>The parameter is incorrect.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_PARAMETER = new Win32Error(87);
        /// <summary>The data area passed to a system call is too small.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INSUFFICIENT_BUFFER = new Win32Error(122);
        /// <summary>Cannot nest calls to LoadModule.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NESTING_NOT_ALLOWED = new Win32Error(215);
        /// <summary>Illegal operation attempted on a registry key that has been marked for deletion.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_KEY_DELETED = new Win32Error(1018);
        /// <summary>Element not found.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NOT_FOUND = new Win32Error(1168);
        /// <summary>There was no match for the specified key in the index.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_NO_MATCH = new Win32Error(1169);
        /// <summary>An invalid device was specified.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_BAD_DEVICE = new Win32Error(1200);
        /// <summary>The operation was canceled by the user.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CANCELLED = new Win32Error(1223);
        /// <summary>Cannot find window class.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CANNOT_FIND_WND_CLASS = new Win32Error(1407);
        /// <summary>The window class was already registered.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_CLASS_ALREADY_EXISTS = new Win32Error(1410);
        /// <summary>The specified datatype is invalid.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Win32Error ERROR_INVALID_DATATYPE = new Win32Error(1804);

        /// <summary>
        /// Create a new Win32 error.
        /// </summary>
        /// <param name="i">The integer value of the error.</param>
        public Win32Error(int i)
        {
            _value = i;
        }

        /// <summary>Performs HRESULT_FROM_WIN32 conversion.</summary>
        /// <param name="error">The Win32 error being converted to an HRESULT.</param>
        /// <returns>The equivilent HRESULT value.</returns>
        public static explicit operator HRESULT(Win32Error error)
        {
            // #define __HRESULT_FROM_WIN32(x) 
            //     ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))
            if (error._value <= 0)
            {
                return new HRESULT((uint)error._value);
            }
            return HRESULT.Make(true, Facility.Win32, error._value & 0x0000FFFF);
        }

        // Method version of the cast operation
        /// <summary>Performs HRESULT_FROM_WIN32 conversion.</summary>
        /// <returns>The equivilent HRESULT value.</returns>
        public HRESULT ToHRESULT()
        {
            return (HRESULT)this; 
        }

        /// <summary>Performs the equivalent of Win32's GetLastError()</summary>
        /// <returns>A Win32Error instance with the result of the native GetLastError</returns>
        public static Win32Error GetLastError()
        {
            return new Win32Error(Marshal.GetLastWin32Error());
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((Win32Error)obj)._value == _value;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Compare two Win32 error codes for equality.
        /// </summary>
        /// <param name="errLeft">The first error code to compare.</param>
        /// <param name="errRight">The second error code to compare.</param>
        /// <returns>Whether the two error codes are the same.</returns>
        public static bool operator ==(Win32Error errLeft, Win32Error errRight)
        {
            return errLeft._value == errRight._value;
        }

        /// <summary>
        /// Compare two Win32 error codes for inequality.
        /// </summary>
        /// <param name="errLeft">The first error code to compare.</param>
        /// <param name="errRight">The second error code to compare.</param>
        /// <returns>Whether the two error codes are not the same.</returns>
        public static bool operator !=(Win32Error errLeft, Win32Error errRight)
        {
            return !(errLeft == errRight);
        }
    }

    internal enum Facility
    {
        /// <summary>FACILITY_NULL</summary>
        Null = 0,
        /// <summary>FACILITY_RPC</summary>
        Rpc = 1,
        /// <summary>FACILITY_DISPATCH</summary>
        Dispatch = 2,
        /// <summary>FACILITY_STORAGE</summary>
        Storage = 3,
        /// <summary>FACILITY_ITF</summary>
        Itf = 4,
        /// <summary>FACILITY_WIN32</summary>
        Win32 = 7,
        /// <summary>FACILITY_WINDOWS</summary>
        Windows = 8,
        /// <summary>FACILITY_CONTROL</summary>
        Control = 10,
        /// <summary>MSDN doced facility code for ESE errors.</summary>
        Ese = 0xE5E,
        /// <summary>FACILITY_WINCODEC (WIC)</summary>
        WinCodec = 0x898,
    }

    /// <summary>Wrapper for HRESULT status codes.</summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct HRESULT
    {
        [FieldOffset(0)]
        private readonly uint _value;

        // NOTE: These public static field declarations are automatically
        // picked up by ToString through reflection.
        /// <summary>S_OK</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT S_OK = new HRESULT(0x00000000);
        /// <summary>S_FALSE</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT S_FALSE = new HRESULT(0x00000001);
        /// <summary>E_PENDING</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_PENDING = new HRESULT(0x8000000A);
        /// <summary>E_NOTIMPL</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_NOTIMPL = new HRESULT(0x80004001);
        /// <summary>E_NOINTERFACE</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_NOINTERFACE = new HRESULT(0x80004002);
        /// <summary>E_POINTER</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_POINTER = new HRESULT(0x80004003);
        /// <summary>E_ABORT</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_ABORT = new HRESULT(0x80004004);
        /// <summary>E_FAIL</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_FAIL = new HRESULT(0x80004005);
        /// <summary>E_UNEXPECTED</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_UNEXPECTED = new HRESULT(0x8000FFFF);
        /// <summary>STG_E_INVALIDFUNCTION</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT STG_E_INVALIDFUNCTION = new HRESULT(0x80030001);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT OLE_E_ADVISENOTSUPPORTED = new HRESULT(0x80040003);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_FORMATETC = new HRESULT(0x80040064);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_TYMED = new HRESULT(0x80040069);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_CLIPFORMAT = new HRESULT(0x8004006A);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DV_E_DVASPECT = new HRESULT(0x8004006B);

        /// <summary>REGDB_E_CLASSNOTREG</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT REGDB_E_CLASSNOTREG = new HRESULT(0x80040154);

        /// <summary>DESTS_E_NO_MATCHING_ASSOC_HANDLER.  Win7 internal error code for Jump Lists.</summary>
        /// <remarks>There is no Assoc Handler for the given item registered by the specified application.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NO_MATCHING_ASSOC_HANDLER = new HRESULT(0x80040F03);
        /// <summary>DESTS_E_NORECDOCS.  Win7 internal error code for Jump Lists.</summary>
        /// <remarks>The given item is excluded from the recent docs folder by the NoRecDocs bit on its registration.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NORECDOCS = new HRESULT(0x80040F04);
        /// <summary>DESTS_E_NOTALLCLEARED.  Win7 internal error code for Jump Lists.</summary>
        /// <remarks>Not all of the items were successfully cleared</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT DESTS_E_NOTALLCLEARED = new HRESULT(0x80040F05);

        /// <summary>E_ACCESSDENIED</summary>
        /// <remarks>Win32Error ERROR_ACCESS_DENIED.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_ACCESSDENIED = new HRESULT(0x80070005);
        /// <summary>E_OUTOFMEMORY</summary>
        /// <remarks>Win32Error ERROR_OUTOFMEMORY.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_OUTOFMEMORY = new HRESULT(0x8007000E);
        /// <summary>E_INVALIDARG</summary>
        /// <remarks>Win32Error ERROR_INVALID_PARAMETER.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT E_INVALIDARG = new HRESULT(0x80070057);
        /// <summary>INTSAFE_E_ARITHMETIC_OVERFLOW</summary>
        public static readonly HRESULT INTSAFE_E_ARITHMETIC_OVERFLOW = new HRESULT(0x80070216);
        /// <summary>COR_E_OBJECTDISPOSED</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT COR_E_OBJECTDISPOSED = new HRESULT(0x80131622);
        /// <summary>WC_E_GREATERTHAN</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WC_E_GREATERTHAN = new HRESULT(0xC00CEE23);
        /// <summary>WC_E_SYNTAX</summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WC_E_SYNTAX = new HRESULT(0xC00CEE2D);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_GENERIC_ERROR = E_FAIL;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDPARAMETER = E_INVALIDARG;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_OUTOFMEMORY = E_OUTOFMEMORY;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_NOTIMPLEMENTED = E_NOTIMPL;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ABORTED = E_ABORT;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ACCESSDENIED = E_ACCESSDENIED;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_VALUEOVERFLOW = INTSAFE_E_ARITHMETIC_OVERFLOW;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_WRONGSTATE = Make(true, Facility.WinCodec, 0x2f04);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_VALUEOUTOFRANGE = Make(true, Facility.WinCodec, 0x2f05);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNKNOWNIMAGEFORMAT = Make(true, Facility.WinCodec, 0x2f07);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDVERSION = Make(true, Facility.WinCodec, 0x2f0B);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_NOTINITIALIZED = Make(true, Facility.WinCodec, 0x2f0C);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_ALREADYLOCKED = Make(true, Facility.WinCodec, 0x2f0D);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYNOTFOUND = Make(true, Facility.WinCodec, 0x2f40);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYNOTSUPPORTED = Make(true, Facility.WinCodec, 0x2f41);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYSIZE = Make(true, Facility.WinCodec, 0x2f42);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECPRESENT = Make(true, Facility.WinCodec, 0x2f43);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECNOTHUMBNAIL = Make(true, Facility.WinCodec, 0x2f44);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PALETTEUNAVAILABLE = Make(true, Facility.WinCodec, 0x2f45);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_CODECTOOMANYSCANLINES = Make(true, Facility.WinCodec, 0x2f46);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INTERNALERROR = Make(true, Facility.WinCodec, 0x2f48);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_SOURCERECTDOESNOTMATCHDIMENSIONS = Make(true, Facility.WinCodec, 0x2f49);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_COMPONENTNOTFOUND = Make(true, Facility.WinCodec, 0x2f50);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_IMAGESIZEOUTOFRANGE = Make(true, Facility.WinCodec, 0x2f51);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_TOOMUCHMETADATA = Make(true, Facility.WinCodec, 0x2f52);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADIMAGE = Make(true, Facility.WinCodec, 0x2f60);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADHEADER = Make(true, Facility.WinCodec, 0x2f61);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_FRAMEMISSING = Make(true, Facility.WinCodec, 0x2f62);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADMETADATAHEADER = Make(true, Facility.WinCodec, 0x2f63);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_BADSTREAMDATA = Make(true, Facility.WinCodec, 0x2f70);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMWRITE = Make(true, Facility.WinCodec, 0x2f71);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMREAD = Make(true, Facility.WinCodec, 0x2f72);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_STREAMNOTAVAILABLE = Make(true, Facility.WinCodec, 0x2f73);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDPIXELFORMAT = Make(true, Facility.WinCodec, 0x2f80);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNSUPPORTEDOPERATION = Make(true, Facility.WinCodec, 0x2f81);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDREGISTRATION = Make(true, Facility.WinCodec, 0x2f8A);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_COMPONENTINITIALIZEFAILURE = Make(true, Facility.WinCodec, 0x2f8B);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INSUFFICIENTBUFFER = Make(true, Facility.WinCodec, 0x2f8C);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_DUPLICATEMETADATAPRESENT = Make(true, Facility.WinCodec, 0x2f8D);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_PROPERTYUNEXPECTEDTYPE = Make(true, Facility.WinCodec, 0x2f8E);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNEXPECTEDSIZE = Make(true, Facility.WinCodec, 0x2f8F);

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDQUERYREQUEST = Make(true, Facility.WinCodec, 0x2f90);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_UNEXPECTEDMETADATATYPE = Make(true, Facility.WinCodec, 0x2f91);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_REQUESTONLYVALIDATMETADATAROOT = Make(true, Facility.WinCodec, 0x2f92);
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly HRESULT WINCODEC_ERR_INVALIDQUERYCHARACTER = Make(true, Facility.WinCodec, 0x2f93);

        /// <summary>
        /// Create an HRESULT from an integer value.
        /// </summary>
        /// <param name="i"></param>
        public HRESULT(uint i)
        {
            _value = i;
        }

        public HRESULT(int i)
        {
            _value = unchecked((uint)i);
        }

        /// <summary>
        /// Convert an HRESULT to an int.  Used for COM interface declarations out of our control.
        /// </summary>
        public static explicit operator int(HRESULT hr)
        {
            unchecked
            {
                return (int)hr._value;
            }
        }

        public static HRESULT Make(bool severe, Facility facility, int code)
        {
            //#define MAKE_HRESULT(sev,fac,code) \
            //    ((HRESULT) (((unsigned long)(sev)<<31) | ((unsigned long)(fac)<<16) | ((unsigned long)(code))) )

            // Severity has 1 bit reserved.
            // bitness is enforced by the boolean parameter.

            // Facility has 11 bits reserved (different than SCODES, which have 4 bits reserved)
            // MSDN documentation incorrectly uses 12 bits for the ESE facility (e5e), so go ahead and let that one slide.
            // And WIC also ignores it the documented size...
            Assert.Implies((int)facility != (int)((int)facility & 0x1FF), facility == Facility.Ese || facility == Facility.WinCodec);
            // Code has 4 bits reserved.
            Assert.AreEqual(code, code & 0xFFFF);

            return new HRESULT((uint)((severe ? (1 << 31) : 0) | ((int)facility << 16) | code));
        }

        /// <summary>
        /// retrieve HRESULT_FACILITY
        /// </summary>
        public Facility Facility
        {
            get
            {
                return GetFacility((int)_value);
            }
        }

        public static Facility GetFacility(int errorCode)
        {
            // #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
            return (Facility)((errorCode >> 16) & 0x1fff);
        }

        /// <summary>
        /// retrieve HRESULT_CODE
        /// </summary>
        public int Code
        {
            get
            {
                return GetCode((int)_value);
            }
        }

        public static int GetCode(int error)
        {
            // #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
            return (int)(error & 0xFFFF);
        }

        #region Object class override members

        /// <summary>
        /// Get a string representation of this HRESULT.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Use reflection to try to name this HRESULT.
            // This is expensive, but if someone's ever printing HRESULT strings then
            // I think it's a fair guess that they're not in a performance critical area
            // (e.g. printing exception strings).
            // This is less error prone than trying to keep the list in the function.
            // To properly add an HRESULT's name to the ToString table, just add the HRESULT
            // like all the others above.
            //
            // CONSIDER: This data is static.  It could be cached 
            // after first usage for fast lookup since the keys are unique.
            //
            foreach (FieldInfo publicStaticField in typeof(HRESULT).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (publicStaticField.FieldType == typeof(HRESULT))
                {
                    var hr = (HRESULT)publicStaticField.GetValue(null);
                    if (hr == this)
                    {
                        return publicStaticField.Name;
                    }
                }
            }

            // Try Win32 error codes also
            if (Facility == Facility.Win32)
            {
                foreach (FieldInfo publicStaticField in typeof(Win32Error).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (publicStaticField.FieldType == typeof(Win32Error))
                    {
                        var error = (Win32Error)publicStaticField.GetValue(null);
                        if ((HRESULT)error == this)
                        {
                            return "HRESULT_FROM_WIN32(" + publicStaticField.Name + ")";
                        }
                    }
                }
            }

            // If there's no good name for this HRESULT,
            // return the string as readable hex (0x########) format.
            return string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", _value);
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((HRESULT)obj)._value == _value;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        public static bool operator ==(HRESULT hrLeft, HRESULT hrRight)
        {
            return hrLeft._value == hrRight._value;
        }

        public static bool operator !=(HRESULT hrLeft, HRESULT hrRight)
        {
            return !(hrLeft == hrRight);
        }

        public bool Succeeded
        {
            get { return (int)_value >= 0; }
        }

        public bool Failed
        {
            get { return (int)_value < 0; }
        }

        public void ThrowIfFailed()
        {
            ThrowIfFailed(null);
        }

        [
            SuppressMessage(
                "Microsoft.Usage",
                "CA2201:DoNotRaiseReservedExceptionTypes",
                Justification="Only recreating Exceptions that were already raised."),
            SuppressMessage(
                "Microsoft.Security",
                "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")
        ]
        public void ThrowIfFailed(string message)
        {
            if (Failed)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = ToString();
                }
#if DEBUG
                else
                {
                    message += " (" + ToString() + ")";
                }
#endif
                // Wow.  Reflection in a throw call.  Later on this may turn out to have been a bad idea.
                // If you're throwing an exception I assume it's OK for me to take some time to give it back.
                // I want to convert the HRESULT to a more appropriate exception type than COMException.
                // Marshal.ThrowExceptionForHR does this for me, but the general call uses GetErrorInfo
                // if it's set, and then ignores the HRESULT that I've provided.  This makes it so this
                // call works the first time but you get burned on the second.  To avoid this, I use
                // the overload that explicitly ignores the IErrorInfo.
                // In addition, the function doesn't allow me to set the Message unless I go through
                // the process of implementing an IErrorInfo and then use that.  There's no stock
                // implementations of IErrorInfo available and I don't think it's worth the maintenance
                // overhead of doing it, nor would it have significant value over this approach.
                Exception e = Marshal.GetExceptionForHR((int)_value, new IntPtr(-1));
                Assert.IsNotNull(e);
                // ArgumentNullException doesn't have the right constructor parameters,
                // (nor does Win32Exception...)
                // but E_POINTER gets mapped to NullReferenceException,
                // so I don't think it will ever matter.
                Assert.IsFalse(e is ArgumentNullException);

                // If we're not getting anything better than a COMException from Marshal,
                // then at least check the facility and attempt to do better ourselves.
                if (e.GetType() == typeof(COMException))
                {
                    switch (Facility)
                    {
                        case Facility.Win32:
                            e = new Win32Exception(Code, message);
                            break;
                        default:
                            e = new COMException(message, (int)_value);
                            break;
                    }
                }
                else
                {
                    ConstructorInfo cons = e.GetType().GetConstructor(new[] { typeof(string) });
                    if (null != cons)
                    {
                        e = cons.Invoke(new object[] { message }) as Exception;
                        Assert.IsNotNull(e);
                    }
                }
                throw e;
            }
        }

        /// <summary>
        /// Convert the result of Win32 GetLastError() into a raised exception.
        /// </summary>
        public static void ThrowLastError()
        {
            ((HRESULT)Win32Error.GetLastError()).ThrowIfFailed();
            // Only expecting to call this when we're expecting a failed GetLastError()
            Assert.Fail();
        }
    }
}