// ZipEntry.cs
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
using System.IO;

namespace SWF.NET.ZIP.Zip;

/// <summary>
///     This class represents an entry in a zip archive.  This can be a file
///     or a directory
///     ZipFile and ZipInputStream will give you instances of this class as
///     information about the members in an archive.  ZipOutputStream
///     uses an instance of this class when creating an entry in a Zip file.
///     <br />
///     <br />Author of the original java version : Jochen Hoenicke
/// </summary>
public class ZipEntry : ICloneable
{
    private static readonly int KNOWN_SIZE = 1;
    private static readonly int KNOWN_CSIZE = 2;
    private static readonly int KNOWN_CRC = 4;
    private static readonly int KNOWN_TIME = 8;
    private static readonly int KNOWN_EXTERN_ATTRIBUTES = 16;
    private string comment;
    private ulong compressedSize;
    private uint crc;
    private uint dosTime;
    private int externalFileAttributes = -1; // contains external attributes (os dependant)
    private byte[] extra;

    private ushort known; // Bit flags made up of above bits

    // only relevant for central header entries

    private int offset; // used by ZipFile and ZipOutputStream
    private ulong size;

    private readonly ushort versionMadeBy; // Contains host system and version information
    private readonly ushort versionToExtract; // Version required to extract (library handles <= 2.0)

    /// <summary>
    ///     Get/Set flag indicating if entry is encrypted.
    ///     A simple helper routine to aid interpretation of <see cref="Flags">flags</see>
    /// </summary>
    public bool IsCrypted
    {
        get => (Flags & 1) != 0;
        set
        {
            if (value)
                Flags |= 1;
            else
                Flags &= ~1;
        }
    }

    /// <summary>
    ///     Get/Set general purpose bit flag for entry
    /// </summary>
    /// <remarks>
    ///     General purpose bit flag<br />
    ///     Bit 0: If set, indicates the file is encrypted<br />
    ///     Bit 1-2 Only used for compression type 6 Imploding, and 8, 9 deflating<br />
    ///     Imploding:<br />
    ///     Bit 1 if set indicates an 8K sliding dictionary was used.  If clear a 4k dictionary was used<br />
    ///     Bit 2 if set indicates 3 Shannon-Fanno trees were used to encode the sliding dictionary, 2 otherwise<br />
    ///     <br />
    ///     Deflating:<br />
    ///     Bit 2    Bit 1<br />
    ///     0        0       Normal compression was used<br />
    ///     0        1       Maximum compression was used<br />
    ///     1        0       Fast compression was used<br />
    ///     1        1       Super fast compression was used<br />
    ///     <br />
    ///     Bit 3: If set, the fields crc-32, compressed size
    ///     and uncompressed size are were not able to be written during zip file creation
    ///     The correct values are held in a data descriptor immediately following the compressed data. <br />
    ///     Bit 4: Reserved for use by PKZIP for enhanced deflating<br />
    ///     Bit 5: If set indicates the file contains compressed patch data<br />
    ///     Bit 6: If set indicates strong encryption was used.<br />
    ///     Bit 7-15: Unused or reserved<br />
    /// </remarks>
    public int Flags { get; set; }


    /// <summary>
    ///     Get/Set index of this entry in Zip file
    /// </summary>
    public int ZipFileIndex { get; set; } = -1;

    /// <summary>
    ///     Get/set offset for use in central header
    /// </summary>
    public int Offset
    {
        get => offset;
        set
        {
            if (((ulong)value & 0xFFFFFFFF00000000L) != 0) throw new ArgumentOutOfRangeException("Offset");
            offset = value;
        }
    }


    /// <summary>
    ///     Get/Set external file attributes as an integer.
    ///     The values of this are operating system dependant see
    ///     <see cref="HostSystem">HostSystem</see> for details
    /// </summary>
    public int ExternalFileAttributes
    {
        get
        {
            if ((known & KNOWN_EXTERN_ATTRIBUTES) == 0)
                return -1;
            return externalFileAttributes;
        }

        set
        {
            externalFileAttributes = value;
            known |= (ushort)KNOWN_EXTERN_ATTRIBUTES;
        }
    }

    /// <summary>
    ///     Get the version made by for this entry or zero if unknown.
    ///     The value / 10 indicates the major version number, and
    ///     the value mod 10 is the minor version number
    /// </summary>
    public int VersionMadeBy =>
        versionMadeBy & 0xff;

    /// <summary>
    ///     Gets the compatability information for the <see cref="ExternalFileAttributes">external file attribute</see>
    ///     If the external file attributes are compatible with MS-DOS and can be read
    ///     by PKZIP for DOS version 2.04g then this value will be zero.  Otherwise the value
    ///     will be non-zero and identify the host system on which the attributes are compatible.
    /// </summary>
    /// <remarks>
    ///     The values for this as defined in the Zip File format and by others are shown below.  The values are somewhat
    ///     misleading in some cases as they are not all used as shown.  You should consult the relevant documentation
    ///     to obtain up to date and correct information.  The modified appnote by the infozip group is
    ///     particularly helpful as it documents a lot of peculiarities.  The document is however a little dated.
    ///     <list type="table">
    ///         <item>0 - MS-DOS and OS/2 (FAT / VFAT / FAT32 file systems)</item>
    ///         <item>1 - Amiga</item>
    ///         <item>2 - OpenVMS</item>
    ///         <item>3 - Unix</item>
    ///         <item>4 - VM/CMS</item>
    ///         <item>5 - Atari ST</item>
    ///         <item>6 - OS/2 HPFS</item>
    ///         <item>7 - Macintosh</item>
    ///         <item>8 - Z-System</item>
    ///         <item>9 - CP/M</item>
    ///         <item>10 - Windows NTFS</item>
    ///         <item>11 - MVS (OS/390 - Z/OS)</item>
    ///         <item>12 - VSE</item>
    ///         <item>13 - Acorn Risc</item>
    ///         <item>14 - VFAT</item>
    ///         <item>15 - Alternate MVS</item>
    ///         <item>16 - BeOS</item>
    ///         <item>17 - Tandem</item>
    ///         <item>18 - OS/400</item>
    ///         <item>19 - OS/X (Darwin)</item>
    ///         <item>99 - WinZip AES</item>
    ///         <item>remainder - unused</item>
    ///     </list>
    /// </remarks>

    public int HostSystem =>
        (versionMadeBy >> 8) & 0xff;

    /// <summary>
    ///     Get minimum Zip feature version required to extract this entry
    /// </summary>
    /// <remarks>
    ///     Minimum features are defined as:<br />
    ///     1.0 - Default value<br />
    ///     1.1 - File is a volume label<br />
    ///     2.0 - File is a folder/directory<br />
    ///     2.0 - File is compressed using Deflate compression<br />
    ///     2.0 - File is encrypted using traditional encryption<br />
    ///     2.1 - File is compressed using Deflate64<br />
    ///     2.5 - File is compressed using PKWARE DCL Implode<br />
    ///     2.7 - File is a patch data set<br />
    ///     4.5 - File uses Zip64 format extensions<br />
    ///     4.6 - File is compressed using BZIP2 compression<br />
    ///     5.0 - File is encrypted using DES<br />
    ///     5.0 - File is encrypted using 3DES<br />
    ///     5.0 - File is encrypted using original RC2 encryption<br />
    ///     5.0 - File is encrypted using RC4 encryption<br />
    ///     5.1 - File is encrypted using AES encryption<br />
    ///     5.1 - File is encrypted using corrected RC2 encryption<br />
    ///     5.1 - File is encrypted using corrected RC2-64 encryption<br />
    ///     6.1 - File is encrypted using non-OAEP key wrapping<br />
    ///     6.2 - Central directory encryption (not confirmed yet)<br />
    /// </remarks>
    public int Version
    {
        get
        {
            if (versionToExtract != 0)
                return versionToExtract;
            var result = 10;
            if (CompressionMethod.Deflated == CompressionMethod)
                result = 20;
            else if (IsDirectory)
                result = 20;
            else if (IsCrypted)
                result = 20;
            else if ((known & KNOWN_EXTERN_ATTRIBUTES) != 0 && (externalFileAttributes & 0x08) != 0) result = 11;
            return result;
        }
    }

    /// <summary>
    ///     Get/Set DosTime
    /// </summary>
    public long DosTime
    {
        get
        {
            if ((known & KNOWN_TIME) == 0)
                return 0;
            return dosTime;
        }
        set
        {
            dosTime = (uint)value;
            known |= (ushort)KNOWN_TIME;
        }
    }


    /// <summary>
    ///     Gets/Sets the time of last modification of the entry.
    /// </summary>
    public DateTime DateTime
    {
        get
        {
            var sec = 2 * (dosTime & 0x1f);
            var min = (dosTime >> 5) & 0x3f;
            var hrs = (dosTime >> 11) & 0x1f;
            var day = (dosTime >> 16) & 0x1f;
            var mon = (dosTime >> 21) & 0xf;
            var year = ((dosTime >> 25) & 0x7f) + 1980;
            return new DateTime((int)year, (int)mon, (int)day, (int)hrs, (int)min, (int)sec);
        }
        set =>
            DosTime = ((((uint)value.Year - 1980) & 0x7f) << 25) |
                      ((uint)value.Month << 21) |
                      ((uint)value.Day << 16) |
                      ((uint)value.Hour << 11) |
                      ((uint)value.Minute << 5) |
                      ((uint)value.Second >> 1);
    }

    /// <summary>
    ///     Returns the entry name.  The path components in the entry should
    ///     always separated by slashes ('/').  Dos device names like C: should also
    ///     be removed.  See <see cref="CleanName">CleanName</see>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets/Sets the size of the uncompressed data.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     If the size is not in the range 0..0xffffffffL
    /// </exception>
    /// <returns>
    ///     The size or -1 if unknown.
    /// </returns>
    public long Size
    {
        get => (known & KNOWN_SIZE) != 0 ? (long)size : -1L;
        set
        {
            if (((ulong)value & 0xFFFFFFFF00000000L) != 0) throw new ArgumentOutOfRangeException("size");
            size = (ulong)value;
            known |= (ushort)KNOWN_SIZE;
        }
    }

    /// <summary>
    ///     Gets/Sets the size of the compressed data.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     Size is not in the range 0..0xffffffff
    /// </exception>
    /// <returns>
    ///     The size or -1 if unknown.
    /// </returns>
    public long CompressedSize
    {
        get => (known & KNOWN_CSIZE) != 0 ? (long)compressedSize : -1L;
        set
        {
            if (((ulong)value & 0xffffffff00000000L) != 0) throw new ArgumentOutOfRangeException();
            compressedSize = (ulong)value;
            known |= (ushort)KNOWN_CSIZE;
        }
    }

    /// <summary>
    ///     Gets/Sets the crc of the uncompressed data.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     Crc is not in the range 0..0xffffffffL
    /// </exception>
    /// <returns>
    ///     The crc value or -1 if unknown.
    /// </returns>
    public long Crc
    {
        get => (known & KNOWN_CRC) != 0 ? crc & 0xffffffffL : -1L;
        set
        {
            if ((crc & 0xffffffff00000000L) != 0) throw new ArgumentOutOfRangeException();
            crc = (uint)value;
            known |= (ushort)KNOWN_CRC;
        }
    }

    /// <summary>
    ///     Gets/Sets the compression method. Only Deflated and Stored are supported.
    /// </summary>
    /// <returns>
    ///     The compression method for this entry
    /// </returns>
    /// <see cref="Zip.CompressionMethod.Deflated" />
    /// <see cref="Zip.CompressionMethod.Stored" />
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.Deflated;

    /// <summary>
    ///     Gets/Sets the extra data.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     Extra data is longer than 0xffff bytes.
    /// </exception>
    /// <returns>
    ///     Extra data or null if not set.
    /// </returns>
    public byte[] ExtraData
    {
        get => extra;
        set
        {
            if (value == null)
            {
                extra = null;
                return;
            }

            if (value.Length > 0xffff) throw new ArgumentOutOfRangeException();

            extra = new byte[value.Length];
            Array.Copy(value, 0, extra, 0, value.Length);

            try
            {
                var pos = 0;
                while (pos < extra.Length)
                {
                    var sig = (extra[pos++] & 0xff) | ((extra[pos++] & 0xff) << 8);
                    var len = (extra[pos++] & 0xff) | ((extra[pos++] & 0xff) << 8);

                    if (len < 0 || pos + len > extra.Length)
                        // This is still lenient but the extra data is corrupt
                        // TODO drop the extra data? or somehow indicate to user 
                        // there is a problem...
                        break;

                    if (sig == 0x5455)
                    {
                        // extended time stamp, unix format by Rainer Prem <Rainer@Prem.de>
                        int flags = extra[pos];
                        // Can include other times but these are ignored.  Length of data should
                        // actually be 1 + 4 * no of bits in flags.
                        if ((flags & 1) != 0 && len >= 5)
                        {
                            var iTime = (extra[pos + 1] & 0xff) |
                                        ((extra[pos + 2] & 0xff) << 8) |
                                        ((extra[pos + 3] & 0xff) << 16) |
                                        ((extra[pos + 4] & 0xff) << 24);

                            DateTime = (new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                            known |= (ushort)KNOWN_TIME;
                        }
                    }
                    else if (sig == 0x0001)
                    {
                        // ZIP64 extended information extra field
                        // Of variable size depending on which fields in header are too small
                        // fields appear here if the corresponding local or central directory record field
                        // is set to 0xFFFF or 0xFFFFFFFF and the entry is in Zip64 format.
                        //
                        // Original Size          8 bytes
                        // Compressed size        8 bytes
                        // Relative header offset 8 bytes
                        // Disk start number      4 bytes
                    }

                    pos += len;
                }
            }
            catch (Exception)
            {
                /* be lenient */
            }
        }
    }


    /// <summary>
    ///     Gets/Sets the entry comment.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     If comment is longer than 0xffff.
    /// </exception>
    /// <returns>
    ///     The comment or null if not set.
    /// </returns>
    public string Comment
    {
        get => comment;
        set
        {
            // TODO this test is strictly incorrect as the length is in characters
            // While the test is correct in that a comment of this length or greater 
            // is definitely invalid, shorter comments may also have an invalid length.
            if (value != null && value.Length > 0xffff) throw new ArgumentOutOfRangeException();
            comment = value;
        }
    }

    /// <summary>
    ///     Gets a value indicating of the if the entry is a directory.  A directory is determined by
    ///     an entry name with a trailing slash '/'.  The external file attributes
    ///     can also mark a file as a directory.  The trailing slash convention should always be followed
    ///     however.
    /// </summary>
    public bool IsDirectory
    {
        get
        {
            var result = false;
            var nlen = Name.Length;
            result = nlen > 0 && Name[nlen - 1] == '/';

            if (result == false && (known & KNOWN_EXTERN_ATTRIBUTES) != 0)
                if (HostSystem == 0 && (ExternalFileAttributes & 16) != 0)
                    result = true;
            return result;
        }
    }

    /// <summary>
    ///     Creates a zip entry with the given name.
    /// </summary>
    /// <param name="name">
    ///     The name for this entry. Can include directory components.
    ///     The convention for names is 'unix'  style paths with no device names and
    ///     path elements separated by '/' characters.  This is not enforced see <see cref="CleanName">CleanName</see>
    ///     on how to ensure names are valid if this is desired.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     The name passed is null
    /// </exception>
    public ZipEntry(string name) : this(name, 0, ZipConstants.VERSION_MADE_BY)
    {
    }

    /// <summary>
    ///     Creates a zip entry with the given name and version required to extract
    /// </summary>
    /// <param name="name">
    ///     The name for this entry. Can include directory components.
    ///     The convention for names is 'unix'  style paths with no device names and
    ///     path elements separated by '/' characters.  This is not enforced see <see cref="CleanName">CleanName</see>
    ///     on how to ensure names are valid if this is desired.
    /// </param>
    /// <param name="versionRequiredToExtract">
    ///     The minimum 'feature version' required this entry
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     The name passed is null
    /// </exception>
    internal ZipEntry(string name, int versionRequiredToExtract) : this(name, versionRequiredToExtract, ZipConstants.VERSION_MADE_BY)
    {
    }

    /// <summary>
    ///     Initializes an entry with the given name and made by information
    /// </summary>
    /// <param name="name">Name for this entry</param>
    /// <param name="madeByInfo">Version and HostSystem Information</param>
    /// <param name="versionRequiredToExtract">Minimum required zip feature version required to extract this entry</param>
    /// <exception cref="ArgumentNullException">
    ///     The name passed is null
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     versionRequiredToExtract should be 0 (auto-calculate) or > 10
    /// </exception>
    /// <remarks>
    ///     This constructor is used by the ZipFile class when reading from the central header
    ///     It is not generally useful, use the constructor specifying the name only.
    /// </remarks>
    internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo)
    {
        if (name == null) throw new ArgumentNullException("ZipEntry name");

        if (versionRequiredToExtract != 0 && versionRequiredToExtract < 10) throw new ArgumentOutOfRangeException("versionRequiredToExtract");

        DateTime = DateTime.Now;
        Name = name;
        versionMadeBy = (ushort)madeByInfo;
        versionToExtract = (ushort)versionRequiredToExtract;
    }

    /// <summary>
    ///     Creates a copy of the given zip entry.
    /// </summary>
    /// <param name="e">
    ///     The entry to copy.
    /// </param>
    public ZipEntry(ZipEntry e)
    {
        known = e.known;
        Name = e.Name;
        size = e.size;
        compressedSize = e.compressedSize;
        crc = e.crc;
        dosTime = e.dosTime;
        CompressionMethod = e.CompressionMethod;
        ExtraData = e.ExtraData; // Note use of property ensuring data is unique
        comment = e.comment;
        versionToExtract = e.versionToExtract;
        versionMadeBy = e.versionMadeBy;
        externalFileAttributes = e.externalFileAttributes;
        Flags = e.Flags;

        ZipFileIndex = -1;
        offset = 0;
    }

    /// <summary>
    ///     Creates a copy of this zip entry.
    /// </summary>
    public object Clone() =>
        MemberwiseClone();

    /// <summary>
    ///     Cleans a name making it conform to Zip file conventions.
    ///     Devices names ('c:\') and UNC share names ('\\server\share') are removed
    ///     and forward slashes ('\') are converted to back slashes ('/').
    /// </summary>
    /// <param name="name">Name to clean</param>
    /// <param name="relativePath">Make names relative if true or absolute if false</param>
    public static string CleanName(string name, bool relativePath)
    {
        if (name == null) return "";

        if (Path.IsPathRooted(name))
            // NOTE:
            // for UNC names...  \\machine\share\zoom\beet.txt gives \zoom\beet.txt
            name = name.Substring(Path.GetPathRoot(name).Length);

        name = name.Replace(@"\", "/");

        if (relativePath)
        {
            if (name.Length > 0 && (name[0] == Path.AltDirectorySeparatorChar || name[0] == Path.DirectorySeparatorChar)) name = name.Remove(0, 1);
        }
        else
        {
            if (name.Length > 0 && name[0] != Path.AltDirectorySeparatorChar && name[0] != Path.DirectorySeparatorChar) name = name.Insert(0, "/");
        }

        return name;
    }

    /// <summary>
    ///     Cleans a name making it conform to Zip file conventions.
    ///     Devices names ('c:\') and UNC share names ('\\server\share') are removed
    ///     and forward slashes ('\') are converted to back slashes ('/').
    ///     Names are made relative by trimming leading slashes which is compatible
    ///     with Windows-XPs built in Zip file handling.
    /// </summary>
    /// <param name="name">Name to clean</param>
    public static string CleanName(string name) =>
        CleanName(name, true);

    /// <summary>
    ///     Gets the string representation of this ZipEntry.
    /// </summary>
    public override string ToString() =>
        Name;
}