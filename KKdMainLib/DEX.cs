﻿using System.Collections.Generic;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdMainLib.F2nd;

namespace KKdMainLib
{
    public class DEX
    {
        public DEX()
        { Dex = null; Header = new Header(); }

        private int Offset = 0;
        private Header Header;
        private Stream IO;

        public EXP[] Dex;

        public int DEXReader(string filepath, string ext)
        {
            Header = new Header();
            IO = File.OpenReader(filepath + ext);

            Header.Format = Format.F;
            Header.SectionSignature = IO.ReadInt32();
            if (Header.SectionSignature == 0x43505845)
                Header = IO.ReadHeader(true);
            if (Header.SectionSignature != 0x64)
                return 0;

            Offset = IO.Position - 0x4;
            Dex = new EXP[IO.ReadInt32()];
            int DEXOffset = IO.ReadInt32();
            if (IO.ReadInt32() == 0x00) Header.Format = Format.X;
            int DEXNameOffset = IO.ReadInt32();
            if (Header.IsX) IO.ReadInt32();

            IO.Seek(DEXOffset + Offset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
                Dex[i0] = new EXP { Main = new List<EXPElement>(), Eyes = new List<EXPElement>() };

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
                Dex[i0].EyesOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
            }
            IO.Seek(DEXNameOffset + Offset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
            }

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                EXPElement element = new EXPElement();
                IO.Seek(Dex[i0].MainOffset + Offset, 0);
                while (true)
                {
                    element.Frame = IO.ReadSingle();
                    element.Both  = IO.ReadUInt16();
                    element.ID    = IO.ReadUInt16();
                    element.Value = IO.ReadSingle();
                    element.Trans = IO.ReadSingle();
                    Dex[i0].Main.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                IO.Seek(Dex[i0].EyesOffset + Offset, 0);
                while(true)
                {
                    element.Frame = IO.ReadSingle();
                    element.Both  = IO.ReadUInt16();
                    element.ID    = IO.ReadUInt16();
                    element.Value = IO.ReadSingle();
                    element.Trans = IO.ReadSingle();
                    Dex[i0].Eyes.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                IO.Seek(Dex[i0].NameOffset + Offset, 0);
                Dex[i0].Name = IO.NullTerminatedUTF8();
            }

            IO.Close();
            return 1;
        }

        public void DEXWriter(string filepath, Format Format)
        {
            Header = new Header();
            IO = File.OpenWriter(filepath + (Format > Format.F ? ".dex" : ".bin"), true);
            Header.Format = IO.Format = Format;

            IO.Offset = Format > Format.F ? 0x20 : 0;
            IO.Write(0x64);
            IO.Write(Dex.Length);

            IO.WriteX(Header.IsX ? 0x28 : 0x20);
            IO.WriteX(0x00);

            int Position0 = IO.Position;
            IO.Write(0x00L);
            IO.Write(0x00L);

            for (int i = 0; i < Dex.Length * 3; i++) IO.WriteX(0x00);

            IO.Align(0x20, true);

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.Position;
                for (int i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    IO.Write(Dex[i0].Main[i1].Frame);
                    IO.Write(Dex[i0].Main[i1].Both );
                    IO.Write(Dex[i0].Main[i1].ID   );
                    IO.Write(Dex[i0].Main[i1].Value);
                    IO.Write(Dex[i0].Main[i1].Trans);
                }
                IO.Align(0x20, true);

                Dex[i0].EyesOffset = IO.Position;
                for (int i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                {
                    IO.Write(Dex[i0].Eyes[i1].Frame);
                    IO.Write(Dex[i0].Eyes[i1].Both );
                    IO.Write(Dex[i0].Eyes[i1].ID   );
                    IO.Write(Dex[i0].Eyes[i1].Value);
                    IO.Write(Dex[i0].Eyes[i1].Trans);
                }
                IO.Align(0x20, true);
            }
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.Position;
                IO.Write(Dex[i0].Name + "\0");
            }
            IO.Align(0x10, true);

            IO.Position = Header.IsX ? 0x28 : 0x20;
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                IO.WriteX(Dex[i0].MainOffset);
                IO.WriteX(Dex[i0].EyesOffset);
            }
            int Position1 = IO.Position;
            for (int i0 = 0; i0 < Dex.Length; i0++)
                IO.WriteX(Dex[i0].NameOffset);

            IO.Position = Position0 - (Header.IsX ? 8 : 4);
            IO.Write(Position1);

            if (Format > Format.F)
            {
                Offset = IO.Length;
                IO.Offset = 0;
                IO.Position = IO.Length;
                IO.WriteEOFC(0);
                IO.Position = 0;
                Header.DataSize = Offset;
                Header.SectionSize = Offset;
                Header.Signature = 0x43505845;
                IO.Write(Header, true);
            }
            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            this.Dex = new EXP[0];
            Header = new Header();

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if (!MsgPack.ElementArray("Dex", out MsgPack Dex)) return;

            this.Dex = new EXP[Dex.Array.Length];
            for (i0 = 0; i0 < this.Dex.Length; i0++)
            {
                this.Dex[i0] = new EXP { Name = Dex[i0].ReadString("Name") };

                if (Dex[i0].ElementArray("Main", out MsgPack Main))
                {
                    this.Dex[i0].Main = new List<EXPElement>();
                    for (i1 = 0; i1 < Main.Array.Length; i1++)
                        this.Dex[i0].Main.Add(ReadEXP(Main[i1]));
                }
                if (Dex[i0].ElementArray("Eyes", out MsgPack Eyes))
                {
                    this.Dex[i0].Eyes = new List<EXPElement>();
                    for (i1 = 0; i1 < Eyes.Array.Length; i1++)
                        this.Dex[i0].Eyes.Add(ReadEXP(Eyes[i1]));
                }
            }
            MsgPack = MsgPack.New;
        }

        private EXPElement ReadEXP(MsgPack mp) =>
            new EXPElement() { Frame = mp.ReadSingle("F"), Both  = mp.ReadUInt16("B"),
                               ID    = mp.ReadUInt16("I"), Value = mp.ReadSingle("V"),
                               Trans = mp.ReadSingle("T") };

        public void MsgPackWriter(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            MsgPack Dex = new MsgPack(this.Dex.Length, "Dex");
            for (i0 = 0; i0 < this.Dex.Length; i0++)
            {
                MsgPack EXP = MsgPack.New.Add("Name", this.Dex[i0].Name);
                MsgPack Main = new MsgPack(this.Dex[i0].Main.Count, "Main");
                for (i1 = 0; i1 < this.Dex[i0].Main.Count; i1++)
                    Main[i1] = WriteEXP(this.Dex[i0].Main[i1]);
                EXP.Add(Main);

                MsgPack Eyes = new MsgPack(this.Dex[i0].Eyes.Count, "Eyes");
                for (i1 = 0; i1 < this.Dex[i0].Eyes.Count; i1++)
                    Eyes[i1] = WriteEXP(this.Dex[i0].Eyes[i1]);
                EXP.Add(Eyes);
                Dex[i0] = EXP;
            }

            Dex.Write(true, file, JSON);
        }

        private MsgPack WriteEXP(EXPElement element) =>
            MsgPack.New.Add("F", element.Frame).Add("B", element.Both ).Add("I", element.ID   )
                         .Add("V", element.Value).Add("T", element.Trans);

        public struct EXP
        {
            public int MainOffset;
            public int EyesOffset;
            public int NameOffset;
            public string Name;
            public List<EXPElement> Main;
            public List<EXPElement> Eyes;
        }

        public struct EXPElement
        {
            public  float Frame;
            public ushort Both;
            public ushort ID;
            public  float Value;
            public  float Trans;
        }
    }
}
