// ZipFile.cs
//
// Copyright (C) 2001 Mike Krueger
// Copyright (C) 2004 John Reilly
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
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
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using SWF.NET.ZIP.Zip.Compression;
using SWF.NET.ZIP.Zip.Compression.Streams;

namespace SWF.NET.ZIP.Zip;

/// <summary>
///     This class represents a Zip archive.  You can ask for the contained
///     entries, or get an input stream for a file entry.  The entry is
///     automatically decompressed.
///     This class is thread safe:  You can open input streams for arbitrary
///     entries in different threads.
///     <br />
///     <br />Author of the original java version : Jochen Hoenicke
/// </summary>
/// <example>
///     <code>
/// using System;
/// using System.Text;
/// using System.Collections;
/// using System.IO;
/// 
/// using ICSharpCode.SharpZipLib.Zip;
/// 
/// class MainClass
/// {
/// 	static public void Main(string[] args)
/// 	{
/// 		ZipFile zFile = new ZipFile(args[0]);
/// 		Console.WriteLine("Listing of : " + zFile.Name);
/// 		Console.WriteLine("");
/// 		Console.WriteLine("Raw Size    Size      Date     Time     Name");
/// 		Console.WriteLine("--------  --------  --------  ------  ---------");
/// 		foreach (ZipEntry e in zFile) {
/// 			DateTime d = e.DateTime;
/// 			Console.WriteLine("{0, -10}{1, -10}{2}  {3}   {4}", e.Size, e.CompressedSize,
/// 			                                                    d.ToString("dd-MM-yy"), d.ToString("t"),
/// 			                                                    e.Name);
/// 		}
/// 	}
/// }
/// </code>
/// </example>
public class ZipFile : IEnumerable
{
    private readonly Stream baseStream;
    private ZipEntry[] entries;

    /// <summary>
    ///     Indexer property for ZipEntries
    /// </summary>
    [IndexerName("EntryByIndex")]
    public ZipEntry this[int index] =>
        (ZipEntry)entries[index].Clone();

    /// <summary>
    ///     Gets the comment for the zip file.
    /// </summary>
    public string ZipFileComment { get; private set; }

    /// <summary>
    ///     Gets the name of this zip file.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the number of entries in this zip file.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The Zip file has been closed.
    /// </exception>
    public int Size
    {
        get
        {
            if (entries != null)
                return entries.Length;
            throw new InvalidOperationException("ZipFile is closed");
        }
    }

    /// <summary>
    ///     Opens a Zip file with the given name for reading.
    /// </summary>
    /// <exception cref="IOException">
    ///     An i/o error occurs
    /// </exception>
    /// <exception cref="ZipException">
    ///     The file doesn't contain a valid zip archive.
    /// </exception>
    public ZipFile(string name) : this(File.OpenRead(name))
    {
    }

    /// <summary>
    ///     Opens a Zip file reading the given FileStream
    /// </summary>
    /// <exception cref="IOException">
    ///     An i/o error occurs.
    /// </exception>
    /// <exception cref="ZipException">
    ///     The file doesn't contain a valid zip archive.
    /// </exception>
    public ZipFile(FileStream file)
    {
        baseStream = file;
        Name = file.Name;
        ReadEntries();
    }

    /// <summary>
    ///     Opens a Zip file reading the given Stream
    /// </summary>
    /// <exception cref="IOException">
    ///     An i/o error occurs
    /// </exception>
    /// <exception cref="ZipException">
    ///     The file doesn't contain a valid zip archive.<br />
    ///     The stream provided cannot seek
    /// </exception>
    public ZipFile(Stream baseStream)
    {
        this.baseStream = baseStream;
        Name = null;
        ReadEntries();
    }

    /// <summary>
    ///     Returns an enumerator for the Zip entries in this Zip file.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The Zip file has been closed.
    /// </exception>
    public IEnumerator GetEnumerator()
    {
        if (entries == null) throw new InvalidOperationException("ZipFile has closed");

        return new ZipEntryEnumeration(entries);
    }


    /// <summary>
    ///     Read an unsigned short in little endian byte order.
    /// </summary>
    /// <exception cref="IOException">
    ///     An i/o error occurs.
    /// </exception>
    /// <exception cref="EndOfStreamException">
    ///     The file ends prematurely
    /// </exception>
    private int ReadLeShort() =>
        baseStream.ReadByte() | (baseStream.ReadByte() << 8);

    /// <summary>
    ///     Read an int in little endian byte order.
    /// </summary>
    /// <exception cref="IOException">
    ///     An i/o error occurs.
    /// </exception>
    /// <exception cref="System.IO.EndOfStreamException">
    ///     The file ends prematurely
    /// </exception>
    private int ReadLeInt() =>
        ReadLeShort() | (ReadLeShort() << 16);

    /// <summary>
    ///     Search for and read the central directory of a zip file filling the entries
    ///     array.  This is called exactly once by the constructors.
    /// </summary>
    /// <exception cref="System.IO.IOException">
    ///     An i/o error occurs.
    /// </exception>
    /// <exception cref="ZipException">
    ///     The central directory is malformed or cannot be found
    /// </exception>
    private void ReadEntries()
    {
        // Search for the End Of Central Directory.  When a zip comment is
        // present the directory may start earlier.
        // 
        // TODO: The search is limited to 64K which is the maximum size of a trailing comment field to aid speed.
        // This should be compatible with both SFX and ZIP files but has only been tested for Zip files
        // Need to confirm this is valid in all cases.
        // Could also speed this up by reading memory in larger blocks?


        if (baseStream.CanSeek == false) throw new ZipException("ZipFile stream must be seekable");

        var pos = baseStream.Length - ZipConstants.ENDHDR;
        if (pos <= 0) throw new ZipException("File is too small to be a Zip file");

        var giveUpMarker = Math.Max(pos - 0x10000, 0);

        do
        {
            if (pos < giveUpMarker) throw new ZipException("central directory not found, probably not a zip file");
            baseStream.Seek(pos--, SeekOrigin.Begin);
        }
        while (ReadLeInt() != ZipConstants.ENDSIG);

        var thisDiskNumber = ReadLeShort();
        var startCentralDirDisk = ReadLeShort();
        var entriesForThisDisk = ReadLeShort();
        var entriesForWholeCentralDir = ReadLeShort();
        var centralDirSize = ReadLeInt();
        var offsetOfCentralDir = ReadLeInt();
        var commentSize = ReadLeShort();

        var zipComment = new byte[commentSize];
        baseStream.Read(zipComment, 0, zipComment.Length);
        ZipFileComment = ZipConstants.ConvertToString(zipComment);

/* Its seems possible that this is too strict, more digging required.
			if (thisDiskNumber != 0 || startCentralDirDisk != 0 || entriesForThisDisk != entriesForWholeCentralDir) {
				throw new ZipException("Spanned archives are not currently handled");
			}
*/

        entries = new ZipEntry[entriesForWholeCentralDir];
        baseStream.Seek(offsetOfCentralDir, SeekOrigin.Begin);

        for (var i = 0; i < entriesForWholeCentralDir; i++)
        {
            if (ReadLeInt() != ZipConstants.CENSIG) throw new ZipException("Wrong Central Directory signature");

            var versionMadeBy = ReadLeShort();
            var versionToExtract = ReadLeShort();
            var bitFlags = ReadLeShort();
            var method = ReadLeShort();
            var dostime = ReadLeInt();
            var crc = ReadLeInt();
            var csize = ReadLeInt();
            var size = ReadLeInt();
            var nameLen = ReadLeShort();
            var extraLen = ReadLeShort();
            var commentLen = ReadLeShort();

            var diskStartNo = ReadLeShort(); // Not currently used
            var internalAttributes = ReadLeShort(); // Not currently used

            var externalAttributes = ReadLeInt();
            var offset = ReadLeInt();

            var buffer = new byte[Math.Max(nameLen, commentLen)];

            baseStream.Read(buffer, 0, nameLen);
            var name = ZipConstants.ConvertToString(buffer, nameLen);

            var entry = new ZipEntry(name, versionToExtract, versionMadeBy);
            entry.CompressionMethod = (CompressionMethod)method;
            entry.Crc = crc & 0xffffffffL;
            entry.Size = size & 0xffffffffL;
            entry.CompressedSize = csize & 0xffffffffL;
            entry.DosTime = (uint)dostime;

            if (extraLen > 0)
            {
                var extra = new byte[extraLen];
                baseStream.Read(extra, 0, extraLen);
                entry.ExtraData = extra;
            }

            if (commentLen > 0)
            {
                baseStream.Read(buffer, 0, commentLen);
                entry.Comment = ZipConstants.ConvertToString(buffer, commentLen);
            }

            entry.ZipFileIndex = i;
            entry.Offset = offset;
            entry.ExternalFileAttributes = externalAttributes;

            entries[i] = entry;
        }
    }

    /// <summary>
    ///     Closes the ZipFile.  This also closes all input streams managed by
    ///     this class.  Once closed, no further instance methods should be
    ///     called.
    /// </summary>
    /// <exception cref="System.IO.IOException">
    ///     An i/o error occurs.
    /// </exception>
    public void Close()
    {
        entries = null;
        lock (baseStream) baseStream.Close();
    }

    /// <summary>
    ///     Return the index of the entry with a matching name
    /// </summary>
    /// <param name="name">Entry name to find</param>
    /// <param name="ignoreCase">If true the comparison is case insensitive</param>
    /// <returns>The index position of the matching entry or -1 if not found</returns>
    /// <exception cref="InvalidOperationException">
    ///     The Zip file has been closed.
    /// </exception>
    public int FindEntry(string name, bool ignoreCase)
    {
        if (entries == null) throw new InvalidOperationException("ZipFile has been closed");

        for (var i = 0; i < entries.Length; i++)
            if (string.Compare(name, entries[i].Name, ignoreCase) == 0)
                return i;
        return -1;
    }

    /// <summary>
    ///     Searches for a zip entry in this archive with the given name.
    ///     String comparisons are case insensitive
    /// </summary>
    /// <param name="name">
    ///     The name to find. May contain directory components separated by slashes ('/').
    /// </param>
    /// <returns>
    ///     The zip entry, or null if no entry with that name exists.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     The Zip file has been closed.
    /// </exception>
    public ZipEntry GetEntry(string name)
    {
        if (entries == null) throw new InvalidOperationException("ZipFile has been closed");

        var index = FindEntry(name, true);
        return index >= 0 ? (ZipEntry)entries[index].Clone() : null;
    }

    /// <summary>
    ///     Checks, if the local header of the entry at index i matches the
    ///     central directory, and returns the offset to the data.
    /// </summary>
    /// <returns>
    ///     The start offset of the (compressed) data.
    /// </returns>
    /// <exception cref="System.IO.EndOfStreamException">
    ///     The stream ends prematurely
    /// </exception>
    /// <exception cref="ZipException">
    ///     The local header signature is invalid, the entry and central header file name lengths are different
    ///     or the local and entry compression methods dont match
    /// </exception>
    private long CheckLocalHeader(ZipEntry entry)
    {
        lock (baseStream)
        {
            baseStream.Seek(entry.Offset, SeekOrigin.Begin);
            if (ReadLeInt() != ZipConstants.LOCSIG) throw new ZipException("Wrong Local header signature");

            var shortValue = (short)ReadLeShort(); // version required to extract
            if (shortValue > ZipConstants.VERSION_MADE_BY) throw new ZipException(string.Format("Version required to extract this entry not supported ({0})", shortValue));

            shortValue = (short)ReadLeShort(); // general purpose bit flags.
            if ((shortValue & 0x30) != 0) throw new ZipException("The library doesnt support the zip version required to extract this entry");

            if (entry.CompressionMethod != (CompressionMethod)ReadLeShort()) throw new ZipException("Compression method mismatch");

            // Skip time, crc, size and csize
            var oldPos = baseStream.Position;
            baseStream.Position += ZipConstants.LOCNAM - ZipConstants.LOCTIM;

            if (baseStream.Position - oldPos != ZipConstants.LOCNAM - ZipConstants.LOCTIM) throw new EndOfStreamException();

            // TODO make test more correct...  cant compare lengths as was done originally as this can fail for MBCS strings
            var storedNameLength = ReadLeShort();
            if (entry.Name.Length > storedNameLength) throw new ZipException("file name length mismatch");

            var extraLen = storedNameLength + ReadLeShort();
            return entry.Offset + ZipConstants.LOCHDR + extraLen;
        }
    }

    /// <summary>
    ///     Creates an input stream reading the given zip entry as
    ///     uncompressed data.  Normally zip entry should be an entry
    ///     returned by GetEntry().
    /// </summary>
    /// <returns>
    ///     the input stream.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     The ZipFile has already been closed
    /// </exception>
    /// <exception cref="ZipException">
    ///     The compression method for the entry is unknown
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    ///     The entry is not found in the ZipFile
    /// </exception>
    public Stream GetInputStream(ZipEntry entry)
    {
        if (entries == null) throw new InvalidOperationException("ZipFile has closed");

        var index = entry.ZipFileIndex;
        if (index < 0 || index >= entries.Length || entries[index].Name != entry.Name)
        {
            index = FindEntry(entry.Name, true);
            if (index < 0) throw new IndexOutOfRangeException();
        }

        return GetInputStream(index);
    }


    /// <summary>
    ///     Creates an input stream reading the zip entry based on the index passed
    /// </summary>
    /// <returns>
    ///     An input stream.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     The ZipFile has already been closed
    /// </exception>
    /// <exception cref="ZipException">
    ///     The compression method for the entry is unknown
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    ///     The entry is not found in the ZipFile
    /// </exception>
    public Stream GetInputStream(int entryIndex)
    {
        if (entries == null) throw new InvalidOperationException("ZipFile has closed");

        var start = CheckLocalHeader(entries[entryIndex]);
        var method = entries[entryIndex].CompressionMethod;
        Stream istr = new PartialInputStream(baseStream, start, entries[entryIndex].CompressedSize);

        switch (method)
        {
            case CompressionMethod.Stored:
                return istr;
            case CompressionMethod.Deflated:
                return new InflaterInputStream(istr, new Inflater(true));
            default:
                throw new ZipException("Unknown compression method " + method);
        }
    }

    private class ZipEntryEnumeration : IEnumerator
    {
        private readonly ZipEntry[] array;
        private int ptr = -1;

        public object Current =>
            array[ptr];

        public ZipEntryEnumeration(ZipEntry[] arr) =>
            array = arr;

        public void Reset() =>
            ptr = -1;

        public bool MoveNext() =>
            ++ptr < array.Length;
    }

    private class PartialInputStream : InflaterInputStream
    {
        private readonly Stream baseStream;
        private long filepos;
        private readonly long end;

        public override int Available
        {
            get
            {
                var amount = end - filepos;
                if (amount > int.MaxValue) return int.MaxValue;

                return (int)amount;
            }
        }

        public PartialInputStream(Stream baseStream, long start, long len) : base(baseStream)
        {
            this.baseStream = baseStream;
            filepos = start;
            end = start + len;
        }

        public override int ReadByte()
        {
            if (filepos == end) return -1; //ok

            lock (baseStream)
            {
                baseStream.Seek(filepos++, SeekOrigin.Begin);
                return baseStream.ReadByte();
            }
        }

        public override int Read(byte[] b, int off, int len)
        {
            if (len > end - filepos)
            {
                len = (int)(end - filepos);
                if (len == 0) return 0;
            }

            lock (baseStream)
            {
                baseStream.Seek(filepos, SeekOrigin.Begin);
                var count = baseStream.Read(b, off, len);
                if (count > 0) filepos += len;
                return count;
            }
        }

        public long SkipBytes(long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException();
            if (amount > end - filepos) amount = end - filepos;
            filepos += amount;
            return amount;
        }
    }
}