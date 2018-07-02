// The ComStream class is used for the contact property types.
// The types can have unexpected behavior if they're changed by callers,
// so this provides an immutable stream implementation.
// The volatile functions are implemented (not tested)
// in case a separate ReadonlyStream needs to be implemented.
//#define FEATURE_MUTABLE_COM_STREAMS

namespace Standard
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    // disambiguate with System.Runtime.InteropServices.STATSTG
    using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

    // This is adapted from Microsoft KB article 321340
    /// <summary>
    /// Wraps an IStream interface pointer from COM into a form consumable by .Net.
    /// </summary>
    /// <remarks>
    /// This implementation is immutable, though it's possible that the underlying
    /// stream can be changed in another context.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class ComStream : Stream
    {
        private const int STATFLAG_NONAME = 1;

        private IStream _source;

        private void _Validate()
        {
            if (null == _source)
            {
                throw new ObjectDisposedException("this");
            }
        }

        /// <summary>
        /// Wraps a native IStream interface into a CLR Stream subclass.
        /// </summary>
        /// <param name="stream">
        /// The stream that this object wraps.
        /// </param>
        /// <remarks>
        /// Note that the parameter is passed by ref.  On successful creation it is
        /// zeroed out to the caller.  This object becomes responsible for the lifetime
        /// management of the wrapped IStream.
        /// </remarks>
        public ComStream(ref IStream stream)
        {
            Verify.IsNotNull(stream, "stream");
            _source = stream;
            // Zero out caller's reference to this.  The object now owns the memory.
            stream = null;
        }

        #region Overridden Stream Methods

        // Experimentally, the base class seems to deal with the IDisposable pattern.
        // Overridden implementations aren't called, but Close is as part of the Dispose call.
        public override void Close()
        {
            if (null != _source)
            {
#if FEATURE_MUTABLE_COM_STREAMS
                Flush();
#endif
                Utility.SafeRelease(ref _source);
            }
        }

        public override bool CanRead
        {
            get
            {
                // For the context of this class, this should be true...
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                // This should be true...
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
#if FEATURE_MUTABLE_COM_STREAMS
                // Really don't know that this is true...
                return true;
#endif
                return false;
            }
        }

        public override void Flush()
        {
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();
            // Don't have enough context of the underlying object to reliably do anything here.
            try
            {
                _source.Commit(STGC_DEFAULT);
            }
            catch { }
#endif
        }

        public override long Length
        {
            get
            {
                _Validate();

                STATSTG statstg;
                _source.Stat(out statstg, STATFLAG_NONAME);
                return statstg.cbSize;
            }
        }

        public override long Position
        {
            get { return Seek(0, SeekOrigin.Current); }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _Validate();

            IntPtr pcbRead = IntPtr.Zero;

            try
            {
                pcbRead = Marshal.AllocHGlobal(sizeof(Int32));

                // PERFORMANCE NOTE: This buffer doesn't need to be allocated if offset == 0
                var tempBuffer = new byte[count];
                _source.Read(tempBuffer, count, pcbRead);
                Array.Copy(tempBuffer, 0, buffer, offset, Marshal.ReadInt32(pcbRead));

                return Marshal.ReadInt32(pcbRead);
            }
            finally
            {
                Utility.SafeFreeHGlobal(ref pcbRead);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _Validate();

            IntPtr plibNewPosition = IntPtr.Zero;

            try
            {
                plibNewPosition = Marshal.AllocHGlobal(sizeof(Int64));
                _source.Seek(offset, (int)origin, plibNewPosition);

                return Marshal.ReadInt64(plibNewPosition);
            }
            finally
            {
                Utility.SafeFreeHGlobal(ref plibNewPosition);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();
            _source.SetSize(value);
#endif
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
#if FEATURE_MUTABLE_COM_STREAMS
            _Validate();

            // PERFORMANCE NOTE: This buffer doesn't need to be allocated if offset == 0
            byte[] tempBuffer = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, tempBuffer, 0, tempBuffer.Length);
            _source.Write(tempBuffer, tempBuffer.Length, IntPtr.Zero);
#endif
        }

        #endregion
    }

    // All these methods return void.  Does the standard marshaller convert them to HRESULTs?
    /// <summary>
    /// Wraps a managed stream instance into an interface pointer consumable by COM.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class ManagedIStream : IStream, IDisposable
    {
        private const int STGTY_STREAM = 2;
        private const int STGM_READWRITE = 2;
        private const int LOCK_EXCLUSIVE = 2;

        private Stream _source;

        /// <summary>
        /// Initializes a new instance of the ManagedIStream class with the specified managed Stream object.
        /// </summary>
        /// <param name="source">
        /// The stream that this IStream reference is wrapping.
        /// </param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ManagedIStream(Stream source)
        {
            Verify.IsNotNull(source, "source");
            _source = source;
        }

        private void _Validate()
        {
            if (null == _source)
            {
                throw new ObjectDisposedException("this");
            }
        }

        // Comments are taken from MSDN IStream documentation.
        #region IStream Members

        /// <summary>
        /// Creates a new stream object with its own seek pointer that
        /// references the same bytes as the original stream. 
        /// </summary>
        /// <param name="ppstm">
        /// When this method returns, contains the new stream object. This parameter is passed uninitialized.
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::Clone in the MSDN library.
        /// This class doesn't implement Clone.  A COMException is thrown if it is used.
        /// </remarks>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void Clone(out IStream ppstm)
        {
            ppstm = null;
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        /// <summary>
        /// Ensures that any changes made to a stream object that is open in transacted
        /// mode are reflected in the parent storage. 
        /// </summary>
        /// <param name="grfCommitFlags">
        /// A value that controls how the changes for the stream object are committed. 
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::Commit in the MSDN library.
        /// </remarks>
        public void Commit(int grfCommitFlags)
        {
            _Validate();
            _source.Flush();
        }

        /// <summary>
        /// Copies a specified number of bytes from the current seek pointer in the
        /// stream to the current seek pointer in another stream. 
        /// </summary>
        /// <param name="pstm">
        /// A reference to the destination stream. 
        /// </param>
        /// <param name="cb">
        /// The number of bytes to copy from the source stream. 
        /// </param>
        /// <param name="pcbRead">
        /// On successful return, contains the actual number of bytes read from the source.
        /// (Note the native signature is to a ULARGE_INTEGER*, so 64 bits are written
        /// to this parameter on success.)
        /// </param>
        /// <param name="pcbWritten">
        /// On successful return, contains the actual number of bytes written to the destination.
        /// (Note the native signature is to a ULARGE_INTEGER*, so 64 bits are written
        /// to this parameter on success.)
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            Verify.IsNotNull(pstm, "pstm");

            _Validate();

            // Reasonbly sized buffer, don't try to copy large streams in bulk.
            var buffer = new byte[4096];
            long cbWritten = 0;
            
            while (cbWritten < cb)
            {
                int cbRead = _source.Read(buffer, 0, buffer.Length);
                if (0 == cbRead)
                {
                    break;
                }

                // COM documentation is a bit vague here whether NULL is valid for the third parameter.
                // Going to assume it is, as most implementations I've seen treat it as optional.
                // It's possible this will break on some IStream implementations.
                pstm.Write(buffer, cbRead, IntPtr.Zero);
                cbWritten += cbRead;
            }

            if (IntPtr.Zero != pcbRead)
            {
                Marshal.WriteInt64(pcbRead, cbWritten);
            }

            if (IntPtr.Zero != pcbWritten)
            {
                Marshal.WriteInt64(pcbWritten, cbWritten);
            }
        }

        /// <summary>
        /// Restricts access to a specified range of bytes in the stream. 
        /// </summary>
        /// <param name="libOffset">
        /// The byte offset for the beginning of the range. 
        /// </param>
        /// <param name="cb">
        /// The length of the range, in bytes, to restrict.
        /// </param>
        /// <param name="dwLockType">
        /// The requested restrictions on accessing the range.
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::LockRegion in the MSDN library.
        /// This class doesn't implement LockRegion.  A COMException is thrown if it is used.
        /// </remarks>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)"), Obsolete("The method is not implemented", true)]
        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        /// <summary>
        /// Reads a specified number of bytes from the stream object into memory starting at the current seek pointer. 
        /// </summary>
        /// <param name="pv">
        /// When this method returns, contains the data read from the stream. This parameter is passed uninitialized.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to read from the stream object. 
        /// </param>
        /// <param name="pcbRead">
        /// A pointer to a ULONG variable that receives the actual number of bytes read from the stream object.
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for ISequentialStream::Read in the MSDN library.
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            _Validate();

            int cbRead = _source.Read(pv, 0, cb);

            if (IntPtr.Zero != pcbRead)
            {
                Marshal.WriteInt32(pcbRead, cbRead);
            }
        }


        /// <summary>
        /// Discards all changes that have been made to a transacted stream since the last Commit call.
        /// </summary>
        /// <remarks>
        /// This class doesn't implement Revert.  A COMException is thrown if it is used.
        /// </remarks>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)"), Obsolete("The method is not implemented", true)]
        public void Revert()
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        /// <summary>
        /// Changes the seek pointer to a new location relative to the beginning of the
        /// stream, to the end of the stream, or to the current seek pointer.
        /// </summary>
        /// <param name="dlibMove">
        /// The displacement to add to dwOrigin.
        /// </param>
        /// <param name="dwOrigin">
        /// The origin of the seek. The origin can be the beginning of the file, the current seek pointer, or the end of the file. 
        /// </param>
        /// <param name="plibNewPosition">
        /// On successful return, contains the offset of the seek pointer from the beginning of the stream.
        /// (Note the native signature is to a ULARGE_INTEGER*, so 64 bits are written
        /// to this parameter on success.)
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::Seek in the MSDN library.
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            _Validate();

            long position = _source.Seek(dlibMove, (SeekOrigin)dwOrigin);

            if (IntPtr.Zero != plibNewPosition)
            {
                Marshal.WriteInt64(plibNewPosition, position);
            }
        }

        /// <summary>
        /// Changes the size of the stream object. 
        /// </summary>
        /// <param name="libNewSize">
        /// The new size of the stream as a number of bytes. 
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::SetSize in the MSDN library.
        /// </remarks>
        public void SetSize(long libNewSize)
        {
            _Validate();
            _source.SetLength(libNewSize);
        }

        /// <summary>
        /// Retrieves the STATSTG structure for this stream. 
        /// </summary>
        /// <param name="pstatstg">
        /// When this method returns, contains a STATSTG structure that describes this stream object.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <param name="grfStatFlag">
        /// Members in the STATSTG structure that this method does not return, thus saving some memory allocation operations. 
        /// </param>
        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = default(STATSTG);
            _Validate();

            pstatstg.type = STGTY_STREAM;
            pstatstg.cbSize = _source.Length;
            pstatstg.grfMode = STGM_READWRITE;
            pstatstg.grfLocksSupported = LOCK_EXCLUSIVE;
        }

        /// <summary>
        /// Removes the access restriction on a range of bytes previously restricted with the LockRegion method.
        /// </summary>
        /// <param name="libOffset">The byte offset for the beginning of the range.
        /// </param>
        /// <param name="cb">
        /// The length, in bytes, of the range to restrict.
        /// </param>
        /// <param name="dwLockType">
        /// The access restrictions previously placed on the range.
        /// </param>
        /// <remarks>
        /// For more information, see the existing documentation for IStream::UnlockRegion in the MSDN library.
        /// This class doesn't implement UnlockRegion.  A COMException is thrown if it is used.
        /// </remarks>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Standard.HRESULT.ThrowIfFailed(System.String)")]
        [Obsolete("The method is not implemented", true)]
        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            HRESULT.STG_E_INVALIDFUNCTION.ThrowIfFailed("The method is not implemented.");
        }

        /// <summary>
        /// Writes a specified number of bytes into the stream object starting at the current seek pointer.
        /// </summary>
        /// <param name="pv">
        /// The buffer to write this stream to.
        /// </param>
        /// <param name="cb">
        /// The number of bytes to write to the stream. 
        /// </param>
        /// <param name="pcbWritten">
        /// On successful return, contains the actual number of bytes written to the stream object. 
        /// If the caller sets this pointer to null, this method does not provide the actual number
        /// of bytes written.
        /// </param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            _Validate();

            _source.Write(pv, 0, cb);

            if (IntPtr.Zero != pcbWritten)
            {
                Marshal.WriteInt32(pcbWritten, cb);
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases resources controlled by this object.
        /// </summary>
        /// <remarks>
        /// Dispose can be called multiple times, but trying to use the object
        /// after it has been disposed will generally throw ObjectDisposedExceptions.
        /// </remarks>
        public void Dispose()
        {
            _source = null;
        }

        #endregion
    }

#if CONSIDER_ADDING
    /// <summary>
    /// Wraps an existing stream in a read-only interface.  The stream can still be modified externally.
    /// </summary>
    internal class ReadonlyStream : Stream
    {
        private Stream _stream;

        public ReadonlyStream(Stream source)
        {
            Verify.IsNotNull(source, "source");
            _stream = source;
        }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void Flush() { }

        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("The stream doesn't support modifications.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("The stream doesn't support modifications.");
        }

        public override void Close()
        {
            base.Close();
        }
    }

    /// <summary>
    /// Wraps a string to provide read-only Stream semantics.
    /// </summary>
    internal class StringStream : Stream
    {
        private string _source;
        private int _position;

        public StringStream(string source)
        {
            _source = source;
            _position = 0;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return _source.Length * 2; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                Validate.BoundedInteger(0, (int)value, (int)Length + 1, "value");
                _position = (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int cbRead = 0;
            for (; cbRead < count; ++cbRead)
            {
                if (Length <= Position)
                {
                    break;
                }
                buffer[offset + cbRead] = (byte)(0xFF & (_source[(int)Position / 2] >> ((0 == Position % 2) ? 0 : 8)));
                ++Position;
            }
            return cbRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                default:
                    throw new FormatException("Bad value for origin");
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
#endif
}
