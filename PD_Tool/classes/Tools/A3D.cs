﻿using System;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdA3DA = KKdMainLib.A3DA;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool.Tools
{
    class A3D
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "A3DA Converter";
            Program.Choose(1, "a3da", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            bool MP = false;
            foreach (string file in FileNames)
                if (file.EndsWith(".mp") || file.EndsWith(".json") || file.EndsWith(".farc")) { MP = true; break; }

            Format Format = Format.NULL;
            string format = "";
            if (MP)
            {
                Console.Clear();
                Program.ConsoleDesign(true);
                Program.ConsoleDesign("          Choose type of format to export:");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign("1. A3DA [DT/AC/F]");
                Program.ConsoleDesign("2. A3DC [DT/AC/F]");
                Program.ConsoleDesign("3. A3DA [AFT/FT] ");
                Program.ConsoleDesign("4. A3DC [AFT/FT] ");
                Program.ConsoleDesign("5. A3DC [F2]     ");
                Program.ConsoleDesign("6. A3DC [MGF]    ");
                Program.ConsoleDesign("7. A3DC [X]      ");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign(true);
                Console.WriteLine();
                format = Console.ReadLine();
                     if (format == "1") Format = Format.DT  ;
                else if (format == "2") Format = Format.F   ;
                else if (format == "3") Format = Format.FT  ;
                else if (format == "4") Format = Format.FT  ;
                else if (format == "5") Format = Format.F2LE;
                else if (format == "6") Format = Format.MGF ;
                else if (format == "7") Format = Format.X   ;
                else return;
            }

            KKdA3DA A;
            int state;
            foreach (string file in FileNames)
            {
                A = new KKdA3DA();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                Console.Title = "A3DA Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".farc")
                    using (KKdFARC FARC = new KKdFARC(file))
                    {
                        if (!FARC.HeaderReader()) continue;
                        if (!FARC.HasFiles) continue;

                        MsgPack A3DA = MsgPack.Null;
                        byte[] data = null;
                        for (int i = 0; i < FARC.Files.Length; i++)
                        {
                            data = FARC.FileReader(i);
                            state = A.A3DAReader(data);
                            if (state == 1)
                            {
                                A3DA = A.MsgPackWriter();
                                A = new KKdA3DA();
                                A.MsgPackReader(A3DA);
                                A.Data._.CompressF16 = Format > Format.FT ? Format == Format.MGF ? 2 : 1 : 0;
                                A.Head.Format = Format;
                                FARC.Files[i].Data = (format != "1" && format != "3") ? A.A3DCWriter() : A.A3DAWriter();
                            }
                        }
                        FARC.Save();
                    }
                else if (ext == ".a3da")
                {
                    state = A.A3DAReader(filepath);
                    if (state == 1) A.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    A.MsgPackReader(filepath, ext == ".json");
                    A.Data._.CompressF16 = Format > Format.FT ? Format == Format.MGF ? 2 : 1 : 0;
                    A.Head.Format = Format;

                    File.WriteAllBytes(filepath + ".a3da", (format != "1" &&
                        format != "3") ? A.A3DCWriter() : A.A3DAWriter());
                }
                A = null;
            }
        }
    }
}
