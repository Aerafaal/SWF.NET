// BZip2InputStream.cs
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
using SWF.NET.ZIP.Checksums;

namespace SWF.NET.ZIP.BZip2;

/// <summary>
///     An input stream that decompresses files in the BZip2 format
/// </summary>
public class BZip2InputStream : Stream
{
    private const int START_BLOCK_STATE = 1;
    private const int RAND_PART_A_STATE = 2;
    private const int RAND_PART_B_STATE = 3;
    private const int RAND_PART_C_STATE = 4;
    private const int NO_RAND_PART_A_STATE = 5;
    private const int NO_RAND_PART_B_STATE = 6;
    private const int NO_RAND_PART_C_STATE = 7;
    private readonly int[][] baseArray = new int[BZip2Constants.N_GROUPS][];

    private Stream baseStream;

    private bool blockRandomised;

    /*--
    always: in the range 0 .. 9.
    The current block size is 100000 * this number.
    --*/
    private int blockSize100k;

    private int bsBuff;
    private int bsLive;
    private int computedBlockCRC;
    private uint computedCombinedCRC;

    private int count, chPrev, ch2;

    private int currentChar = -1;

    private int currentState = START_BLOCK_STATE;
    private int i2, j2;

    private readonly bool[] inUse = new bool[256];

    /*--
    index of the last char in the block, so
    the block size == last + 1.
    --*/
    private int last;

    private readonly int[][] limit = new int[BZip2Constants.N_GROUPS][];
    private byte[] ll8;
    private readonly IChecksum mCrc = new StrangeCRC();
    private readonly int[] minLens = new int[BZip2Constants.N_GROUPS];
    private int nInUse;

    /*--
    index in zptr[] of original string after sorting.
    --*/
    private int origPtr;
    private readonly int[][] perm = new int[BZip2Constants.N_GROUPS][];
    private int rNToGo;
    private int rTPos;

    private readonly byte[] selector = new byte[BZip2Constants.MAX_SELECTORS];
    private readonly byte[] selectorMtf = new byte[BZip2Constants.MAX_SELECTORS];

    private readonly byte[] seqToUnseq = new byte[256];

    private int storedBlockCRC, storedCombinedCRC;
    private bool streamEnd;
    private int tPos;

    private int[] tt;
    private readonly byte[] unseqToSeq = new byte[256];

    /*--
    freq table collected to save a pass over the data
    during decompression.
    --*/
    private readonly int[] unzftab = new int[256];
    private byte z;

    /// <summary>
    ///     Gets a value indicating if the stream supports reading
    /// </summary>
    public override bool CanRead =>
        baseStream.CanRead;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    public override bool CanSeek =>
        baseStream.CanSeek;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports writing.
    ///     This property always returns false
    /// </summary>
    public override bool CanWrite =>
        //				return baseStream.CanWrite;
        false;

    /// <summary>
    ///     Gets the length in bytes of the stream.
    /// </summary>
    public override long Length =>
        baseStream.Length;

    /// <summary>
    ///     Gets or sets the streams position
    ///     Setting the position is not supported and will throw a NotSupportException
    /// </summary>
    /// <exception cref="NotSupportedException">Any attempt to set the position</exception>
    public override long Position
    {
        get => baseStream.Position;
        set =>
            throw
                //				baseStream.Position = value;
                new NotSupportedException("BZip2InputStream position cannot be set");
    }

    /// <summary>
    ///     Construct instance for reading from stream
    /// </summary>
    /// <param name="stream">Data source</param>
    public BZip2InputStream(Stream stream)
    {
        // init arrays
        for (var i = 0; i < BZip2Constants.N_GROUPS; ++i)
        {
            limit[i] = new int[BZip2Constants.MAX_ALPHA_SIZE];
            baseArray[i] = new int[BZip2Constants.MAX_ALPHA_SIZE];
            perm[i] = new int[BZip2Constants.MAX_ALPHA_SIZE];
        }

        ll8 = null;
        tt = null;
        BsSetStream(stream);
        Initialize();
        InitBlock();
        SetupBlock();
    }

    /// <summary>
    ///     Flushes the baseInputStream
    /// </summary>
    public override void Flush()
    {
        if (baseStream != null) baseStream.Flush();
    }

    /// <summary>
    ///     Set the streams position.  This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException("BZip2InputStream Seek not supported");

    /// <summary>
    ///     Sets the length of this stream to the given value.
    ///     This operation is not supported and will throw a NotSupportedExceptionortedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void SetLength(long val) =>
        throw new NotSupportedException("BZip2InputStream SetLength not supported");

    /// <summary>
    ///     Writes a block of bytes to this stream using data from a buffer.
    ///     This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void Write(byte[] array, int offset, int count) =>
        throw new NotSupportedException("BZip2InputStream Write not supported");

    /// <summary>
    ///     Writes a byte to the current position in the file stream.
    ///     This operation is not supported and will throw a NotSupportedException
    /// </summary>
    /// <exception cref="NotSupportedException">Any access</exception>
    public override void WriteByte(byte val) =>
        throw new NotSupportedException("BZip2InputStream WriteByte not supported");

    /// <summary>
    ///     Read a sequence of bytes and advance position in stream
    /// </summary>
    /// <param name="b">Array of bytes to store values in</param>
    /// <param name="off">Offset in array to begin storing data</param>
    /// <param name="len">The maximum number of bytes to read</param>
    /// <returns>
    ///     The total number of bytes read into the buffer. This might be less
    ///     than the number of bytes requested if that number of bytes are not
    ///     currently available or zero if the end of the stream is reached.
    /// </returns>
    public override int Read(byte[] b, int off, int len)
    {
        for (var i = 0; i < len; ++i)
        {
            var rb = ReadByte();
            if (rb == -1) return i;
            b[off + i] = (byte)rb;
        }

        return len;
    }

    /// <summary>
    ///     Closes the input stream
    /// </summary>
    public override void Close()
    {
        if (baseStream != null) baseStream.Close();
    }

    private void MakeMaps()
    {
        nInUse = 0;
        for (var i = 0; i < 256; ++i)
            if (inUse[i])
            {
                seqToUnseq[nInUse] = (byte)i;
                unseqToSeq[i] = (byte)nInUse;
                nInUse++;
            }
    }

    /// <summary>
    ///     Read a byte from stream advancing position
    /// </summary>
    /// <returns>byte read or -1 on end of stream</returns>
    public override int ReadByte()
    {
        if (streamEnd) return -1; // ok

        var retChar = currentChar;
        switch (currentState)
        {
            case RAND_PART_B_STATE:
                SetupRandPartB();
                break;
            case RAND_PART_C_STATE:
                SetupRandPartC();
                break;
            case NO_RAND_PART_B_STATE:
                SetupNoRandPartB();
                break;
            case NO_RAND_PART_C_STATE:
                SetupNoRandPartC();
                break;
            case START_BLOCK_STATE:
            case NO_RAND_PART_A_STATE:
            case RAND_PART_A_STATE:
                break;
        }

        return retChar;
    }

    private void Initialize()
    {
        var magic1 = BsGetUChar();
        var magic2 = BsGetUChar();

        var magic3 = BsGetUChar();
        var magic4 = BsGetUChar();

        if (magic1 != 'B' || magic2 != 'Z' || magic3 != 'h' || magic4 < '1' || magic4 > '9')
        {
            streamEnd = true;
            return;
        }

        SetDecompressStructureSizes(magic4 - '0');
        computedCombinedCRC = 0;
    }

    private void InitBlock()
    {
        var magic1 = BsGetUChar();
        var magic2 = BsGetUChar();
        var magic3 = BsGetUChar();
        var magic4 = BsGetUChar();
        var magic5 = BsGetUChar();
        var magic6 = BsGetUChar();

        if (magic1 == 0x17 && magic2 == 0x72 && magic3 == 0x45 && magic4 == 0x38 && magic5 == 0x50 && magic6 == 0x90)
        {
            Complete();
            return;
        }

        if (magic1 != 0x31 || magic2 != 0x41 || magic3 != 0x59 || magic4 != 0x26 || magic5 != 0x53 || magic6 != 0x59)
        {
            BadBlockHeader();
            streamEnd = true;
            return;
        }

        storedBlockCRC = BsGetInt32();

        blockRandomised = BsR(1) == 1;

        GetAndMoveToFrontDecode();

        mCrc.Reset();
        currentState = START_BLOCK_STATE;
    }

    private void EndBlock()
    {
        computedBlockCRC = (int)mCrc.Value;

        /*-- A bad CRC is considered a fatal error. --*/
        if (storedBlockCRC != computedBlockCRC) CrcError();

        // 1528150659
        computedCombinedCRC = ((computedCombinedCRC << 1) & 0xFFFFFFFF) | (computedCombinedCRC >> 31);
        computedCombinedCRC = computedCombinedCRC ^ (uint)computedBlockCRC;
    }

    private void Complete()
    {
        storedCombinedCRC = BsGetInt32();
        if (storedCombinedCRC != (int)computedCombinedCRC) CrcError();

        streamEnd = true;
    }

    private static void CompressedStreamEOF() =>
        throw new ApplicationException("BZip2 input stream end of compressed stream");

    private static void BlockOverrun() =>
        throw new ApplicationException("BZip2 input stream block overrun");

    private static void BadBlockHeader() =>
        throw new ApplicationException("BZip2 input stream bad block header");

    private static void CrcError() =>
        throw new ApplicationException("BZip2 input stream crc error");


    private void BsSetStream(Stream f)
    {
        baseStream = f;
        bsLive = 0;
        bsBuff = 0;
    }

    private void FillBuffer()
    {
        var thech = 0;

        try
        {
            thech = baseStream.ReadByte();
        }
        catch (Exception)
        {
            CompressedStreamEOF();
        }

        if (thech == -1) CompressedStreamEOF();

        bsBuff = (bsBuff << 8) | (thech & 0xFF);
        bsLive += 8;
    }

    private int BsR(int n)
    {
        while (bsLive < n) FillBuffer();

        var v = (bsBuff >> (bsLive - n)) & ((1 << n) - 1);
        bsLive -= n;
        return v;
    }

    private char BsGetUChar() =>
        (char)BsR(8);

    private int BsGetint()
    {
        var u = 0;
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        u = (u << 8) | BsR(8);
        return u;
    }

    private int BsGetIntVS(int numBits) =>
        BsR(numBits);

    private int BsGetInt32() =>
        BsGetint();

    private void HbCreateDecodeTables(int[] limit, int[] baseArray, int[] perm, char[] length, int minLen, int maxLen, int alphaSize)
    {
        var pp = 0;

        for (var i = minLen; i <= maxLen; ++i)
        for (var j = 0; j < alphaSize; ++j)
            if (length[j] == i)
            {
                perm[pp] = j;
                ++pp;
            }

        for (var i = 0; i < BZip2Constants.MAX_CODE_LEN; i++) baseArray[i] = 0;

        for (var i = 0; i < alphaSize; i++) ++baseArray[length[i] + 1];

        for (var i = 1; i < BZip2Constants.MAX_CODE_LEN; i++) baseArray[i] += baseArray[i - 1];

        for (var i = 0; i < BZip2Constants.MAX_CODE_LEN; i++) limit[i] = 0;

        var vec = 0;

        for (var i = minLen; i <= maxLen; i++)
        {
            vec += baseArray[i + 1] - baseArray[i];
            limit[i] = vec - 1;
            vec <<= 1;
        }

        for (var i = minLen + 1; i <= maxLen; i++) baseArray[i] = ((limit[i - 1] + 1) << 1) - baseArray[i];
    }

    private void RecvDecodingTables()
    {
        var len = new char[BZip2Constants.N_GROUPS][];
        for (var i = 0; i < BZip2Constants.N_GROUPS; ++i) len[i] = new char[BZip2Constants.MAX_ALPHA_SIZE];

        var inUse16 = new bool[16];

        /*--- Receive the mapping table ---*/
        for (var i = 0; i < 16; i++) inUse16[i] = BsR(1) == 1;

        for (var i = 0; i < 16; i++)
            if (inUse16[i])
                for (var j = 0; j < 16; j++)
                    inUse[i * 16 + j] = BsR(1) == 1;
            else
                for (var j = 0; j < 16; j++)
                    inUse[i * 16 + j] = false;

        MakeMaps();
        var alphaSize = nInUse + 2;

        /*--- Now the selectors ---*/
        var nGroups = BsR(3);
        var nSelectors = BsR(15);

        for (var i = 0; i < nSelectors; i++)
        {
            var j = 0;
            while (BsR(1) == 1) j++;
            selectorMtf[i] = (byte)j;
        }

        /*--- Undo the MTF values for the selectors. ---*/
        var pos = new byte[BZip2Constants.N_GROUPS];
        for (var v = 0; v < nGroups; v++) pos[v] = (byte)v;

        for (var i = 0; i < nSelectors; i++)
        {
            int v = selectorMtf[i];
            var tmp = pos[v];
            while (v > 0)
            {
                pos[v] = pos[v - 1];
                v--;
            }

            pos[0] = tmp;
            selector[i] = tmp;
        }

        /*--- Now the coding tables ---*/
        for (var t = 0; t < nGroups; t++)
        {
            var curr = BsR(5);
            for (var i = 0; i < alphaSize; i++)
            {
                while (BsR(1) == 1)
                    if (BsR(1) == 0)
                        curr++;
                    else
                        curr--;
                len[t][i] = (char)curr;
            }
        }

        /*--- Create the Huffman decoding tables ---*/
        for (var t = 0; t < nGroups; t++)
        {
            var minLen = 32;
            var maxLen = 0;
            for (var i = 0; i < alphaSize; i++)
            {
                maxLen = Math.Max(maxLen, len[t][i]);
                minLen = Math.Min(minLen, len[t][i]);
            }

            HbCreateDecodeTables(limit[t], baseArray[t], perm[t], len[t], minLen, maxLen, alphaSize);
            minLens[t] = minLen;
        }
    }

    private void GetAndMoveToFrontDecode()
    {
        var yy = new byte[256];
        int nextSym;

        var limitLast = BZip2Constants.baseBlockSize * blockSize100k;
        origPtr = BsGetIntVS(24);

        RecvDecodingTables();
        var EOB = nInUse + 1;
        var groupNo = -1;
        var groupPos = 0;

        /*--
        Setting up the unzftab entries here is not strictly
        necessary, but it does save having to do it later
        in a separate pass, and so saves a block's worth of
        cache misses.
        --*/
        for (var i = 0; i <= 255; i++) unzftab[i] = 0;

        for (var i = 0; i <= 255; i++) yy[i] = (byte)i;

        last = -1;

        if (groupPos == 0)
        {
            groupNo++;
            groupPos = BZip2Constants.G_SIZE;
        }

        groupPos--;
        int zt = selector[groupNo];
        var zn = minLens[zt];
        var zvec = BsR(zn);
        int zj;

        while (zvec > limit[zt][zn])
        {
            if (zn > 20) // the longest code
                throw new ApplicationException("Bzip data error"); // -jr- 17-Dec-2003 from bzip 1.02 why 20???
            zn++;
            while (bsLive < 1) FillBuffer();
            zj = (bsBuff >> (bsLive - 1)) & 1;
            bsLive--;
            zvec = (zvec << 1) | zj;
        }

        if (zvec - baseArray[zt][zn] < 0 || zvec - baseArray[zt][zn] >= BZip2Constants.MAX_ALPHA_SIZE) throw new ApplicationException("Bzip data error"); // -jr- 17-Dec-2003 from bzip 1.02
        nextSym = perm[zt][zvec - baseArray[zt][zn]];

        while (true)
        {
            if (nextSym == EOB) break;

            if (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB)
            {
                var s = -1;
                var n = 1;
                do
                {
                    if (nextSym == BZip2Constants.RUNA)
                        s += (0 + 1) * n;
                    else if (nextSym == BZip2Constants.RUNB) s += (1 + 1) * n;

                    n <<= 1;

                    if (groupPos == 0)
                    {
                        groupNo++;
                        groupPos = BZip2Constants.G_SIZE;
                    }

                    groupPos--;

                    zt = selector[groupNo];
                    zn = minLens[zt];
                    zvec = BsR(zn);

                    while (zvec > limit[zt][zn])
                    {
                        zn++;
                        while (bsLive < 1) FillBuffer();
                        zj = (bsBuff >> (bsLive - 1)) & 1;
                        bsLive--;
                        zvec = (zvec << 1) | zj;
                    }

                    nextSym = perm[zt][zvec - baseArray[zt][zn]];
                }
                while (nextSym == BZip2Constants.RUNA || nextSym == BZip2Constants.RUNB);

                s++;
                var ch = seqToUnseq[yy[0]];
                unzftab[ch] += s;

                while (s > 0)
                {
                    last++;
                    ll8[last] = ch;
                    s--;
                }

                if (last >= limitLast) BlockOverrun();
                continue;
            }

            last++;
            if (last >= limitLast) BlockOverrun();

            var tmp = yy[nextSym - 1];
            unzftab[seqToUnseq[tmp]]++;
            ll8[last] = seqToUnseq[tmp];

            for (var j = nextSym - 1; j > 0; --j) yy[j] = yy[j - 1];
            yy[0] = tmp;

            if (groupPos == 0)
            {
                groupNo++;
                groupPos = BZip2Constants.G_SIZE;
            }

            groupPos--;
            zt = selector[groupNo];
            zn = minLens[zt];
            zvec = BsR(zn);
            while (zvec > limit[zt][zn])
            {
                zn++;
                while (bsLive < 1) FillBuffer();
                zj = (bsBuff >> (bsLive - 1)) & 1;
                bsLive--;
                zvec = (zvec << 1) | zj;
            }

            nextSym = perm[zt][zvec - baseArray[zt][zn]];
        }
    }

    private void SetupBlock()
    {
        var cftab = new int[257];

        cftab[0] = 0;
        Array.Copy(unzftab, 0, cftab, 1, 256);

        for (var i = 1; i <= 256; i++) cftab[i] += cftab[i - 1];

        for (var i = 0; i <= last; i++)
        {
            var ch = ll8[i];
            tt[cftab[ch]] = i;
            cftab[ch]++;
        }

        cftab = null;

        tPos = tt[origPtr];

        count = 0;
        i2 = 0;
        ch2 = 256; /*-- not a char and not EOF --*/

        if (blockRandomised)
        {
            rNToGo = 0;
            rTPos = 0;
            SetupRandPartA();
        }
        else
            SetupNoRandPartA();
    }

    private void SetupRandPartA()
    {
        if (i2 <= last)
        {
            chPrev = ch2;
            ch2 = ll8[tPos];
            tPos = tt[tPos];
            if (rNToGo == 0)
            {
                rNToGo = BZip2Constants.rNums[rTPos];
                rTPos++;
                if (rTPos == 512) rTPos = 0;
            }

            rNToGo--;
            ch2 ^= rNToGo == 1 ? 1 : 0;
            i2++;

            currentChar = ch2;
            currentState = RAND_PART_B_STATE;
            mCrc.Update(ch2);
        }
        else
        {
            EndBlock();
            InitBlock();
            SetupBlock();
        }
    }

    private void SetupNoRandPartA()
    {
        if (i2 <= last)
        {
            chPrev = ch2;
            ch2 = ll8[tPos];
            tPos = tt[tPos];
            i2++;

            currentChar = ch2;
            currentState = NO_RAND_PART_B_STATE;
            mCrc.Update(ch2);
        }
        else
        {
            EndBlock();
            InitBlock();
            SetupBlock();
        }
    }

    private void SetupRandPartB()
    {
        if (ch2 != chPrev)
        {
            currentState = RAND_PART_A_STATE;
            count = 1;
            SetupRandPartA();
        }
        else
        {
            count++;
            if (count >= 4)
            {
                z = ll8[tPos];
                tPos = tt[tPos];
                if (rNToGo == 0)
                {
                    rNToGo = BZip2Constants.rNums[rTPos];
                    rTPos++;
                    if (rTPos == 512) rTPos = 0;
                }

                rNToGo--;
                z ^= (byte)(rNToGo == 1 ? 1 : 0);
                j2 = 0;
                currentState = RAND_PART_C_STATE;
                SetupRandPartC();
            }
            else
            {
                currentState = RAND_PART_A_STATE;
                SetupRandPartA();
            }
        }
    }

    private void SetupRandPartC()
    {
        if (j2 < z)
        {
            currentChar = ch2;
            mCrc.Update(ch2);
            j2++;
        }
        else
        {
            currentState = RAND_PART_A_STATE;
            i2++;
            count = 0;
            SetupRandPartA();
        }
    }

    private void SetupNoRandPartB()
    {
        if (ch2 != chPrev)
        {
            currentState = NO_RAND_PART_A_STATE;
            count = 1;
            SetupNoRandPartA();
        }
        else
        {
            count++;
            if (count >= 4)
            {
                z = ll8[tPos];
                tPos = tt[tPos];
                currentState = NO_RAND_PART_C_STATE;
                j2 = 0;
                SetupNoRandPartC();
            }
            else
            {
                currentState = NO_RAND_PART_A_STATE;
                SetupNoRandPartA();
            }
        }
    }

    private void SetupNoRandPartC()
    {
        if (j2 < z)
        {
            currentChar = ch2;
            mCrc.Update(ch2);
            j2++;
        }
        else
        {
            currentState = NO_RAND_PART_A_STATE;
            i2++;
            count = 0;
            SetupNoRandPartA();
        }
    }

    private void SetDecompressStructureSizes(int newSize100k)
    {
        if (!(0 <= newSize100k && newSize100k <= 9 && 0 <= blockSize100k && blockSize100k <= 9)) throw new ApplicationException("Invalid block size");

        blockSize100k = newSize100k;

        if (newSize100k == 0) return;

        var n = BZip2Constants.baseBlockSize * newSize100k;
        ll8 = new byte[n];
        tt = new int[n];
    }
}
/* This file was derived from a file containing under this license:
 * 
 * This file is a part of bzip2 and/or libbzip2, a program and
 * library for lossless, block-sorting data compression.
 * 
 * Copyright (C) 1996-1998 Julian R Seward.  All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. The origin of this software must not be misrepresented; you must 
 * not claim that you wrote the original software.  If you use this 
 * software in a product, an acknowledgment in the product 
 * documentation would be appreciated but is not required.
 * 
 * 3. Altered source versions must be plainly marked as such, and must
 * not be misrepresented as being the original software.
 * 
 * 4. The name of the author may not be used to endorse or promote 
 * products derived from this software without specific prior written 
 * permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * Java version ported by Keiron Liddle, Aftex Software <keiron@aftexsw.com> 1999-2001
 */