// Uncomment these only if you want to export GetString() or ExecuteBang().
//#define DLLEXPORT_GETSTRING
//#define DLLEXPORT_EXECUTEBANG

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This is a blank canvas on which to build your plugin.

// Note: Measure.GetString, Plugin.GetString, Measure.ExecuteBang, and
// Plugin.ExecuteBang have been commented out. If you need GetString
// and/or ExecuteBang and you have read what they are used for from the
// SDK docs, uncomment the function(s). Otherwise leave them commented out
// (or get rid of them)!

namespace Gallifreyan
{
    internal class Measure
    {
        const float ScaleFactor = 8.0F;
        int number;
        string outFilePath;

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            try
            {
                number = api.ReadInt("Value", 0);
            }
            catch (Exception e) {}
            try
            {
                Arc.Thickness = Convert.ToSingle(api.ReadDouble("Width", Arc.Thickness));
            }
            catch (Exception e) {}
            try
            {
                Digit.Size = Convert.ToInt32(api.ReadInt("Size", Digit.Size) * ScaleFactor);
                }
            catch (Exception e) {}
            try
            {
                outFilePath = api.ReadString("Out", string.Format("{0}.png", number.ToString()));
                }
            catch (Exception e) {}
            try
            {
                var colorVals = api.ReadString("Color", string.Format("{0},{1},{2},{3}", Arc.Red, Arc.Green, Arc.Blue, Arc.Opacity)).Split(',').Select(n => Int32.Parse(n)).ToArray();
                switch (colorVals.Length)
                {
                    case 4:
                        {
                            Arc.Opacity = colorVals[3];
                            goto case 3;
                        }
                    case 3:
                        {
                            Arc.Red = colorVals[0];
                            Arc.Green = colorVals[1];
                            Arc.Blue = colorVals[2];
                            break;
                        }
                }
            }
            catch (Exception e) {}
            try
            {
                Digit.GlobalAngle = Convert.ToSingle(api.ReadDouble("Angle", Digit.GlobalAngle));
            }
            catch (Exception e) { }
        }

        internal double Update()
        {
            Bitmap png = new Bitmap((int)(Digit.Size*Digit.BackgroundMultiplier), (int)(Digit.Size*Digit.BackgroundMultiplier));
            Graphics graphics = Graphics.FromImage(png);

            string numberStr = number.ToString();
            Digit.NumberOfDigits = numberStr.Length;

            for (int i = 1; i <= Digit.NumberOfDigits;i++)
            {
                Digit d = new Digit(Int32.Parse(numberStr.Substring(numberStr.Length - i, 1)), i);
                d.Draw(graphics);
            }

            if (File.Exists(outFilePath))
            {
                File.Delete(outFilePath);
            }

            try
            {
                png.Save(outFilePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e) { }
            graphics.Dispose();
            png.Dispose();
            
            return 0.0;
        }
        
        public static float RadiansToDegrees(double rad)
        {
            return RadiansToDegrees((float)rad);
        }

        public static float RadiansToDegrees(float rad)
        {
            return (float)(rad * 180 / Math.PI);
        }
        
#if DLLEXPORT_GETSTRING
        internal string GetString()
        {
            return "";
        }
#endif
        
#if DLLEXPORT_EXECUTEBANG
        internal void ExecuteBang(string args)
        {
        }
#endif
    }

    public static class Plugin
    {
        internal static Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();
#if DLLEXPORT_GETSTRING
        static IntPtr StringBuffer = IntPtr.Zero;
#endif

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();
            
#if DLLEXPORT_GETSTRING
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
#endif
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }
        
#if DLLEXPORT_GETSTRING
        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null)
            {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }
#endif

#if DLLEXPORT_EXECUTEBANG
        [DllExport]
        public static void ExecuteBang(IntPtr data, IntPtr args)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(Marshal.PtrToStringUni(args));
        }
#endif
    }
}
