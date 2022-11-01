// TarHeader.cs
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


/* The tar format and its POSIX successor PAX have a long history which makes for compatability
   issues when creating and reading files...
   This is further complicated by a large number of programs with variations on formats
   One common issue is the handling of names longer than 100 characters.
   GNU style long names are currently supported.

This is the ustar (Posix 1003.1) header.

struct header 
{
	char t_name[100];          //   0 Filename
	char t_mode[8];            // 100 Permissions
	char t_uid[8];             // 108 Numerical User ID
	char t_gid[8];             // 116 Numerical Group ID
	char t_size[12];           // 124 Filesize
	char t_mtime[12];          // 136 st_mtime
	char t_chksum[8];          // 148 Checksum
	char t_typeflag;           // 156 Type of File
	char t_linkname[100];      // 157 Target of Links
	char t_magic[6];           // 257 "ustar" or other...
	char t_version[2];         // 263 Version fixed to 00
	char t_uname[32];          // 265 User Name
	char t_gname[32];          // 297 Group Name
	char t_devmajor[8];        // 329 Major for devices
	char t_devminor[8];        // 337 Minor for devices
	char t_prefix[155];        // 345 Prefix for t_name
	char t_mfill[12];          // 500 Filler up to 512
};

*/

using System;
using System.Text;

namespace SWF.NET.ZIP.Tar;

/// <summary>
///     This class encapsulates the Tar Entry Header used in Tar Archives.
///     The class also holds a number of tar constants, used mostly in headers.
/// </summary>
public class TarHeader : ICloneable
{
    //
    // LF_ constants represent the "type" of an entry
    //

    /// <summary>
    ///     The "old way" of indicating a normal file.
    /// </summary>
    public const byte LF_OLDNORM = 0;

    /// <summary>
    ///     Normal file type.
    /// </summary>
    public const byte LF_NORMAL = (byte)'0';

    /// <summary>
    ///     Link file type.
    /// </summary>
    public const byte LF_LINK = (byte)'1';

    /// <summary>
    ///     Symbolic link file type.
    /// </summary>
    public const byte LF_SYMLINK = (byte)'2';

    /// <summary>
    ///     Character device file type.
    /// </summary>
    public const byte LF_CHR = (byte)'3';

    /// <summary>
    ///     Block device file type.
    /// </summary>
    public const byte LF_BLK = (byte)'4';

    /// <summary>
    ///     Directory file type.
    /// </summary>
    public const byte LF_DIR = (byte)'5';

    /// <summary>
    ///     FIFO (pipe) file type.
    /// </summary>
    public const byte LF_FIFO = (byte)'6';

    /// <summary>
    ///     Contiguous file type.
    /// </summary>
    public const byte LF_CONTIG = (byte)'7';

    /// <summary>
    ///     Posix.1 2001 global extended header
    /// </summary>
    public const byte LF_GHDR = (byte)'g';


    // POSIX allows for upper case ascii type as extensions

    /// <summary>
    ///     Solaris access control list file type
    /// </summary>
    public const byte LF_ACL = (byte)'A';

    /// <summary>
    ///     GNU dir dump file type
    ///     This is a dir entry that contains the names of files that were in the
    ///     dir at the time the dump was made
    /// </summary>
    public const byte LF_GNU_DUMPDIR = (byte)'D';

    /// <summary>
    ///     Solaris Extended Attribute File
    /// </summary>
    public const byte LF_EXTATTR = (byte)'E';

    /// <summary>
    ///     Inode (metadata only) no file content
    /// </summary>
    public const byte LF_META = (byte)'I';

    /// <summary>
    ///     Identifies the next file on the tape as having a long link name
    /// </summary>
    public const byte LF_GNU_LONGLINK = (byte)'K';

    /// <summary>
    ///     Identifies the next file on the tape as having a long name
    /// </summary>
    public const byte LF_GNU_LONGNAME = (byte)'L';

    /// <summary>
    ///     Continuation of a file that began on another volume
    /// </summary>
    public const byte LF_GNU_MULTIVOL = (byte)'M';

    /// <summary>
    ///     For storing filenames that dont fit in the main header (old GNU)
    /// </summary>
    public const byte LF_GNU_NAMES = (byte)'N';

    /// <summary>
    ///     GNU Sparse file
    /// </summary>
    public const byte LF_GNU_SPARSE = (byte)'S';

    /// <summary>
    ///     GNU Tape/volume header ignore on extraction
    /// </summary>
    public const byte LF_GNU_VOLHDR = (byte)'V';

    /// <summary>
    ///     The length of the name field in a header buffer.
    /// </summary>
    public static readonly int NAMELEN = 100;

    /// <summary>
    ///     The length of the mode field in a header buffer.
    /// </summary>
    public static readonly int MODELEN = 8;

    /// <summary>
    ///     The length of the user id field in a header buffer.
    /// </summary>
    public static readonly int UIDLEN = 8;

    /// <summary>
    ///     The length of the group id field in a header buffer.
    /// </summary>
    public static readonly int GIDLEN = 8;

    /// <summary>
    ///     The length of the checksum field in a header buffer.
    /// </summary>
    public static readonly int CHKSUMLEN = 8;

    /// <summary>
    ///     The length of the size field in a header buffer.
    /// </summary>
    public static readonly int SIZELEN = 12;

    /// <summary>
    ///     The length of the magic field in a header buffer.
    /// </summary>
    public static readonly int MAGICLEN = 6;

    /// <summary>
    ///     The length of the version field in a header buffer.
    /// </summary>
    public static readonly int VERSIONLEN = 2;

    /// <summary>
    ///     The length of the modification time field in a header buffer.
    /// </summary>
    public static readonly int MODTIMELEN = 12;

    /// <summary>
    ///     The length of the user name field in a header buffer.
    /// </summary>
    public static readonly int UNAMELEN = 32;

    /// <summary>
    ///     The length of the group name field in a header buffer.
    /// </summary>
    public static readonly int GNAMELEN = 32;

    /// <summary>
    ///     The length of the devices field in a header buffer.
    /// </summary>
    public static readonly int DEVLEN = 8;

    /// <summary>
    ///     Posix.1 2001 extended header
    /// </summary>
    public static readonly byte LF_XHDR = (byte)'x';

    /// <summary>
    ///     The magic tag representing a POSIX tar archive.  (includes trailing NULL)
    /// </summary>
    public static readonly string TMAGIC = "ustar ";

    /// <summary>
    ///     The magic tag representing an old GNU tar archive where version is included in magic and overwrites it
    /// </summary>
    public static readonly string GNU_TMAGIC = "ustar  ";

    private static readonly long timeConversionFactor = 10000000L; // -jr- 1 tick == 100 nanoseconds
    private static readonly DateTime datetTime1970 = new(1970, 1, 1, 0, 0, 0, 0);

    /// <summary>
    ///     The entry's checksum.
    /// </summary>
    public int checkSum;

    /// <summary>
    ///     The entry's major device number.
    /// </summary>
    public int devMajor;

    /// <summary>
    ///     The entry's minor device number.
    /// </summary>
    public int devMinor;

    /// <summary>
    ///     The entry's group id.
    /// </summary>
    public int groupId;

    /// <summary>
    ///     The entry's group name.
    /// </summary>
    public StringBuilder groupName;

    /// <summary>
    ///     The entry's link name.
    /// </summary>
    public StringBuilder linkName;

    /// <summary>
    ///     The entry's magic tag.
    /// </summary>
    public StringBuilder magic;

    /// <summary>
    ///     The entry's Unix style permission mode.
    /// </summary>
    public int mode;

    /// <summary>
    ///     The entry's modification time.
    /// </summary>
    public DateTime modTime;

    /// <summary>
    ///     The entry's name.
    /// </summary>
    public StringBuilder name;

    /// <summary>
    ///     The entry's size.
    /// </summary>
    public long size;

    /// <summary>
    ///     The entry's type flag.
    /// </summary>
    public byte typeFlag;

    /// <summary>
    ///     The entry's user id.
    /// </summary>
    public int userId;

    /// <summary>
    ///     The entry's user name.
    /// </summary>
    public StringBuilder userName;

    /// <summary>
    ///     The entry's version.
    /// </summary>
    public StringBuilder version;

    /// <summary>
    ///     Construct default TarHeader instance
    /// </summary>
    public TarHeader()
    {
        magic = new StringBuilder(TMAGIC);
        version = new StringBuilder(" ");

        name = new StringBuilder();
        linkName = new StringBuilder();

#if COMPACT_FRAMEWORK
			string user = "PocketPC";
#else
        var user = Environment.UserName;
#endif
        if (user.Length > 31) user = user.Substring(0, 31);

        userId = 0;
        groupId = 0;
        userName = new StringBuilder(user);
        groupName = new StringBuilder("None");
        size = 0;
    }

    /// <summary>
    ///     TarHeaders can be cloned.
    /// </summary>
    public object Clone()
    {
        var hdr = new TarHeader();

        hdr.name = name == null ? null : new StringBuilder(name.ToString());
        hdr.mode = mode;
        hdr.userId = userId;
        hdr.groupId = groupId;
        hdr.size = size;
        hdr.modTime = modTime;
        hdr.checkSum = checkSum;
        hdr.typeFlag = typeFlag;
        hdr.linkName = linkName == null ? null : new StringBuilder(linkName.ToString());
        hdr.magic = magic == null ? null : new StringBuilder(magic.ToString());
        hdr.version = version == null ? null : new StringBuilder(version.ToString());
        hdr.userName = userName == null ? null : new StringBuilder(userName.ToString());
        hdr.groupName = groupName == null ? null : new StringBuilder(groupName.ToString());
        hdr.devMajor = devMajor;
        hdr.devMinor = devMinor;

        return hdr;
    }

    /// <summary>
    ///     Get the name of this entry.
    /// </summary>
    /// <returns>
    ///     The entry's name.
    /// </returns>
    public string GetName() =>
        name.ToString();

    /// <summary>
    ///     Parse an octal string from a header buffer. This is used for the
    ///     file permission mode value.
    /// </summary>
    /// <param name="header">
    ///     The header buffer from which to parse.
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer from which to parse.
    /// </param>
    /// <param name="length">
    ///     The number of header bytes to parse.
    /// </param>
    /// <returns>
    ///     The long value of the octal string.
    /// </returns>
    public static long ParseOctal(byte[] header, int offset, int length)
    {
        long result = 0;
        var stillPadding = true;

        var end = offset + length;
        for (var i = offset; i < end; ++i)
        {
            if (header[i] == 0) break;

            if (header[i] == (byte)' ' || header[i] == '0')
            {
                if (stillPadding) continue;

                if (header[i] == (byte)' ') break;
            }

            stillPadding = false;

            result = (result << 3) + (header[i] - '0');
        }

        return result;
    }

    /// <summary>
    ///     Parse an entry name from a header buffer.
    /// </summary>
    /// <param name="header">
    ///     The header buffer from which to parse.
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer from which to parse.
    /// </param>
    /// <param name="length">
    ///     The number of header bytes to parse.
    /// </param>
    /// <returns>
    ///     The header's entry name.
    /// </returns>
    public static StringBuilder ParseName(byte[] header, int offset, int length)
    {
        var result = new StringBuilder(length);

        for (var i = offset; i < offset + length; ++i)
        {
            if (header[i] == 0) break;
            result.Append((char)header[i]);
        }

        return result;
    }

    /// <summary>
    ///     Add <paramref name="name">name</paramref> to the buffer as a collection of bytes
    /// </summary>
    /// <param name="name">the name to add</param>
    /// <param name="nameOffset">the offset of the first character</param>
    /// <param name="buf">the buffer to add to</param>
    /// <param name="bufferOffset">the index of the first byte to add</param>
    /// <param name="length">the number of characters/bytes to add</param>
    /// <returns>the next free index in the <paramref name="buf">buffer</paramref></returns>
    public static int GetNameBytes(StringBuilder name, int nameOffset, byte[] buf, int bufferOffset, int length)
    {
        int i;

        for (i = 0; i < length && nameOffset + i < name.Length; ++i) buf[bufferOffset + i] = (byte)name[nameOffset + i];

        for (; i < length; ++i) buf[bufferOffset + i] = 0;

        return bufferOffset + length;
    }

    /// <summary>
    ///     Add an entry name to the buffer
    /// </summary>
    /// <param name="name">
    ///     The name to add
    /// </param>
    /// <param name="buf">
    ///     The buffer to add to
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer from which to start adding
    /// </param>
    /// <param name="length">
    ///     The number of header bytes to add
    /// </param>
    /// <returns>
    ///     The index of the next free byte in the buffer
    /// </returns>
    public static int GetNameBytes(StringBuilder name, byte[] buf, int offset, int length) =>
        GetNameBytes(name, 0, buf, offset, length);

    /// <summary>
    ///     Put an octal representation of a value into a buffer
    /// </summary>
    /// <param name="val">
    ///     the value to be converted to octal
    /// </param>
    /// <param name="buf">
    ///     buffer to store the octal string
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer where the value starts
    /// </param>
    /// <param name="length">
    ///     The length of the octal string to create
    /// </param>
    /// <returns>
    ///     The offset of the character next byte after the octal string
    /// </returns>
    public static int GetOctalBytes(long val, byte[] buf, int offset, int length)
    {
        var idx = length - 1;

        // Either a space or null is valid here.  We use NULL as per GNUTar
        buf[offset + idx] = 0;
        --idx;

        if (val > 0)
            for (var v = val; idx >= 0 && v > 0; --idx)
            {
                buf[offset + idx] = (byte)((byte)'0' + (byte)(v & 7));
                v >>= 3;
            }

        for (; idx >= 0; --idx) buf[offset + idx] = (byte)'0';

        return offset + length;
    }

    /// <summary>
    ///     Put an octal representation of a value into a buffer
    /// </summary>
    /// <param name="val">
    ///     Value to be convert to octal
    /// </param>
    /// <param name="buf">
    ///     The buffer to update
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer to store the value
    /// </param>
    /// <param name="length">
    ///     The length of the octal string
    /// </param>
    /// <returns>
    ///     Index of next byte
    /// </returns>
    public static int GetLongOctalBytes(long val, byte[] buf, int offset, int length) =>
        GetOctalBytes(val, buf, offset, length);

    /// <summary>
    ///     Add the checksum octal integer to header buffer.
    /// </summary>
    /// <param name="val">
    /// </param>
    /// <param name="buf">
    ///     The header buffer to set the checksum for
    /// </param>
    /// <param name="offset">
    ///     The offset into the buffer for the checksum
    /// </param>
    /// <param name="length">
    ///     The number of header bytes to update.
    ///     It's formatted differently from the other fields: it has 6 digits, a
    ///     null, then a space -- rather than digits, a space, then a null.
    ///     The final space is already there, from checksumming
    /// </param>
    /// <returns>
    ///     The modified buffer offset
    /// </returns>
    private static int GetCheckSumOctalBytes(long val, byte[] buf, int offset, int length)
    {
        GetOctalBytes(val, buf, offset, length - 1);
//			buf[offset + length - 1] = (byte)' ';  -jr- 23-Jan-2004 this causes failure!!!
//			buf[offset + length - 2] = 0;
        return offset + length;
    }

    /// <summary>
    ///     Compute the checksum for a tar entry header.
    ///     The checksum field must be all spaces prior to this happening
    /// </summary>
    /// <param name="buf">
    ///     The tar entry's header buffer.
    /// </param>
    /// <returns>
    ///     The computed checksum.
    /// </returns>
    private static long ComputeCheckSum(byte[] buf)
    {
        long sum = 0;
        for (var i = 0; i < buf.Length; ++i) sum += buf[i];
        return sum;
    }
//		readonly static DateTime datetTime1970        = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime(); // -jr- Should be UTC?  doesnt match Gnutar if this is so though, why?

    private static int GetCTime(DateTime dateTime) =>
        (int)((dateTime.Ticks - datetTime1970.Ticks) / timeConversionFactor);

    private static DateTime GetDateTimeFromCTime(long ticks) =>
        new DateTime(datetTime1970.Ticks + ticks * timeConversionFactor);

    /// <summary>
    ///     Parse TarHeader information from a header buffer.
    /// </summary>
    /// <param name="header">
    ///     The tar entry header buffer to get information from.
    /// </param>
    public void ParseBuffer(byte[] header)
    {
        var offset = 0;

        name = ParseName(header, offset, NAMELEN);
        offset += NAMELEN;

        mode = (int)ParseOctal(header, offset, MODELEN);
        offset += MODELEN;

        userId = (int)ParseOctal(header, offset, UIDLEN);
        offset += UIDLEN;

        groupId = (int)ParseOctal(header, offset, GIDLEN);
        offset += GIDLEN;

        size = ParseOctal(header, offset, SIZELEN);
        offset += SIZELEN;

        modTime = GetDateTimeFromCTime(ParseOctal(header, offset, MODTIMELEN));
        offset += MODTIMELEN;

        checkSum = (int)ParseOctal(header, offset, CHKSUMLEN);
        offset += CHKSUMLEN;

        typeFlag = header[offset++];

        linkName = ParseName(header, offset, NAMELEN);
        offset += NAMELEN;

        magic = ParseName(header, offset, MAGICLEN);
        offset += MAGICLEN;

        version = ParseName(header, offset, VERSIONLEN);
        offset += VERSIONLEN;

        userName = ParseName(header, offset, UNAMELEN);
        offset += UNAMELEN;

        groupName = ParseName(header, offset, GNAMELEN);
        offset += GNAMELEN;

        devMajor = (int)ParseOctal(header, offset, DEVLEN);
        offset += DEVLEN;

        devMinor = (int)ParseOctal(header, offset, DEVLEN);

        // Fields past this point not currently parsed or used...
    }

    /// <summary>
    ///     'Write' header information to buffer provided
    /// </summary>
    /// <param name="outbuf">output buffer for header information</param>
    public void WriteHeader(byte[] outbuf)
    {
        var offset = 0;

        offset = GetNameBytes(name, outbuf, offset, NAMELEN);
        offset = GetOctalBytes(mode, outbuf, offset, MODELEN);
        offset = GetOctalBytes(userId, outbuf, offset, UIDLEN);
        offset = GetOctalBytes(groupId, outbuf, offset, GIDLEN);

        var size = this.size;

        offset = GetLongOctalBytes(size, outbuf, offset, SIZELEN);
        offset = GetLongOctalBytes(GetCTime(modTime), outbuf, offset, MODTIMELEN);

        var csOffset = offset;
        for (var c = 0; c < CHKSUMLEN; ++c) outbuf[offset++] = (byte)' ';

        outbuf[offset++] = typeFlag;

        offset = GetNameBytes(linkName, outbuf, offset, NAMELEN);
        offset = GetNameBytes(magic, outbuf, offset, MAGICLEN);
        offset = GetNameBytes(version, outbuf, offset, VERSIONLEN);
        offset = GetNameBytes(userName, outbuf, offset, UNAMELEN);
        offset = GetNameBytes(groupName, outbuf, offset, GNAMELEN);

        if (typeFlag == LF_CHR || typeFlag == LF_BLK)
        {
            offset = GetOctalBytes(devMajor, outbuf, offset, DEVLEN);
            offset = GetOctalBytes(devMinor, outbuf, offset, DEVLEN);
        }

        for (; offset < outbuf.Length;) outbuf[offset++] = 0;

        var checkSum = ComputeCheckSum(outbuf);

        GetCheckSumOctalBytes(checkSum, outbuf, csOffset, CHKSUMLEN);
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