﻿using System;
using KKdBaseLib;
using MSIO = System.IO;
using ENRSDict = System.Collections.Generic.Dictionary<long, int>;

namespace KKdMainLib.IO
{
    public unsafe class Stream : IDisposable
    {
        private int I, i, BitRead, BitWrite, TempBitRead, TempBitWrite, ValRead, ValWrite;
        private byte[] b;
        private MSIO.Stream s;

        private bool getENRS;

        private Format format = Format.NULL;

        public Format Format
        {   get =>      format;
            set {       format = value;
                  IsBE = format == Format.F2BE;
                  IsX  = format == Format.X || format == Format.XHD; } }

        public ENRSDict ENRSDict;

        public bool IsBE = false;
        public bool IsX  = false;
        public bool GetENRS
        { get => getENRS;
          set { if (value && ENRSDict == null)
                    ENRSDict = new ENRSDict(); getENRS = value; } }

        public  int    O { get => ( int)I64O; set => I64O = value; }
        public uint U32O { get => (uint)I64O; set => I64O = value; }
        public long I64O;

        public  int    L { get => ( int)s.Length -    O; set => s.SetLength(value +    O); }
        public uint U32L { get => (uint)s.Length - U32O; set => s.SetLength(value + U32O); }
        public long I64L { get =>       s.Length - I64O; set => s.SetLength(value + I64O); }

        public  int    P
        { get => ( int)s.Position -    O; set => s.Position = value +     O; }
        public uint U32P
        { get => (uint)s.Position - U32O; set => s.Position = value + U32O; }
        public long I64P
        { get =>       s.Position - I64O; set => s.Position = value + I64O; }

        public bool CanRead    => s.CanRead;
        public bool CanSeek    => s.CanSeek;
        public bool CanTimeout => s.CanTimeout;
        public bool CanWrite   => s.CanWrite;

        public string File = null;

        public Stream(MSIO.Stream output = null, bool isBE = false)
        {
            if (output == null) output = MSIO.Stream.Null;
            I64O = 0;
            BitRead = 8;
            ValRead = ValRead = BitWrite = 0;
            s = output;
            Format = Format.NULL;
            b = new byte[0x100];
            this.IsBE = isBE;
        }

        public void C() => D(true);

        public void F() => s.Flush();

        public void SL(long length = 0) => s.SetLength(length);

        public long S(long offset, SeekOrigin origin = 0) =>
            s.Seek(offset + O, (MSIO.SeekOrigin)(int)origin);
        
        public long? S(long? offset, SeekOrigin origin)
        { if (offset == null) return null;
            return s.Seek((long)offset + O, (MSIO.SeekOrigin)(int)origin); }

        private bool disposed = false;
        public void D() => D(true);
        public void Dispose() => D(true);

        protected virtual void D(bool dispose)
        { CW(); if (ENRSDict != null) { ENRSDict.Clear(); ENRSDict = null; }
            if (s != MSIO.Stream.Null && !disposed) { disposed = true; s.Flush();
                s.Dispose(); } if (dispose) GC.SuppressFinalize(this); }

        public MSIO.Stream BS
        { get { s.Flush(); return s; } set { s = value; } }

        public void A(long align)
        {
            long Al = align - P % align;
            if (P % align != 0) s.Seek(P + O + Al, 0);
        }

        public void A(long align, bool SetLength)
        {
            if (SetLength) s.SetLength(P + O);
            long Al = align - P % align;
            if (P % align != 0) s.Seek(P + O + Al, 0);
            if (SetLength) s.SetLength(P + O);
        }

        public void A(long align, bool setLength0, bool setLength1)
        {
            if (setLength0) s.SetLength(P + O);
            long Al = align - P % align;
            if (P % align != 0) s.Seek(P + Al, 0);
            if (setLength1) s.SetLength(P + O);
        }

        public   bool RBo () =>        s.ReadByte() != 0;
        public  sbyte RI8 () => (sbyte)s.ReadByte();
        public   byte RU8 () => ( byte)s.ReadByte();
        public  short RI16() { s.Read(b, 0, 2); return b.TI16(); }
        public ushort RU16() { s.Read(b, 0, 2); return b.TU16(); }
        public    int RI32() { s.Read(b, 0, 4); return b.TI32(); }
        public   uint RU32() { s.Read(b, 0, 4); return b.TU32(); }
        public   long RI64() { s.Read(b, 0, 8); return b.TI64(); }
        public  ulong RU64() { s.Read(b, 0, 8); return b.TU64(); }
        public  float RF32() { s.Read(b, 0, 4); return b.TF32(); }
        public double RF64() { s.Read(b, 0, 8); return b.TF64(); }
        
        public  short RI16E() { s.Read(b, 0, 2); b.E(2, IsBE); return b.TI16(); }
        public ushort RU16E() { s.Read(b, 0, 2); b.E(2, IsBE); return b.TU16(); }
        public    int RI32E() { s.Read(b, 0, 4); b.E(4, IsBE); return b.TI32(); }
        public   uint RU32E() { s.Read(b, 0, 4); b.E(4, IsBE); return b.TU32(); }
        public   long RI64E() { s.Read(b, 0, 8); b.E(8, IsBE); return b.TI64(); }
        public  ulong RU64E() { s.Read(b, 0, 8); b.E(8, IsBE); return b.TU64(); }
        public  float RF32E() { s.Read(b, 0, 4); b.E(4, IsBE); return b.TF32(); }
        public double RF64E() { s.Read(b, 0, 8); b.E(8, IsBE); return b.TF64(); }

        public  short RI16E(bool isBE) { s.Read(b, 0, 2); b.E(2, isBE); return b.TI16(); }
        public ushort RU16E(bool isBE) { s.Read(b, 0, 2); b.E(2, isBE); return b.TU16(); }
        public    int RI32E(bool isBE) { s.Read(b, 0, 4); b.E(4, isBE); return b.TI32(); }
        public   uint RU32E(bool isBE) { s.Read(b, 0, 4); b.E(4, isBE); return b.TU32(); }
        public   long RI64E(bool isBE) { s.Read(b, 0, 8); b.E(8, isBE); return b.TI64(); }
        public  ulong RU64E(bool isBE) { s.Read(b, 0, 8); b.E(8, isBE); return b.TU64(); }
        public  float RF32E(bool isBE) { s.Read(b, 0, 4); b.E(4, isBE); return b.TF32(); }
        public double RF64E(bool isBE) { s.Read(b, 0, 8); b.E(8, isBE); return b.TF64(); }

        public void W(byte[] Val                        ) =>
            s.Write(Val ?? new byte[0],      0, Val.Length);
        public void W(byte[] Val,             int Length) =>
            s.Write(Val ?? new byte[0],      0,     Length);
        public void W(byte[] Val, int Offset, int Length) =>
            s.Write(Val ?? new byte[0], Offset,     Length);
        public void W(char[] val, bool UTF8 = true) =>
            W(UTF8 ? val.ToUTF8() : val.ToASCII());

        public void W(  bool val) => s.WriteByte((byte)(val ? 1 : 0));
        public void W( sbyte val) => s.WriteByte((byte) val);
        public void W(  byte val) => s.WriteByte(       val);
        public void W( short val) { b.GBy(val); s.Write(b, 0, 2); }
        public void W(ushort val) { b.GBy(val); s.Write(b, 0, 2); }
        public void W(   int val) { b.GBy(val); s.Write(b, 0, 4); }
        public void W(  uint val) { b.GBy(val); s.Write(b, 0, 4); }
        public void W(  long val) { b.GBy(val); s.Write(b, 0, 8); }
        public void W( ulong val) { b.GBy(val); s.Write(b, 0, 8); }
        public void W( float val) { b.GBy(val); s.Write(b, 0, 4); }
        public void W(double val) { b.GBy(val); s.Write(b, 0, 8); }

        public void W( sbyte? val) => W(val ?? default);
        public void W(  byte? val) => W(val ?? default);
        public void W( short? val) => W(val ?? default);
        public void W(ushort? val) => W(val ?? default);
        public void W(   int? val) => W(val ?? default);
        public void W(  uint? val) => W(val ?? default);
        public void W(  long? val) => W(val ?? default);
        public void W( ulong? val) => W(val ?? default);
        public void W( float? val) => W(val ?? default);
        public void W(double? val) => W(val ?? default);

        public void W(  char val, bool UTF8 = true) =>
            W(UTF8 ? val.ToString().ToUTF8() : val.ToString().ToASCII());
        public void W(string val, bool UTF8 = true) =>
            W(UTF8 ? val           .ToUTF8() : val           .ToASCII());
        
        public void WE( short val) { b.GBy(val); b.E(2, IsBE); s.Write(b, 0, 2); }
        public void WE(ushort val) { b.GBy(val); b.E(2, IsBE); s.Write(b, 0, 2); }
        public void WE(   int val) { b.GBy(val); b.E(4, IsBE); s.Write(b, 0, 4); }
        public void WE(  uint val) { b.GBy(val); b.E(4, IsBE); s.Write(b, 0, 4); }
        public void WE(  long val) { b.GBy(val); b.E(8, IsBE); s.Write(b, 0, 8); }
        public void WE( ulong val) { b.GBy(val); b.E(8, IsBE); s.Write(b, 0, 8); }
        public void WE( float val) { b.GBy(val); b.E(4, IsBE); s.Write(b, 0, 4); }
        public void WE(double val) { b.GBy(val); b.E(8, IsBE); s.Write(b, 0, 8); }

        public void WE( short val, bool isBE) { b.GBy(val); b.E(2, isBE); s.Write(b, 0, 2); }
        public void WE(ushort val, bool isBE) { b.GBy(val); b.E(2, isBE); s.Write(b, 0, 2); }
        public void WE(   int val, bool isBE) { b.GBy(val); b.E(4, isBE); s.Write(b, 0, 4); }
        public void WE(  uint val, bool isBE) { b.GBy(val); b.E(4, isBE); s.Write(b, 0, 4); }
        public void WE(  long val, bool isBE) { b.GBy(val); b.E(8, isBE); s.Write(b, 0, 8); }
        public void WE( ulong val, bool isBE) { b.GBy(val); b.E(8, isBE); s.Write(b, 0, 8); }
        public void WE( float val, bool isBE) { b.GBy(val); b.E(4, isBE); s.Write(b, 0, 4); }
        public void WE(double val, bool isBE) { b.GBy(val); b.E(8, isBE); s.Write(b, 0, 8); }

        private void RENRS(byte[] b, int c)
        { if (getENRS) ENRSDict.Add(P, c); s.Read(b, 0, c); }
        
        public  short RI16ENRS() { RENRS(b, 2); return b.TI16(); }
        public ushort RU16ENRS() { RENRS(b, 2); return b.TU16(); }
        public    int RI32ENRS() { RENRS(b, 4); return b.TI32(); }
        public   uint RU32ENRS() { RENRS(b, 4); return b.TU32(); }
        public   long RI64ENRS() { RENRS(b, 8); return b.TI64(); }
        public  ulong RU64ENRS() { RENRS(b, 8); return b.TU64(); }
        public  float RF32ENRS() { RENRS(b, 4); return b.TF32(); }
        public double RF64ENRS() { RENRS(b, 8); return b.TF64(); }
        
        public  short RI16ENRSE() { RENRS(b, 2); b.E(2, IsBE); return b.TI16(); }
        public ushort RU16ENRSE() { RENRS(b, 2); b.E(2, IsBE); return b.TU16(); }
        public    int RI32ENRSE() { RENRS(b, 4); b.E(4, IsBE); return b.TI32(); }
        public   uint RU32ENRSE() { RENRS(b, 4); b.E(4, IsBE); return b.TU32(); }
        public   long RI64ENRSE() { RENRS(b, 8); b.E(8, IsBE); return b.TI64(); }
        public  ulong RU64ENRSE() { RENRS(b, 8); b.E(8, IsBE); return b.TU64(); }
        public  float RF32ENRSE() { RENRS(b, 4); b.E(4, IsBE); return b.TF32(); }
        public double RF64ENRSE() { RENRS(b, 8); b.E(8, IsBE); return b.TF64(); }
        
        public  short RI16ENRSE(bool isBE) { RENRS(b, 2); b.E(2, isBE); return b.TI16(); }
        public ushort RU16ENRSE(bool isBE) { RENRS(b, 2); b.E(2, isBE); return b.TU16(); }
        public    int RI32ENRSE(bool isBE) { RENRS(b, 4); b.E(4, isBE); return b.TI32(); }
        public   uint RU32ENRSE(bool isBE) { RENRS(b, 4); b.E(4, isBE); return b.TU32(); }
        public   long RI64ENRSE(bool isBE) { RENRS(b, 8); b.E(8, isBE); return b.TI64(); }
        public  ulong RU64ENRSE(bool isBE) { RENRS(b, 8); b.E(8, isBE); return b.TU64(); }
        public  float RF32ENRSE(bool isBE) { RENRS(b, 4); b.E(4, isBE); return b.TF32(); }
        public double RF64ENRSE(bool isBE) { RENRS(b, 8); b.E(8, isBE); return b.TF64(); }

        private void WENRS(byte[] b, int c)
        { if (getENRS) ENRSDict.Add(P, c); s.Write(b, 0, c); }

        public void WENRS( short val) { b.GBy(val); WENRS(b, 2); }
        public void WENRS(ushort val) { b.GBy(val); WENRS(b, 2); }
        public void WENRS(   int val) { b.GBy(val); WENRS(b, 4); }
        public void WENRS(  uint val) { b.GBy(val); WENRS(b, 4); }
        public void WENRS(  long val) { b.GBy(val); WENRS(b, 8); }
        public void WENRS( ulong val) { b.GBy(val); WENRS(b, 8); }
        public void WENRS( float val) { b.GBy(val); WENRS(b, 4); }
        public void WENRS(double val) { b.GBy(val); WENRS(b, 8); }
        
        public void WENRSE( short val) { b.GBy(val); b.E(2, IsBE); WENRS(b, 2); }
        public void WENRSE(ushort val) { b.GBy(val); b.E(2, IsBE); WENRS(b, 2); }
        public void WENRSE(   int val) { b.GBy(val); b.E(4, IsBE); WENRS(b, 4); }
        public void WENRSE(  uint val) { b.GBy(val); b.E(4, IsBE); WENRS(b, 4); }
        public void WENRSE(  long val) { b.GBy(val); b.E(8, IsBE); WENRS(b, 8); }
        public void WENRSE( ulong val) { b.GBy(val); b.E(8, IsBE); WENRS(b, 8); }
        public void WENRSE( float val) { b.GBy(val); b.E(4, IsBE); WENRS(b, 4); }
        public void WENRSE(double val) { b.GBy(val); b.E(8, IsBE); WENRS(b, 8); }
        
        public void WENRSE( short val, bool isBE) { b.GBy(val); b.E(2, isBE); WENRS(b, 2); }
        public void WENRSE(ushort val, bool isBE) { b.GBy(val); b.E(2, isBE); WENRS(b, 2); }
        public void WENRSE(   int val, bool isBE) { b.GBy(val); b.E(4, isBE); WENRS(b, 4); }
        public void WENRSE(  uint val, bool isBE) { b.GBy(val); b.E(4, isBE); WENRS(b, 4); }
        public void WENRSE(  long val, bool isBE) { b.GBy(val); b.E(8, isBE); WENRS(b, 8); }
        public void WENRSE( ulong val, bool isBE) { b.GBy(val); b.E(8, isBE); WENRS(b, 8); }
        public void WENRSE( float val, bool isBE) { b.GBy(val); b.E(4, isBE); WENRS(b, 4); }
        public void WENRSE(double val, bool isBE) { b.GBy(val); b.E(8, isBE); WENRS(b, 8); }

        public Half RF16     (         ) { ushort a = RU16     (    ); return (Half)a; }
        public Half RF16E    (         ) { ushort a = RU16E    (    ); return (Half)a; }
        public Half RF16E    (bool isBE) { ushort a = RU16E    (isBE); return (Half)a; }
        public Half RF16ENRS (         ) { ushort a = RU16E    (    ); return (Half)a; }
        public Half RF16ENRSE(         ) { ushort a = RU16ENRSE(    ); return (Half)a; }
        public Half RF16ENRSE(bool isBE) { ushort a = RU16ENRSE(isBE); return (Half)a; }

        public void W     (Half val           ) => W     ((ushort)val      );
        public void WE    (Half val           ) => WE    ((ushort)val      );
        public void WE    (Half val, bool isBE) => WE    ((ushort)val, isBE);
        public void WENRS (Half val           ) => WENRS ((ushort)val      );
        public void WENRSE(Half val           ) => WENRSE((ushort)val      );
        public void WENRSE(Half val, bool isBE) => WENRSE((ushort)val, isBE);
        
        public char RC(bool UTF8 = true) => UTF8 ? RCUTF8() : (char)s.ReadByte();

        public char RCUTF8()
        {
            byte t;
            int T;
            int val = 0;
            for (I = 0, i = 4; I < i; I++)
            {
                T = s.ReadByte();
                if (T == -1) return '\uFFFF';
                t = (byte)T;

                     if ((t & 0xC0) == 0x80 && I >  0)   val = (val << 6) | (t & 0x3F);
                else if ((t & 0x80) == 0x00 && I == 0)   return (char)t;
                else if ((t & 0xE0) == 0xC0 && I == 0) { val = t & 0x1F; i = 2; }
                else if ((t & 0xF0) == 0xE0 && I == 0) { val = t & 0x0F; i = 3; }
                else if ((t & 0xF8) == 0xF0 && I == 0) { val = t & 0x07; i = 4; }
                else return '\uFFFF';
            }
            return (char)val;
        }

        public string RS(long Length, bool UTF8 = true) =>
            UTF8 ? RSUTF8(Length) : RSASCII(Length);

        public string RSUTF8 (long Length) => RBy(Length).ToUTF8 ();
        public string RSASCII(long Length) => RBy(Length).ToASCII();
        
        
        public string RS(long? Length, bool UTF8 = true) =>
            UTF8 ? RSUTF8(Length) : RSASCII(Length);

        public string RSUTF8 (long? Length) => RBy(Length).ToUTF8 ();
        public string RSASCII(long? Length) => RBy(Length).ToASCII();
        
        public byte[] RBy(long Length, int Offset = -1)
        { byte[] Buf = new byte[Length]; if (Offset > -1) s.Position = Offset;
            s.Read(Buf, 0, (int)Length); return Buf; }
        
        public void RBy(long Length, byte[] Buf, long Offset = -1)
        { if (Offset > -1) s.Position = Offset; s.Read(Buf, 0, (int)Length); }

        public byte[] RBy(long? Length, int Offset = -1)
        { if (Length == null) return new byte[0]; else return RBy((long)Length, Offset); }

        public void RBy(long  Length, byte Bits, byte[] Buf, long Offset = -1)
        { if (Offset > -1) s.Seek(Offset, 0);
                 if (Bits > 0 && Bits < 8) for (i = 0; i < Length; i++) Buf[i] = RBi(Bits); CR(); }
           
        public byte RBi(byte Bits)
        {
            BitRead += Bits;
            TempBitRead = 8 - BitRead;
            if (TempBitRead < 0)
            {
                BitRead = (byte)-TempBitRead;
                TempBitRead = 8 + TempBitRead;
                ValRead = (ushort)((ValRead << 8) | (byte)s.ReadByte());
            }
            return (byte)((ValRead >> TempBitRead) & ((1 << Bits) - 1));
        }

        public byte RHB() => RBi(4);
        
        public void W(int val, byte Bits)
        {
            val &= (1 << Bits) - 1;
            BitWrite += Bits;
            TempBitWrite = 8 - BitWrite;
            if (TempBitWrite < 0)
            {
                BitWrite = (byte)-TempBitWrite;
                TempBitWrite = 8 + TempBitWrite;
                s.WriteByte((byte)(ValWrite | (val >> BitWrite)));
                ValWrite = 0;
            }
            ValWrite |= val << TempBitWrite;
            ValWrite &= 0xFF;
        }

        public void CR() //CheckRead
        { if (BitRead  > 0)                                ValRead  = 0; BitRead  = 8;   }
        public void CW() //CheckWrite
        { if (BitWrite > 0) { s.WriteByte((byte)ValWrite); ValWrite = 0; BitWrite = 0; } }

        public byte[] ToArray(bool Close)
        { byte[] Data = ToArray(); if (Close) Dispose(); return Data; }

        public byte[] ToArray()
        {
            long Position = s.Position;
            byte[] Data = RBy(s.Length, 0);
            s.Position = Position;
            return Data;
        }
    }

    public enum SeekOrigin
    {
        Begin   = 0,
        Current = 1,
        End     = 2,
    }
}
