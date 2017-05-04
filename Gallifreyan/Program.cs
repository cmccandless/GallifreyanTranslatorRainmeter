using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallifreyan
{
    class Program
    {
        static void Main(string[] args)
        {
            int argc = args.Count();

            string outFilePath;

            if (argc < 1 || args.Contains("--help"))
            {
                FPrint("Usage: AtoG.exe [out=<full path of file>] [color=<R,G,B[,A]>] [size=<px>] <value>\r\nOutputs a bitmap Gallifreyan representation of given value\r\n");
                return;
            }

            string wordStr = string.Join(" ", args.Where(a => !a.Contains('=')));
            outFilePath = string.Format("{0}.bmp", wordStr);

            foreach (var arg in args.Where(a => a.Contains('=')))
            {
                var key = arg.Split('=')[0];
                var value = arg.Split('=')[1];
                switch (key)
                {
                    case "width":
                        {
                            int width;
                            if (Int32.TryParse(value, out width))
                            {
                                Global.Thickness = width;
                            }
                            else
                            {
                                FPrint("Error: value for key 'width' not an integer");
                            }
                            break;
                        }
                    case "size":
                        {
                            int size;
                            if (Int32.TryParse(value, out size))
                            {
                                Global.Size = size;
                            }
                            else
                            {
                                FPrint("Error: value for key 'size' not an integer");
                            }
                            break;
                        }
                    case "out":
                        {
                            outFilePath = value;
                            if (!outFilePath.Contains(".bmp")) outFilePath += ".bmp";
                            break;
                        }
                    case "color":
                        {
                            var colorVals = value.Split(',').Select(n => Byte.Parse(n)).ToArray();
                            switch (colorVals.Count())
                            {
                                case 4:
                                    {
                                        Global.Opacity = colorVals[3];
                                        goto case 3;
                                    }
                                case 3:
                                    {
                                        Global.Red = colorVals[0];
                                        Global.Green = colorVals[1];
                                        Global.Blue = colorVals[2];
                                        break;
                                    }
                                default:
                                    {
                                        FPrint("Error: must provide R,G,B values. A value is optional");
                                        return;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            FPrint("Error: argument key '{0}' not recognized", key);
                            break;
                        }

                }
            }

            Bitmap bmp = new Bitmap((int)(Global.ImageSize), (int)(Global.ImageSize));
            Graphics graphics = Graphics.FromImage(bmp);

#if (DEBUG)
            graphics.FillRectangle(new SolidBrush(Color.Cyan), 0, 0, Global.ImageSize, Global.ImageSize);
#endif

            if (wordStr.Split(' ').Count() == 1)
            {
                var word = new Word(null, wordStr);
                word.Draw(graphics);
            }
            else
            {
                var paragraph = new Paragraph(null, wordStr);
                paragraph.Draw(graphics);
            }


            if (File.Exists(outFilePath))
            {
                File.Delete(outFilePath);
            }

            outFilePath = System.Text.RegularExpressions.Regex.Replace(outFilePath, "[" + new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "]", "");
            bmp.Save(outFilePath, System.Drawing.Imaging.ImageFormat.Png);
            graphics.Dispose();
            bmp.Dispose();

        }

        static void FPrint(string message, params object[] args)
        {
            Console.WriteLine(string.Format(message, args));
        }

        public static float RadiansToDegrees(double rad)
        {
            return RadiansToDegrees((float)rad);
        }

        public static float RadiansToDegrees(float rad)
        {
            return (float)(rad * 180 / Math.PI);
        }

        public static float Max(float[] items)
        {

            float max = items[0];
            for (int i = 1; i < items.Count(); i++)
                max = max > items[i] ? max : items[i];
            return max;
        }
    }
}
