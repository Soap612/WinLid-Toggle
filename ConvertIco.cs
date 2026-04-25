using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program {
    static void Main(string[] args) {
        if (args.Length < 2) return;
        string inPath = args[0];
        string outPath = args[1];

        using (Bitmap bmp = new Bitmap(inPath))
        {
            // Windows icons can be up to 256x256
            using (Bitmap scaled = new Bitmap(bmp, new Size(256, 256)))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    scaled.Save(ms, ImageFormat.Png);
                    byte[] pngBytes = ms.ToArray();
                    
                    using (FileStream fs = new FileStream(outPath, FileMode.Create))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            bw.Write((short)0); // reserved
                            bw.Write((short)1); // icon type (1 for ICO)
                            bw.Write((short)1); // count (number of images)
                            
                            bw.Write((byte)0);  // width (0 means 256)
                            bw.Write((byte)0);  // height (0 means 256)
                            bw.Write((byte)0);  // color count
                            bw.Write((byte)0);  // reserved
                            bw.Write((short)1); // color planes
                            bw.Write((short)32); // bits per pixel
                            bw.Write((int)pngBytes.Length); // size of image bytes
                            bw.Write((int)22); // offset of image data from beginning of file
                            
                            bw.Write(pngBytes);
                        }
                    }
                }
            }
        }
        Console.WriteLine("Icon generated successfully.");
    }
}
