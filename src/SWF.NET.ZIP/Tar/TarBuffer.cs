// TarBuffer.cs
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

namespace SWF.NET.ZIP.Tar;

/// <summary>
///     The TarBuffer class implements the tar archive concept
///     of a buffered input stream. This concept goes back to the
///     days of blocked tape drives and special io devices. In the
///     C# universe, the only real function that this class
///     performs is to ensure that files have the correct "record"
///     size, or other tars will complain.
///     <p>
///         You should never have a need to access this class directly.
///         TarBuffers are created by Tar IO Streams.
///     </p>
/// </summary>
public class TarBuffer
{
/* A quote from GNU tar man file on blocking and records
   A `tar' archive file contains a series of blocks.  Each block
contains `BLOCKSIZE' bytes.  Although this format may be thought of as
being on magnetic tape, other media are often used.

   Each file archived is represented by a header block which describes
the file, followed by zero or more blocks which give the contents of
the file.  At the end of the archive file there may be a block filled
with binary zeros as an end-of-file marker.  A reasonable system should
write a block of zeros at the end, but must not assume that such a
block exists when reading an archive.

   The blocks may be "blocked" for physical I/O operations.  Each
record of N blocks (where N is set by the `--blocking-factor=512-SIZE'
(`-b 512-SIZE') option to `tar') is written with a single `write ()'
operation.  On magnetic tapes, the result of such a write is a single
record.  When writing an archive, the last record of blocks should be
written at the full size, with blocks after the zero block containing
all zeros.  When reading an archive, a reasonable system should
properly handle an archive whose last record is shorter than the rest,
or which contains garbage records after a zero block.
*/

/// <summary>
///     The size of a block in a tar archive
/// </summary>
public static readonly int BlockSize = 512;

/// <summary>
///     Number of blocks in a record by default
/// </summary>
public static readonly int DefaultBlockFactor = 20;

/// <summary>
///     Size in bytes of a record by default
/// </summary>
public static readonly int DefaultRecordSize = BlockSize * DefaultBlockFactor;

    private int currentBlockIndex;
    private int currentRecordIndex;

    private bool debug;

    private Stream inputStream;
    private Stream outputStream;

    private byte[] recordBuffer;

    /// <summary>
    ///     Get the recordsize for this buffer
    /// </summary>
    public int RecordSize { get; private set; } = DefaultRecordSize;

    /// <summary>
    ///     Get the Blocking factor for the buffer
    /// </summary>
    public int BlockFactor { get; private set; } = DefaultBlockFactor;


    /// <summary>
    ///     Construct a default TarBuffer
    /// </summary>
    protected TarBuffer()
    {
    }

    /// <summary>
    ///     Set the debugging flag for the buffer.
    /// </summary>
    public void SetDebug(bool debug) =>
        this.debug = debug;

    /// <summary>
    ///     Create TarBuffer for reading with default BlockFactor
    /// </summary>
    /// <param name="inputStream">Stream to buffer</param>
    /// <returns>TarBuffer</returns>
    public static TarBuffer CreateInputTarBuffer(Stream inputStream) =>
        CreateInputTarBuffer(inputStream, DefaultBlockFactor);

    /// <summary>
    ///     Construct TarBuffer for reading inputStream setting BlockFactor
    /// </summary>
    /// <param name="inputStream">Stream to buffer</param>
    /// <param name="blockFactor">Blocking factor to apply</param>
    /// <returns>TarBuffer</returns>
    public static TarBuffer CreateInputTarBuffer(Stream inputStream, int blockFactor)
    {
        var tarBuffer = new TarBuffer();
        tarBuffer.inputStream = inputStream;
        tarBuffer.outputStream = null;
        tarBuffer.Initialize(blockFactor);

        return tarBuffer;
    }

    /// <summary>
    ///     Construct TarBuffer for writing with default BlockFactor
    /// </summary>
    /// <param name="outputStream">output stream for buffer</param>
    /// <returns>TarBuffer</returns>
    public static TarBuffer CreateOutputTarBuffer(Stream outputStream) =>
        CreateOutputTarBuffer(outputStream, DefaultBlockFactor);

    /// <summary>
    ///     Construct TarBuffer for writing setting BlockFactor
    /// </summary>
    /// <param name="outputStream">output stream to buffer</param>
    /// <param name="blockFactor">Blocking factor to apply</param>
    /// <returns>TarBuffer</returns>
    public static TarBuffer CreateOutputTarBuffer(Stream outputStream, int blockFactor)
    {
        var tarBuffer = new TarBuffer();
        tarBuffer.inputStream = null;
        tarBuffer.outputStream = outputStream;
        tarBuffer.Initialize(blockFactor);

        return tarBuffer;
    }

    /// <summary>
    ///     Initialization common to all constructors.
    /// </summary>
    private void Initialize(int blockFactor)
    {
        debug = false;
        this.BlockFactor = blockFactor;
        RecordSize = blockFactor * BlockSize;

        recordBuffer = new byte[RecordSize];

        if (inputStream != null)
        {
            currentRecordIndex = -1;
            currentBlockIndex = BlockFactor;
        }
        else
        {
            currentRecordIndex = 0;
            currentBlockIndex = 0;
        }
    }

    /// <summary>
    ///     Get the TAR Buffer's block factor
    /// </summary>
    public int GetBlockFactor() =>
        BlockFactor;

    /// <summary>
    ///     Get the TAR Buffer's record size.
    /// </summary>
    public int GetRecordSize() =>
        RecordSize;

    /// <summary>
    ///     Determine if an archive block indicates End of Archive. End of
    ///     archive is indicated by a block that consists entirely of null bytes.
    ///     All remaining blocks for the record should also be null's
    ///     However some older tars only do a couple of null blocks (Old GNU tar for one)
    ///     and also partial records
    /// </summary>
    /// <param name="block">
    ///     The block data to check.
    /// </param>
    public bool IsEOFBlock(byte[] block)
    {
        for (int i = 0, sz = BlockSize; i < sz; ++i)
            if (block[i] != 0)
                return false;

        return true;
    }

    /// <summary>
    ///     Skip over a block on the input stream.
    /// </summary>
    public void SkipBlock()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("SkipBlock: recIdx = " + this.currentRecordIndex + " blkIdx = " + this.currentBlockIndex);
        }

        if (inputStream == null) throw new ApplicationException("no input stream defined");

        if (currentBlockIndex >= BlockFactor)
            if (!ReadRecord())
                return;

        currentBlockIndex++;
    }

    /// <summary>
    ///     Read a block from the input stream and return the data.
    /// </summary>
    /// <returns>
    ///     The block of data read
    /// </returns>
    public byte[] ReadBlock()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine( "ReadBlock: blockIndex = " + this.currentBlockIndex + " recordIndex = " + this.currentRecordIndex );
        }

        if (inputStream == null) throw new ApplicationException("TarBuffer.ReadBlock - no input stream defined");

        if (currentBlockIndex >= BlockFactor)
            if (!ReadRecord())
                return null;

        var result = new byte[BlockSize];

        Array.Copy(recordBuffer, currentBlockIndex * BlockSize, result, 0, BlockSize);
        currentBlockIndex++;
        return result;
    }

    /// <summary>
    ///     Read a record from data stream.
    /// </summary>
    /// <returns>
    ///     alse if End-Of-File, else true
    /// </returns>
    private bool ReadRecord()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("ReadRecord: recordIndex = " + this.currentRecordIndex);
        }

        if (inputStream == null) throw new ApplicationException("no input stream stream defined");

        currentBlockIndex = 0;

        var offset = 0;
        var bytesNeeded = RecordSize;

        while (bytesNeeded > 0)
        {
            long numBytes = inputStream.Read(recordBuffer, offset, bytesNeeded);

            //
            // NOTE
            // We have found EOF, and the record is not full!
            //
            // This is a broken archive. It does not follow the standard
            // blocking algorithm. However, because we are generous, and
            // it requires little effort, we will simply ignore the error
            // and continue as if the entire record were read. This does
            // not appear to break anything upstream. We used to return
            // false in this case.
            //
            // Thanks to 'Yohann.Roussel@alcatel.fr' for this fix.
            //
            if (numBytes <= 0) break;

            offset += (int)numBytes;
            bytesNeeded -= (int)numBytes;
            if (numBytes != RecordSize)
                if (debug)
                {
                    //Console.WriteLine.WriteLine("ReadRecord: INCOMPLETE READ " + numBytes + " of " + this.blockSize + " bytes read.");
                }
        }

        currentRecordIndex++;
        return true;
    }

    /// <summary>
    ///     Get the current block number, within the current record, zero based.
    /// </summary>
    /// <returns>
    ///     The current zero based block number.
    /// </returns>
    public int GetCurrentBlockNum() =>
        currentBlockIndex;

    /// <summary>
    ///     Get the current record number
    ///     Absolute block number in file = (currentRecordNum * block factor) + currentBlockNum.
    /// </summary>
    /// <returns>
    ///     The current zero based record number.
    /// </returns>
    public int GetCurrentRecordNum() =>
        currentRecordIndex;

    /// <summary>
    ///     Write a block of data to the archive.
    /// </summary>
    /// <param name="block">
    ///     The data to write to the archive.
    /// </param>
    public void WriteBlock(byte[] block)
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("WriteRecord: recIdx = " + this.currentRecordIndex + " blkIdx = " + this.currentBlockIndex );
        }

        if (outputStream == null) throw new ApplicationException("TarBuffer.WriteBlock - no output stream defined");

        if (block.Length != BlockSize) throw new ApplicationException("TarBuffer.WriteBlock - block to write has length '" + block.Length + "' which is not the block size of '" + BlockSize + "'");

        if (currentBlockIndex >= BlockFactor) WriteRecord();

        Array.Copy(block, 0, recordBuffer, currentBlockIndex * BlockSize, BlockSize);
        currentBlockIndex++;
    }

    /// <summary>
    ///     Write an archive record to the archive, where the record may be
    ///     inside of a larger array buffer. The buffer must be "offset plus
    ///     record size" long.
    /// </summary>
    /// <param name="buf">
    ///     The buffer containing the record data to write.
    /// </param>
    /// <param name="offset">
    ///     The offset of the record data within buf.
    /// </param>
    public void WriteBlock(byte[] buf, int offset)
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("WriteBlock: recIdx = " + this.currentRecordIndex + " blkIdx = " + this.currentBlockIndex );
        }

        if (outputStream == null) throw new ApplicationException("TarBuffer.WriteBlock - no output stream stream defined");

        if (offset + BlockSize > buf.Length) throw new ApplicationException("TarBuffer.WriteBlock - record has length '" + buf.Length + "' with offset '" + offset + "' which is less than the record size of '" + RecordSize + "'");

        if (currentBlockIndex >= BlockFactor) WriteRecord();

        Array.Copy(buf, offset, recordBuffer, currentBlockIndex * BlockSize, BlockSize);

        currentBlockIndex++;
    }

    /// <summary>
    ///     Write a TarBuffer record to the archive.
    /// </summary>
    private void WriteRecord()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("Writerecord: record index = " + this.currentRecordIndex);
        }

        if (outputStream == null) throw new ApplicationException("TarBuffer.WriteRecord no output stream defined");

        outputStream.Write(recordBuffer, 0, RecordSize);
        outputStream.Flush();

        currentBlockIndex = 0;
        currentRecordIndex++;
    }

    /// <summary>
    ///     Flush the current data block if it has any data in it.
    /// </summary>
    private void Flush()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("TarBuffer.FlushBlock() called.");
        }

        if (outputStream == null) throw new ApplicationException("TarBuffer.Flush no output stream defined");

        if (currentBlockIndex > 0) WriteRecord();
        outputStream.Flush();
    }

    /// <summary>
    ///     Close the TarBuffer. If this is an output buffer, also flush the
    ///     current block before closing.
    /// </summary>
    public void Close()
    {
        if (debug)
        {
            //Console.WriteLine.WriteLine("TarBuffer.Close().");
        }

        if (outputStream != null)
        {
            Flush();

            outputStream.Close();
            outputStream = null;
        }
        else if (inputStream != null)
        {
            inputStream.Close();
            inputStream = null;
        }
    }
}

/* The original Java file had this header:
	*
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