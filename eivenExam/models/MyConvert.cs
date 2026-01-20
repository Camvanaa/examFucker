using System;
using Eiven.EXE.Web.CE;

/// <summary>
/// Convert 的摘要说明。
/// </summary>
/// 
namespace Eiven.EXE.Web.Models
{
    public static class MyConvert
    {
        public static string ToString(object Value)
        {
            if (Value is System.Drawing.Color)
            {

                System.Drawing.Color C = (System.Drawing.Color)Value;
                if (C.IsKnownColor && !C.IsSystemColor) return C.Name;
                else
                {
                    string R = MyConvert.ToRadix16(C.R, 2);
                    string G = MyConvert.ToRadix16(C.G, 2);
                    string B = MyConvert.ToRadix16(C.B, 2);

                    return "#" + R + G + B;
                }
            }

            if (Value is System.Drawing.Font)
            {
                System.Drawing.Font f = (System.Drawing.Font)Value;
                return string.Format("{0};{1};{2};{3}", f.Name, f.Size, f.Unit.ToString(), f.Style.ToString());
            }

            try
            {
                return System.Convert.ToString(Value);
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        public static System.Drawing.Font ToFont(object val)
        {
            if (val is System.Drawing.Font) return (System.Drawing.Font)val;
            string s = MyConvert.ToString(val);
            string[] ss = s.Split(';');
            string name = ss.Length >= 1 ? ss[0] : "宋体";
            string size = ss.Length >= 2 ? ss[1] : "9";
            string unit = ss.Length >= 3 ? ss[2] : "Point";
            string style = ss.Length >= 4 ? ss[3] : "Regular";
            float sz = MyConvert.ToFloat(size, 9);
            if (sz <= 0) sz = 9;
            System.Drawing.GraphicsUnit ut = MyConvert.ToEnum<System.Drawing.GraphicsUnit>(unit, System.Drawing.GraphicsUnit.Point);
            System.Drawing.FontStyle fs = MyConvert.ToEnum<System.Drawing.FontStyle>(style, System.Drawing.FontStyle.Regular);

            return new System.Drawing.Font(
                name, sz, fs, ut);
        }

        public static bool ToBool(object Value)
        {
            try
            {
                if (Value == null) return false;

                if (Value is bool) return (bool)Value;

                string s = Value.ToString();

                if (s.ToLower() == "true") return true;

                bool res = false;
                if (bool.TryParse(s, out res)) return res;

                double val = 0;
                if (double.TryParse(s, out val))
                {
                    if (val != 0) return true;
                    else return false;
                }


                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static decimal ToDecimal(object Value, decimal DefaultValue)
        {
            try
            {
                if (Value == null) return DefaultValue;

                if (Value is decimal ||
                    Value is long || Value is ulong || Value is int || Value is uint ||
                    Value is short || Value is ushort || Value is byte || Value is sbyte)
                    return (decimal)Value;

                string s = Value.ToString();

                bool bol = false;
                decimal val = DefaultValue;

                if (s == "") return DefaultValue;
                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    decimal.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out val);
                    return val;
                }

                if (decimal.TryParse(s, out val)) return val;

                if (bool.TryParse(s, out bol))
                    if (bol) return 1; else return 0;

                return DefaultValue;
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static decimal ToDecimal(object Value)
        {
            return ToDecimal(Value, 0);
        }


        public static long ToLong(object Value, long DefaultValue)
        {
            try
            {
                if (Value == null) return DefaultValue;

                if (Value is long || Value is int || Value is byte || Value is short || Value is uint || Value is ushort || Value is sbyte)
                    return (long)Value;

                string s = Value.ToString();

                bool bol = false;
                long val = DefaultValue;

                if (s == "") return DefaultValue;
                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    long.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out val);
                    return val;
                }

                if (long.TryParse(s, out val)) return val;

                if (bool.TryParse(s, out bol))
                    if (bol) return 1; else return 0;

                return DefaultValue;
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static long ToLong(object Value)
        {
            return ToLong(Value, 0);
        }


        public static int ToInt(object Value, int DefaultValue)
        {
            try
            {
                if (Value == null) return DefaultValue;
                if (Value is int || Value is byte || Value is short || Value is ushort || Value is sbyte)
                    return (int)Value;

                string s = Value.ToString();

                bool bol = false;
                int val = DefaultValue;

                if (s == "") return DefaultValue;
                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                {
                    int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out val);
                    return val;
                }

                if (int.TryParse(s, out val)) return val;

                if (bool.TryParse(s, out bol))
                    if (bol) return 1; else return 0;

                return DefaultValue;
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static int ToInt(object Value)
        {
            return ToInt(Value, 0);
        }

        public static double ToDouble(object Value, double DefaultValue)
        {
            try
            {
                if (Value == null) return DefaultValue;

                if (Value is DateTime) return ((DateTime)Value).Ticks;

                string s = Value.ToString();

                if (s == "") return DefaultValue;

                if (double.TryParse(s, out DefaultValue))
                    return DefaultValue;

                if (string.Compare(s, "True", true) == 0) return 1;
                if (string.Compare(s, "False", true) == 0) return 0;

                return DefaultValue;
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static double ToDouble(object Value)
        {
            if (Value is double) return (double)Value;
            if (Value is float) return (double)((float)Value);
            if (Value is int) return (double)((int)Value);

            return ToDouble(Value, 0);
        }

        public static float ToFloat(object Value)
        {
            try
            {
                float DefaultValue = 0;
                if (Value == null) return DefaultValue;

                string s = Value.ToString();

                if (s == "") return DefaultValue;

                float.TryParse(s, out DefaultValue);

                return DefaultValue;
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        public static float ToFloat(object Value, float defaultValue)
        {
            if (Value is float) return (float)Value;
            if (Value is double) return (float)Value;
            if (Value is int) return (float)Value;
            if (Value is byte) return (float)Value;
            if (Value is DateTime) return ((DateTime)Value).Ticks;

            if (Value == null) return defaultValue;
            if (Value is DBNull) return defaultValue;

            try
            {
                double f = System.Convert.ToDouble(Value);
                return (float)f;
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        public static Guid ToGuid(object V)
        {
            if (V == null)
                return new Guid();

            if (V is string)
            {
                try
                {
                    return new Guid((string)V);
                }
                catch (System.Exception)
                {
                    return new Guid();
                }
            }

            if (V is byte[])
            {
                byte[] bs = (byte[])V;
                if (bs.Length == 16)
                    return new Guid(bs);
            }

            if (V is Guid)
                return (Guid)V;

            return new Guid();
        }

        //public static Eiven.Drawing.Unit ToUnit(object Value)
        //{
        //    if (Value == null)
        //        return new Eiven.Drawing.Unit();

        //    if (Value is Eiven.Drawing.Unit)
        //        return (Eiven.Drawing.Unit)Value;

        //    if (Value is string)
        //        return new Eiven.Drawing.Unit((string)Value);

        //    if (Value is float || Value is double)
        //        return new Eiven.Drawing.Unit((double)Value,
        //            Eiven.Drawing.UnitType.Pixel);

        //    return new Eiven.Drawing.Unit();
        //}

        //public static Eiven.DataObjects.Xml.XmlBytes ToXmlBytes(object V)
        //{
        //    if (V == null)
        //        return new Eiven.DataObjects.Xml.XmlBytes();

        //    if (V is Eiven.DataObjects.Xml.XmlBytes)
        //        return (Eiven.DataObjects.Xml.XmlBytes)V;

        //    if (V is byte[])
        //        return new Eiven.DataObjects.Xml.XmlBytes((byte[])V);

        //    if (V is string)
        //        return new Eiven.DataObjects.Xml.XmlBytes(ToBytes(V));

        //    return new Eiven.DataObjects.Xml.XmlBytes();
        //}

        public static byte[] ToBytes(object V)
        {
            if (V == null)
                return new byte[0];

            if (V is byte[])
                return (byte[])V;

            if (V is string)
            {
                string s = (string)V;
                byte[] bs = new byte[s.Length / 2];

                for (int i = 0; i < bs.Length; i++)
                {
                    byte b = 0;
                    int i1 = 0;
                    int i2 = 0;
                    char c1 = s[i << 1];
                    char c2 = s[(i << 1) + 1];

                    if (c1 >= '0' && c1 <= '9')
                        i1 = ((int)c1) - ((int)'0');
                    if (c1 >= 'A' && c1 <= 'F')
                        i1 = ((int)c1) - ((int)'A') + 10;
                    if (c1 >= 'a' && c1 <= 'f')
                        i1 = ((int)c1) - ((int)'a') + 10;

                    if (c2 >= '0' && c2 <= '9')
                        i2 = ((int)c2) - ((int)'0');
                    if (c2 >= 'A' && c2 <= 'F')
                        i2 = ((int)c2) - ((int)'A') + 10;
                    if (c2 >= 'a' && c2 <= 'f')
                        i2 = ((int)c2) - ((int)'a') + 10;

                    b = (byte)(i1 * 16 + i2);
                    bs[i] = b;
                }
                return bs;
            }
            return new byte[0];
        }

        public static DateTime ToDateTime(object Value, DateTime DefaultValue)
        {
            try
            {
                if (Value is double || Value is int || Value is long || Value is float)
                {
                    long val = (System.Convert.ToInt64(Value));
                    return new DateTime(val);
                }
                //if (Value is Eiven.DataObjects.DBString) Value = Convert.ToString(Value);
                return System.Convert.ToDateTime(Value);
            }
            catch (System.Exception)
            {
                string[] K = Value.ToString().Trim().Replace("  ", " ").Split(' ');

                if (K.Length >= 3)
                {
                    try
                    {
                        int Y = int.Parse(K[2]);
                        int M = int.Parse(K[0]);
                        int D = int.Parse(K[1]);

                        return new DateTime(Y, M, D);
                    }
                    catch (System.Exception)
                    {
                        return DefaultValue;
                    }
                }

                return DefaultValue;
            }
        }

        public static TimeSpan ToTimeSpan(object Value)
        {
            if (Value is TimeSpan) return (TimeSpan)Value;
            string v = Value.ToString();
            TimeSpan t;
            if (TimeSpan.TryParse(v, out t)) return t;

            return new TimeSpan(0, 0, 0, 0);
        }
        public static DateTime ToDateTime(object Value)
        {
            return ToDateTime(Value, new DateTime(1800, 1, 1, 0, 0, 0, 0));
        }

        public static System.Drawing.Color ToColor(object Value, System.Drawing.Color DefaultValue)
        {
            if (Value == null)
                return DefaultValue;

            if (Value is System.Drawing.Color)
            {
                return (System.Drawing.Color)Value;
            }

            try
            {
                string C = Value.ToString().Trim();


                if (C.StartsWith("#"))
                {
                    int R = MyConvert.ToInt("0x" + C.Substring(1, 2));
                    int G = MyConvert.ToInt("0x" + C.Substring(3, 2));
                    int B = MyConvert.ToInt("0x" + C.Substring(5, 2));

                    return System.Drawing.Color.FromArgb(R, G, B);
                }
                else
                    return System.Drawing.Color.FromName(C);
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static System.Drawing.Color ToColor(object Value)
        {
            return ToColor(Value, System.Drawing.Color.White);
        }

        public static object ToEnum(object Value, System.Type EnumType, System.Enum DefaultValue)
        {
            try
            {
                if (Value == null) return DefaultValue;
                if (Value.ToString() == "") return DefaultValue;
                return System.Enum.Parse(EnumType, MyConvert.ToString(Value), false);
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static EnumType ToEnum<EnumType>(object Value, EnumType DefaultValue, bool ignoreCase = true)
        {
            try
            {
                if (Value == null) return DefaultValue;
                if (Value.ToString() == "") return DefaultValue;
                return (EnumType)System.Enum.Parse(typeof(EnumType), MyConvert.ToString(Value), ignoreCase);
            }
            catch (System.Exception)
            {
                return DefaultValue;
            }
        }

        public static EnumType ToEnum<EnumType>(object Value, bool ignoreCase = true)
        {
            try
            {
                return (EnumType)System.Enum.Parse(typeof(EnumType), MyConvert.ToString(Value), ignoreCase);
            }
            catch (System.Exception)
            {
                object obj = 0;
                return (EnumType)obj;
            }
        }

        private static string ToAnyRadix(object argToConvert, string formatStr)
        {
            AnyRadix provider = new AnyRadix();
            string messageStr =
                String.Format("{{0:{0}}}", formatStr);

            // Write the first part of the output line.
            //Console.Write( "{0,18}  {1,-6}", argToConvert, formatStr );

            // Convert the specified argument using the specified format.
            try
            {
                return String.Format(provider, messageStr, new object[] { argToConvert });
            }
            catch (Exception ex)
            {
                // Display the exception without the stack trace.
                return ex.Message;
            }
        }

        private static string ToAnyRadix(object argToConvert, string formatStr, int Length)
        {
            string K = ToAnyRadix(argToConvert, formatStr);

            while (K.Length < Length)
                K = "0" + K;

            return K;
        }

        public static string ToAnyRadix(object Value, int Radix)
        {
            return ToAnyRadix(Value, "Ra" + Radix.ToString());
        }

        public static string ToAnyRadix(object Value, int Radix, int Length)
        {
            return ToAnyRadix(Value, "Ra" + Radix.ToString(), Length);
        }

        public static string ToRadix2(object Value)
        {
            return ToAnyRadix(Value, 2);
        }

        public static string ToRadix2(object Value, int Length)
        {
            return ToAnyRadix(Value, 2, Length);
        }

        public static string ToRadix8(object Value)
        {
            return ToAnyRadix(Value, 8);
        }

        public static string ToRadix8(object Value, int Length)
        {
            return ToAnyRadix(Value, 8, Length);
        }

        public static string ToRadix16(object Value)
        {
            return ToAnyRadix(Value, 16);
        }

        public static string ToRadix16(object Value, int Length)
        {
            return ToAnyRadix(Value, 16, Length);
        }

        public static void Swap(ref object A, ref object B)
        {
            object T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref string A, ref string B)
        {
            string T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref System.Drawing.Color A, ref System.Drawing.Color B)
        {
            System.Drawing.Color T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref int A, ref int B)
        {
            int T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref double A, ref double B)
        {
            double T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref long A, ref long B)
        {
            long T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref float A, ref float B)
        {
            float T = A;
            A = B;
            B = T;
        }

        public static void Swap(ref DateTime A, ref DateTime B)
        {
            DateTime T = A;
            A = B;
            B = T;
        }

        public static System.Drawing.Color MixColor
            (System.Drawing.Color C1, System.Drawing.Color C2, int c1, int c2)
        {
            int R = (C1.R * c1 + C2.R * c2) / (c1 + c2);
            int G = (C1.G * c1 + C2.G * c2) / (c1 + c2);
            int B = (C1.B * c1 + C2.B * c2) / (c1 + c2);

            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public static System.Drawing.Color MixColor
            (System.Drawing.Color C1, System.Drawing.Color C2, double c1)
        {
            double c2 = 1 - c1;

            int R = (int)((C1.R * c1 + C2.R * c2));
            int G = (int)((C1.G * c1 + C2.G * c2));
            int B = (int)((C1.B * c1 + C2.B * c2));

            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public static System.Drawing.RectangleF Union(params System.Drawing.RectangleF[] rects)
        {
            if (rects.Length == 0) return new System.Drawing.RectangleF(0, 0, 0, 0);
            float l = rects[0].Left;
            float t = rects[0].Top;
            float r = rects[0].Left;
            float b = rects[0].Top;

            foreach (System.Drawing.RectangleF rect in rects)
            {
                if (rect.Left < l) l = rect.Left;
                if (rect.Right < l) l = rect.Right;

                if (rect.Left > r) r = rect.Right;
                if (rect.Right > r) r = rect.Right;

                if (rect.Top < t) t = rect.Top;
                if (rect.Bottom < t) t = rect.Bottom;

                if (rect.Top > b) b = rect.Bottom;
                if (rect.Bottom > b) b = rect.Bottom;
            }

            return new System.Drawing.RectangleF(l, t, r - l, b - t);
        }

        public static System.Drawing.Rectangle Union(params System.Drawing.Rectangle[] rects)
        {
            int l = rects[0].Left;
            int t = rects[0].Top;
            int r = rects[0].Left;
            int b = rects[0].Top;

            foreach (System.Drawing.Rectangle rect in rects)
            {
                if (rect.Left < l) l = rect.Left;
                if (rect.Right < l) l = rect.Right;

                if (rect.Left > r) r = rect.Right;
                if (rect.Right > r) r = rect.Right;

                if (rect.Top < t) t = rect.Top;
                if (rect.Bottom < t) t = rect.Bottom;

                if (rect.Top > b) b = rect.Bottom;
                if (rect.Bottom > b) b = rect.Bottom;
            }

            return new System.Drawing.Rectangle(l, t, r - l, b - t);
        }

        public static System.Drawing.SizeF FitSize(System.Drawing.SizeF original, System.Drawing.SizeF target)
        {
            if (original.Width == 0 && original.Height == 0) return target;
            if (original.Width == 0) return new System.Drawing.SizeF(original.Width, target.Height);
            if (original.Height == 0) return new System.Drawing.SizeF(target.Width, original.Height);

            double d1 = original.Width * 1.0 / original.Height;
            double d2 = target.Width * 1.0 / target.Height;

            if (d1 > d2)
            {
                float h = (float)(target.Width / d1);
                return new System.Drawing.SizeF(target.Width, h);
            }
            else
            {
                float w = (float)(target.Height * d1);
                return new System.Drawing.SizeF(w, target.Height);
            }
        }

        public static System.Drawing.Size FitSize(System.Drawing.Size original, System.Drawing.Size target)
        {
            if (original.Width == 0 && original.Height == 0) return target;
            if (original.Width == 0) return new System.Drawing.Size(original.Width, target.Height);
            if (original.Height == 0) return new System.Drawing.Size(target.Width, original.Height);

            double d1 = original.Width * 1.0 / original.Height;
            double d2 = target.Width * 1.0 / target.Height;

            if (d1 > d2)
            {
                int h = (int)(target.Width / d1);
                return new System.Drawing.Size(target.Width, h);
            }
            else
            {
                int w = (int)(target.Height * d1);
                return new System.Drawing.Size(w, target.Height);
            }
        }

        public static System.Drawing.RectangleF FitRectangle(System.Drawing.RectangleF original,
            System.Drawing.RectangleF target)
        {
            System.Drawing.SizeF os = original.Size;
            System.Drawing.SizeF ts = MyConvert.FitSize(os, target.Size);

            System.Drawing.PointF tp = new System.Drawing.PointF(
                (target.Width - ts.Width) / 2 + target.Left,
                (target.Height - ts.Height) / 2 + target.Top);

            return new System.Drawing.RectangleF(tp, ts);
        }


        public static System.Drawing.Rectangle FitRectangle(System.Drawing.Rectangle original,
            System.Drawing.Rectangle target)
        {
            System.Drawing.Size os = original.Size;
            System.Drawing.Size ts = MyConvert.FitSize(os, target.Size);

            System.Drawing.Point tp = new System.Drawing.Point(
                (target.Width - ts.Width) / 2 + target.Left,
                (target.Height - ts.Height) / 2 + target.Top);

            return new System.Drawing.Rectangle(tp, ts);
        }
    }


    public class AnyRadix : ICustomFormatter, IFormatProvider
    {
        // The value to be formatted is returned as a signed string 
        // of digits from the rDigits array. 
        const string radixCode = "Ra";
        private static char[] rDigits = {
                                            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                                            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                                            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                                            'U', 'V', 'W', 'X', 'Y', 'Z' };

        // This method returns an object that implements ICustomFormatter 
        // to do the formatting. 
        public object GetFormat(Type argType)
        {
            // Here, the same object (this) is returned, but it would 
            // be possible to return an object of a different type.
            if (argType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        // This method does the formatting only if it recognizes the 
        // format codes. 
        public string Format(string formatString,
            object argToBeFormatted, IFormatProvider provider)
        {
            // If no format string is provided or the format string cannot 
            // be handled, use IFormattable or standard string processing.
            if (formatString == null ||
                !formatString.Trim().StartsWith(radixCode))
            {
                if (argToBeFormatted is IFormattable)
                    return ((IFormattable)argToBeFormatted).
                        ToString(formatString, provider);
                else
                    return argToBeFormatted.ToString();
            }

            // The formatting is handled here.
            int digitIndex = 0;
            long radix;
            long longToBeFormatted;
            long longPositive;
            char[] outDigits = new char[63];

            // Extract the radix from the format string.
            formatString = formatString.Replace(radixCode, "");
            try
            {
                radix = System.Convert.ToInt64(formatString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format(
                    "The radix \"{0}\" is invalid.",
                    formatString), ex);
            }

            // Verify that the radix is in the proper range.
            if (radix < 2 || radix > 36)
                throw new ArgumentException(String.Format(
                    "The radix \"{0}\" is not in the range 2..36.",
                    formatString));

            // Verify that the argument can be converted to a long integer.
            try
            {
                longToBeFormatted = System.Convert.ToInt64(argToBeFormatted);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format(
                    "The argument \"{0}\" cannot be " +
                    "converted to an integer value.",
                    argToBeFormatted), ex);
            }

            // Extract the magnitude for conversion.
            longPositive = Math.Abs(longToBeFormatted);

            // Convert the magnitude to a digit string.
            for (digitIndex = 0; digitIndex <= 64; digitIndex++)
            {
                if (longPositive == 0) break;

                outDigits[outDigits.Length - digitIndex - 1] =
                    rDigits[longPositive % radix];
                longPositive /= radix;
            }

            // Add a minus sign if the argument is negative.
            if (longToBeFormatted < 0)
                outDigits[outDigits.Length - digitIndex++ - 1] =
                    '-';

            return new string(outDigits,
                outDigits.Length - digitIndex, digitIndex);
        }
    }




}