// TarInputStream.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;
using System.Text;

namespace SWF.NET.ZIP.Tar;

/// <summary>
///     The TarInputStream reads a UNIX tar archive as an InputStream.
///     methods are provided to position at each successive entry in
///     the archive, and the read each entry as a normal input stream
///     using read().
/// </summary>
public class TarInputStream : Stream
{
	/// <summary>
	///     Working buffer
	/// </summary>
	protected TarBuffer buffer;

	/// <summary>
	///     Current entry being read
	/// </summary>
	protected TarEntry currEntry;

	/// <summary>
	///     Internal debugging flag
	/// </summary>
	protected bool debug;

	/// <summary>
	///     Factory used to create TarEntry or descendant class instance
	/// </summary>
	protected IEntryFactory eFactory;

	/// <summary>
	///     Number of bytes read for this entry so far
	/// </summary>
	protected int entryOffset;

	/// <summary>
	///     Size of this entry as recorded in header
	/// </summary>
	protected int entrySize;

	/// <summary>
	///     Flag set when last block has been read
	/// </summary>
	protected bool hasHitEOF;

    private readonly Stream inputStream;

    /// <summary>
    ///     Buffer used with calls to <code>Read()</code>
    /// </summary>
    protected byte[] readBuf;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports reading
    /// </summary>
    public override bool CanRead =>
        inputStream.CanRead;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports seeking
    ///     This property always returns false.
    /// </summary>
    public override bool CanSeek =>
        //				return inputStream.CanSeek;
        false;

    /// <summary>
    ///     Gets a value indicating if the stream supports writing.
    ///     This property always returns false.
    /// </summary>
    public override bool CanWrite =>
        //				return inputStream.CanWrite;
        false;

    /// <summary>
    ///     The length in bytes of the stream
    /// </summary>
    public override long Length =>
        inputStream.Length;

    /// <summary>
    ///     Gets or sets the position within the stream.
    ///     Setting the Position is not supported and throws a NotSupportedExceptionNotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any attempt to set position</exception>
    public override long Position
    {
        get => inputStream.Position;
        set =>
            throw
                //				inputStream.Position = value;
                new NotSupportedException("TarInputStream Seek not supported");
    }

    /// <summary>
    ///     Get the available data that can be read from the current
    ///     entry in the archive. This does not indicate how much data
    ///     is left in the entire archive, only in the current entry.
    ///     This value is determined from the entry's size header field
    ///     and the amount of data already read from the current entry.
    /// </summary>
    /// <returns>
    ///     The number of available bytes for the current entry.
    /// </returns>
    public int Available =>
        entrySize - entryOffset;

    /// <summary>
    ///     Since we do not support marking just yet, we return false.
    /// </summary>
    public bool IsMarkSupported =>
        false;


    /// <summary>
    ///     Construct a TarInputStream with default block factor
    /// </summary>
    /// <param name="inputStream">stream to source data from</param>
    public TarInputStream(Stream inputStream) : this(inputStream, TarBuffer.DefaultBlockFactor)
    {
    }

    /// <summary>
    ///     Construct a TarInputStream with user specified block factor
    /// </summary>
    /// <param name="inputStream">stream to source data from</param>
    /// <param name="blockFactor">block factor to apply to archive</param>
    public TarInputStream(Stream inputStream, int blockFactor)
    {
        this.inputStream = inputStream;
        buffer = TarBuffer.CreateInputTarBuffer(inputStream, blockFactor);

        readBuf = null;
        debug = false;
        hasHitEOF = false;
        eFactory = null;
    }

    /// <summary>
    ///     Flushes the baseInputStream
    /// </summary>
    public override void Flush() =>
        inputStream.Flush();

    /// <summary>
    ///     Set the streams position.  This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override long Seek(long offset, SeekOrigin origin) =>
        //			return inputStream.Seek(offset, origin);
        throw new NotSupportedException("TarInputStream Seek not supported");

    /// <summary>
    ///     Sets the length of the stream
    ///     This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void SetLength(long val) =>
        //			inputStream.SetLength(val);
        throw new NotSupportedException("TarInputStream SetLength not supported");

    /// <summary>
    ///     Writes a block of bytes to this stream using data from a buffer.
    ///     This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void Write(byte[] array, int offset, int count) =>
        //			inputStream.Write(array, offset, count);
        throw new NotSupportedException("TarInputStream Write not supported");

    /// <summary>
    ///     Writes a byte to the current position in the file stream.
    ///     This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void WriteByte(byte val) =>
        //			inputStream.WriteByte(val);
        throw new NotSupportedException("TarInputStream WriteByte not supported");

    /// <summary>
    ///     set debug flag both locally and for buffer
    /// </summary>
    /// <param name="debugFlag">debug on or off</param>
    public void SetDebug(bool debugFlag)
    {
        debug = debugFlag;
        SetBufferDebug(debugFlag);
    }

    /// <summary>
    ///     set debug flag for buffer
    /// </summary>
    /// <param name="debug">debug on or off</param>
    public void SetBufferDebug(bool debug) =>
        buffer.SetDebug(debug);

    /// <summary>
    ///     Set the entry factory for this instance.
    /// </summary>
    /// <param name="factory">The factory for creating new entries</param>
    public void SetEntryFactory(IEntryFactory factory) =>
        eFactory = factory;

    /// <summary>
    ///     Closes this stream. Calls the TarBuffer's close() method.
    ///     The underlying stream is closed by the TarBuffer.
    /// </summary>
    public override void Close() =>
        buffer.Close();

    /// <summary>
    ///     Get the record size being used by this stream's TarBuffer.
    /// </summary>
    /// <returns>
    ///     TarBuffer record size.
    /// </returns>
    public int GetRecordSize() =>
        buffer.GetRecordSize();

    /// <summary>
    ///     Skip bytes in the input buffer. This skips bytes in the
    ///     current entry's data, not the entire archive, and will
    ///     stop at the end of the current entry's data if the number
    ///     to skip extends beyond that point.
    /// </summary>
    /// <param name="numToSkip">
    ///     The number of bytes to skip.
    /// </param>
    public void Skip(int numToSkip)
    {
        // TODO: REVIEW
        // This is horribly inefficient, but it ensures that we
        // properly skip over bytes via the TarBuffer...
        //
        var skipBuf = new byte[8 * 1024];

        for (var num = numToSkip; num > 0;)
        {
            var numRead = Read(skipBuf, 0, num > skipBuf.Length ? skipBuf.Length : num);

            if (numRead == -1) break;

            num -= numRead;
        }
    }

    /// <summary>
    ///     Since we do not support marking just yet, we do nothing.
    /// </summary>
    /// <param name="markLimit">
    ///     The limit to mark.
    /// </param>
    public void Mark(int markLimit)
    {
    }

    /// <summary>
    ///     Since we do not support marking just yet, we do nothing.
    /// </summary>
    public void Reset()
    {
    }

    private void SkipToNextEntry()
    {
        var numToSkip = entrySize - entryOffset;

        if (debug)
        {
            //Console.WriteLine.WriteLine("TarInputStream: SKIP currENTRY '" + this.currEntry.Name + "' SZ " + this.entrySize + " OFF " + this.entryOffset + "  skipping " + numToSkip + " bytes");
        }

        if (numToSkip > 0) Skip(numToSkip);

        readBuf = null;
    }

    /// <summary>
    ///     Get the next entry in this tar archive. This will skip
    ///     over any remaining data in the current entry, if there
    ///     is one, and place the input stream at the header of the
    ///     next entry, and read the header and instantiate a new
    ///     TarEntry from the header bytes and return that entry.
    ///     If there are no more entries in the archive, null will
    ///     be returned to indicate that the end of the archive has
    ///     been reached.
    /// </summary>
    /// <returns>
    ///     The next TarEntry in the archive, or null.
    /// </returns>
    public TarEntry GetNextEntry()
    {
        if (hasHitEOF) return null;

        if (currEntry != null) SkipToNextEntry();

        var headerBuf = buffer.ReadBlock();

        if (headerBuf == null)
        {
            if (debug)
            {
                //Console.WriteLine.WriteLine("READ NULL BLOCK");
            }

            hasHitEOF = true;
        }
        else if (buffer.IsEOFBlock(headerBuf))
        {
            if (debug)
            {
                //Console.WriteLine.WriteLine( "READ EOF BLOCK" );
            }

            hasHitEOF = true;
        }

        if (hasHitEOF)
            currEntry = null;
        else
            try
            {
                var header = new TarHeader();
                header.ParseBuffer(headerBuf);
                entryOffset = 0;
                entrySize = (int)header.size;

                StringBuilder longName = null;

                if (header.typeFlag == TarHeader.LF_GNU_LONGNAME)
                {
                    var nameBuffer = new byte[TarBuffer.BlockSize];

                    var numToRead = entrySize;

                    longName = new StringBuilder();

                    while (numToRead > 0)
                    {
                        var numRead = Read(nameBuffer, 0, numToRead > nameBuffer.Length ? nameBuffer.Length : numToRead);

                        if (numRead == -1) throw new InvalidHeaderException("Failed to read long name entry");

                        longName.Append(TarHeader.ParseName(nameBuffer, 0, numRead).ToString());
                        numToRead -= numRead;
                    }

                    SkipToNextEntry();
                    headerBuf = buffer.ReadBlock();
                }
                else if (header.typeFlag == TarHeader.LF_GHDR)
                {
                    // POSIX global extended header 
                    // Ignore things we dont understand completely for now
                    SkipToNextEntry();
                    headerBuf = buffer.ReadBlock();
                }
                else if (header.typeFlag == TarHeader.LF_XHDR)
                {
                    // POSIX extended header
                    // Ignore things we dont understand completely for now
                    SkipToNextEntry();
                    headerBuf = buffer.ReadBlock();
                }
                else if (header.typeFlag == TarHeader.LF_GNU_VOLHDR)
                {
                    // TODO  could show volume name when verbose?
                    SkipToNextEntry();
                    headerBuf = buffer.ReadBlock();
                }
                else if (header.typeFlag != TarHeader.LF_NORMAL &&
                         header.typeFlag != TarHeader.LF_OLDNORM &&
                         header.typeFlag != TarHeader.LF_DIR)
                {
                    // Ignore things we dont understand completely for now
                    SkipToNextEntry();
                    headerBuf = buffer.ReadBlock();
                }

                if (eFactory == null)
                {
                    currEntry = new TarEntry(headerBuf);
                    if (longName != null)
                    {
                        currEntry.TarHeader.name.Length = 0;
                        currEntry.TarHeader.name.Append(longName.ToString());
                    }
                }
                else
                    currEntry = eFactory.CreateEntry(headerBuf);

                // TODO  ustar is not the only magic possible by any means
                // tar, xtar, ...
                if (!(headerBuf[257] == 'u' && headerBuf[258] == 's' && headerBuf[259] == 't' && headerBuf[260] == 'a' && headerBuf[261] == 'r'))
                    throw new InvalidHeaderException(
                        "header magic is not 'ustar', but '" +
                        headerBuf[257] +
                        headerBuf[258] +
                        headerBuf[259] +
                        headerBuf[260] +
                        headerBuf[261] +
                        "', or (dec) " +
                        (int)headerBuf[257] +
                        ", " +
                        (int)headerBuf[258] +
                        ", " +
                        (int)headerBuf[259] +
                        ", " +
                        (int)headerBuf[260] +
                        ", " +
                        (int)headerBuf[261]);

                if (debug)
                {
                    //Console.WriteLine.WriteLine("TarInputStream: SET CURRENTRY '" + this.currEntry.Name + "' size = " + this.currEntry.Size);
                }

                entryOffset = 0;

                // TODO  Review How do we resolve this discrepancy?!
                entrySize = (int)currEntry.Size;
            }
            catch (InvalidHeaderException ex)
            {
                entrySize = 0;
                entryOffset = 0;
                currEntry = null;
                throw new InvalidHeaderException("bad header in record " + buffer.GetCurrentBlockNum() + " block " + buffer.GetCurrentBlockNum() + ", " + ex.Message);
            }

        return currEntry;
    }

    /// <summary>
    ///     Reads a byte from the current tar archive entry.
    ///     This method simply calls read(byte[], int, int).
    /// </summary>
    public override int ReadByte()
    {
        var oneByteBuffer = new byte[1];
        var num = Read(oneByteBuffer, 0, 1);
        if (num <= 0) // return -1 to indicate that no byte was read.
            return -1;
        return oneByteBuffer[0];
    }

    /// <summary>
    ///     Reads bytes from the current tar archive entry.
    ///     This method is aware of the boundaries of the current
    ///     entry in the archive and will deal with them appropriately
    /// </summary>
    /// <param name="outputBuffer">
    ///     The buffer into which to place bytes read.
    /// </param>
    /// <param name="offset">
    ///     The offset at which to place bytes read.
    /// </param>
    /// <param name="numToRead">
    ///     The number of bytes to read.
    /// </param>
    /// <returns>
    ///     The number of bytes read, or 0 at end of stream/EOF.
    /// </returns>
    public override int Read(byte[] outputBuffer, int offset, int numToRead)
    {
        var totalRead = 0;

        if (entryOffset >= entrySize) return 0;

        if (numToRead + entryOffset > entrySize) numToRead = entrySize - entryOffset;

        if (readBuf != null)
        {
            var sz = numToRead > readBuf.Length ? readBuf.Length : numToRead;

            Array.Copy(readBuf, 0, outputBuffer, offset, sz);

            if (sz >= readBuf.Length)
                readBuf = null;
            else
            {
                var newLen = readBuf.Length - sz;
                var newBuf = new byte[newLen];
                Array.Copy(readBuf, sz, newBuf, 0, newLen);
                readBuf = newBuf;
            }

            totalRead += sz;
            numToRead -= sz;
            offset += sz;
        }

        while (numToRead > 0)
        {
            var rec = buffer.ReadBlock();
            if (rec == null)
                // Unexpected EOF!
                throw new IOException("unexpected EOF with " + numToRead + " bytes unread");

            var sz = numToRead;
            var recLen = rec.Length;

            if (recLen > sz)
            {
                Array.Copy(rec, 0, outputBuffer, offset, sz);
                readBuf = new byte[recLen - sz];
                Array.Copy(rec, sz, readBuf, 0, recLen - sz);
            }
            else
            {
                sz = recLen;
                Array.Copy(rec, 0, outputBuffer, offset, recLen);
            }

            totalRead += sz;
            numToRead -= sz;
            offset += sz;
        }

        entryOffset += totalRead;

        return totalRead;
    }

    /// <summary>
    ///     Copies the contents of the current tar archive entry directly into
    ///     an output stream.
    /// </summary>
    /// <param name="outputStream">
    ///     The OutputStream into which to write the entry's data.
    /// </param>
    public void CopyEntryContents(Stream outputStream)
    {
        var buf = new byte[32 * 1024];

        while (true)
        {
            var numRead = Read(buf, 0, buf.Length);
            if (numRead <= 0) break;
            outputStream.Write(buf, 0, numRead);
        }
    }

    /// <summary>
    ///     This interface is provided, along with the method setEntryFactory(), to allow
    ///     the programmer to have their own TarEntry subclass instantiated for the
    ///     entries return from getNextEntry().
    /// </summary>
    public interface IEntryFactory
    {
	    /// <summary>
	    ///     Create an entry based on name alone
	    /// </summary>
	    /// <param name="name">
	    ///     Name of the new EntryPointNotFoundException to create
	    /// </param>
	    /// <returns>created TarEntry or descendant class</returns>
	    TarEntry CreateEntry(string name);

	    /// <summary>
	    ///     Create an instance based on an actual file
	    /// </summary>
	    /// <param name="fileName">
	    ///     Name of file to represent in the entry
	    /// </param>
	    /// <returns>
	    ///     Created TarEntry or descendant class
	    /// </returns>
	    TarEntry CreateEntryFromFile(string fileName);

	    /// <summary>
	    ///     Create a tar entry based on the header information passed
	    /// </summary>
	    /// <param name="headerBuf">
	    ///     Buffer containing header information to base entry on
	    /// </param>
	    /// <returns>
	    ///     Created TarEntry or descendant class
	    /// </returns>
	    TarEntry CreateEntry(byte[] headerBuf);
    }

    /// <summary>
    ///     Standard entry factory class creating instances of the class TarEntry
    /// </summary>
    public class EntryFactoryAdapter : IEntryFactory
    {
	    /// <summary>
	    ///     Create a TarEntry based on named
	    /// </summary>
	    public TarEntry CreateEntry(string name) =>
            TarEntry.CreateTarEntry(name);

	    /// <summary>
	    ///     Create a tar entry with details obtained from <paramref name="fileName">file</paramref>
	    /// </summary>
	    public TarEntry CreateEntryFromFile(string fileName) =>
            TarEntry.CreateEntryFromFile(fileName);

	    /// <summary>
	    ///     Create and entry based on details in <paramref name="headerBuf">header</paramref>
	    /// </summary>
	    public TarEntry CreateEntry(byte[] headerBuf) =>
            new TarEntry(headerBuf);
    }
}


/* The original Java file had this header:
	** Authored by Timothy Gerard Endres
	** <mailto:time@gjt.org>  <http://www.trustice.com>
	**
	** This work has been placed into the public domain.
	** You may use this work in any way and for any purpose you wish.
	**
	** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
	** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
	** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
	** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
	** REDISTRIBUTION OF THIS SOFTWARE.
	**
	*/