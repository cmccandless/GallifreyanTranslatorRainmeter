using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gallifreyan
{
    /// <summary>
    /// Contains global settings and static methods
    /// </summary>
    public class Global
    {
        /// <summary>
        /// Diameter of largest circle in pixels
        /// </summary>
        public static int Size = 512;

        /// <summary>
        /// Angle of rotation of entire image in radians
        /// </summary>
        private static float globalAngle = 0;
        public static float Angle
        {
            get
            {
                return globalAngle + (float)(Math.PI / 2);
            }
            set
            {
                globalAngle = value;
            }
        }

        /// <summary>
        /// Width of line in pixels
        /// </summary>
        public static float Thickness = 2.0F;

        public static byte Opacity = 255;
        public static byte Red = 0;
        public static byte Green = 0;
        public static byte Blue = 0;

        /// <summary>
        /// Color of Image
        /// </summary>
        public static Color Color
        {
            get
            {
                return Color.FromArgb(Opacity, Red, Green, Blue);
            }
            set
            {
                Opacity = value.A;
                Red = value.R;
                Green = value.G;
                Blue = value.B;
            }
        }

        /// <summary>
        /// X component of center of image
        /// </summary>
        public static float X
        {
            get { return Global.ImageSize / 2.0F; }
        }

        /// <summary>
        /// Y component of center of image
        /// </summary>
        public static float Y
        {
            get { return X; }
        }

        /// <summary>
        /// Root scale factor for circles
        /// </summary>
        public const float ScaleFactor = 0.4F;

        /// <summary>
        /// Root scale factor for background
        /// </summary>
        private const float bkgdMult = 4.2F;

        /// <summary>
        /// Scale factor for background
        /// </summary>
        public static float BackgroundMultiplier
        {
            get
            {
                return bkgdMult * ScaleFactor;
            }
        }

        /// <summary>
        /// Size of image in pixels
        /// </summary>
        public static float ImageSize { get { return Size * BackgroundMultiplier; } }

        /// <summary>
        /// Gets highest value in set
        /// </summary>
        /// <param name="items">set of values to compare</param>
        /// <returns>highest value in set</returns>
        public static float Max(float[] items)
        {

            float max = items[0];
            for (int i = 1; i < items.Count(); i++)
                max = max > items[i] ? max : items[i];
            return max;
        }

        /// <summary>
        /// Gets lowest value in set
        /// </summary>
        /// <param name="items">set of values to compare</param>
        /// <returns>lowest value in set</returns>
        public static float Min(float[] items)
        {

            float min = items[0];
            for (int i = 1; i < items.Count(); i++)
                min = min < items[i] ? min : items[i];
            return min;
        }
    }

    /// <summary>
    /// Root class for Gallifreyan elements
    /// </summary>
    public abstract class Element
    {
        /// <summary>
        /// Owner object
        /// </summary>
        public Element Owner;
        /// <summary>
        /// Value represented by element
        /// </summary>
        public string Value;
        /// <summary>
        /// Position of element among owner's items
        /// </summary>
        public float Position;
        /// <summary>
        /// Set of elements encapsulated in element
        /// </summary>
        public List<Element> Items = new List<Element>();
        /// <summary>
        /// Angle between items in radians
        /// </summary>
        public float AngleIncrement
        {
            get
            {
                if (this is Paragraph)
                    return (float)(2 * Math.PI / (this as Paragraph).Words.Count);
                else if (this is Word)
                    return (float)(2 * Math.PI / (this as Word).Characters.Count);
                else //if (this is Character)
                    return (float)(2 * Math.PI / Items.Count);
            }
        }
        /// <summary>
        /// Angle of element relative to owner in radians
        /// </summary>
        public float AngleN
        {
            get
            {
                if (this is Character)
                {
                    if (Owner.Owner != null)
                        return (float)((Position * Owner.AngleIncrement + Owner.AngleN) % (2 * Math.PI));
                    else
                        return (float)((Position * Owner.AngleIncrement - Owner.AngleN) % (2 * Math.PI));
                }
                else //if (this is Paragraph|| this is Word)
                {
                    var angleInc = (Owner == null ? 2 * Math.PI : Owner.AngleIncrement);
                    var globalAdjust = (Owner == null ? Global.Angle : Owner.AngleN);
                    return (float)((Position * angleInc - globalAdjust) % (2 * Math.PI));
                }
            }
        }
        /// <summary>
        /// X component of center of element
        /// </summary>
        public float XN
        {
            get { return (float)((Owner == null ? Global.X : Owner.RadiusConstants.Outer * Math.Cos(AngleN) + Owner.XN)); }
        }
        /// <summary>
        /// Y component of center of element
        /// </summary>
        public float YN
        {
            get { return (float)((Owner == null ? Global.Y : Owner.RadiusConstants.Outer * Math.Sin(AngleN) + Owner.YN)); }
        }
        /// <summary>
        /// Diameter of largest circle of element in pixels
        /// </summary>
        public float Size { get { return Owner == null ? Global.Size : Owner.Size * Owner.ScaleFactor; } }
        /// <summary>
        /// Scale factor used by element's items
        /// </summary>
        public float ScaleFactor
        {
            get
            {
                return Global.ScaleFactor * 1.1F / (float)Math.Log(Items.Count + 1, Math.E);
            }
        }
        /// <summary>
        /// Constants used by element's items (if element is a Character, these are used by its Arcs)
        /// </summary>
        public Radii RadiusConstants;
        /// <summary>
        /// Defines radius constants
        /// </summary>
        public class Radii
        {
            private Element Owner;
            public float Outer { get { return Owner is Paragraph ? Owner.Size * 2.8F * Owner.ScaleFactor : Owner.Size / 2F; } }
            public float Inner1 { get { return Owner.ScaleFactor * Outer; } }
            public float Inner2 { get { return Inner1 * 0.7176F; } }
            public float Inner3 { get { return Inner2 * 0.6066F; } }

            public Radii(Element owner)
            { this.Owner = owner; }
        }

        public Element()
        {
            this.RadiusConstants = new Radii(this);
        }
        /// <summary>
        /// Draws element and its items
        /// </summary>
        /// <param name="graphics">Graphics object on which to draw element</param>
        public abstract void Draw(Graphics graphics);
    }

    public class Paragraph : Element
    {
        /// <summary>
        /// List of Words in Paragraph
        /// </summary>
        public List<Word> Words { get { return Items.Where(i => i is Word).Cast<Word>().ToList(); } }
        public List<Character> Punctuation { get { return Items.Where(i => i is Character && (i as Character).IsPunctuation).Cast<Character>().ToList(); } }

        public Paragraph(Element owner, string value, float position = 1)
            : base()
        {
            Owner = owner;
            Value = value;
            Position = position;
            int count = 1;
            foreach (var word in value.Split(' ').Reverse())
            {
                if (Regex.IsMatch(word, "[?.!]$"))
                {
                    Items.Add(new Word(this, word.Substring(0, word.Length - 1), count));
                    Items.Add(new Character(this, word.Substring(word.Length - 1), count - 0.5F));
                }
                else
                {
                    Items.Add(new Word(this, word, count));
                }
                count++;
            }
        }

        public override void Draw(Graphics graphics)
        {
            graphics.DrawEllipse(new Pen(Global.Color, Global.Thickness), Global.ImageSize * 0.1F, Global.ImageSize * 0.1F, Global.ImageSize * 0.8F, Global.ImageSize * 0.8F);
            foreach (var item in Items)
                item.Draw(graphics);
        }
    }

    public class Word : Element
    {

        public List<Character> Characters { get { return Items.Where(i => i is Character && !(i as Character).IsPunctuation).Cast<Character>().ToList(); } }
        public List<Character> Punctuation { get { return Items.Where(i => i is Character && (i as Character).IsPunctuation).Cast<Character>().ToList(); } }


        public Word(Paragraph owner, string value, float position = 0)
            : base()
        {
            Owner = owner;
            Value = value;
            Position = position;
            var chars = new List<string>();
            for (int i = 0; i < value.Length; i++)
            {
                var prevCh = i > 0 ? value.Substring(i - 1, 1).ToLower() : null;
                var ch = value.Substring(i, 1).ToLower();
                var nextCh = value.Length - 1 > i ? value.Substring(i + 1, 1).ToLower() : null;
                switch (ch)
                {
                    case "c":
                        {
                            switch (nextCh)
                            {
                                case "h":
                                    {
                                        i++;
                                        ch = "ch";
                                        break;
                                    }
                                case "k":
                                    {
                                        continue;
                                    }
                                case "e":
                                case "i": // "soft" c
                                    {
                                        if (prevCh == "s") continue;
                                        ch = "s";
                                        break;
                                    }
                                default: // "hard" c
                                    {
                                        ch = "k";
                                        break;
                                    }
                            }
                            break;
                        }
                    case "q":
                        {
                            if (nextCh == "u") i++;
                            ch = "qu";
                            break;
                        }
                    case "n":
                        {
                            if (nextCh == "g")
                            {
                                i++;
                                ch = "ng";
                            }
                            break;
                        }
                    case "t":
                        {
                            if (nextCh == "h")
                            {
                                i++;
                                ch = "th";
                            }
                            break;
                        }
                    case "s":
                        {
                            if (nextCh == "h")
                            {
                                i++;
                                ch = "sh";
                            }
                            break;
                        }
                }
                chars.Add(ch);
            }

            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[chars.Count - i - 1];
                if (Regex.IsMatch(ch, @"^[.?!'\-,]$" + "|^\"$"))
                {
                    Items.Add(new Character(this, ch, 0.5F + i));
                    chars.RemoveAt(chars.Count - i - 1);
                }
            }

            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[chars.Count - i - 1];
                Items.Add(new Character(this, ch, i + 1));
            }
        }

        public override void Draw(Graphics graphics)
        {
            foreach (var item in Items)
                item.Draw(graphics);
        }
    }

    public class Character : Element
    {
        public Word Word { get { return Owner as Word; } }
        public bool IsPunctuation
        {
            get
            {
                return Regex.IsMatch(Value, @"^[.?!'\-,]$" + "|^\"$");
            }
        }

        public Character Previous { get { return Word.Characters[Math.Floor(Position) > 1 ? (int)Math.Floor(Position) - 2 : Word.Characters.Count - 1]; } }
        public Character Next { get { return Word.Characters[(int)Math.Floor(Position) < Word.Characters.Count ? (int)Math.Floor(Position) : 0]; } }

        public Arc Outer;
        public Arc Inner1;
        public Arc Inner2;
        public Arc Inner3;

        public Character(Element owner, string value, float position = 1)
            : base()
        {
            Owner = owner;
            Value = value;
            Position = position;
            Outer = new Arc(Arc.ArcType.Outer, this);
            Inner1 = new Arc(Arc.ArcType.Inner1, this);
            Inner2 = new Arc(Arc.ArcType.Inner2, this);
            Inner3 = new Arc(Arc.ArcType.Inner3, this);
        }

        public override void Draw(Graphics graphics)
        {
            Outer.Draw(graphics);
            Inner1.Draw(graphics);
            Inner2.Draw(graphics);
            Inner3.Draw(graphics);
        }
    }

    public class Arc
    {
        public enum ArcType { Outer, Inner1, Inner2, Inner3 }

        private ArcType Type;

        private Word.Radii RadiusConstants { get { return Owner.Owner.RadiusConstants; } }

        public float Radius
        {
            get
            {
                switch (Type)
                {
                    default:
                    case ArcType.Outer:
                        return RadiusConstants.Outer;
                    case ArcType.Inner1:
                        if (Owner.IsPunctuation || Regex.IsMatch(Owner.Value, "^[aeiou]$")) return RadiusConstants.Inner1 * 0.25F;
                        else return RadiusConstants.Inner1;
                    case ArcType.Inner2:
                        return RadiusConstants.Inner2;
                    case ArcType.Inner3:
                        return RadiusConstants.Inner3;
                }
            }
        }

        public float Angle
        {
            get
            {
                float result;
                switch (Type)
                {
                    default:
                    case ArcType.Outer:
                        result = (float)Math.Atan(X * 2 / A); break;
                    case ArcType.Inner1:
                    case ArcType.Inner2:
                    case ArcType.Inner3:
                        result = (float)Math.Atan(-(D - X) * 2 / A); break;
                }
                return result;
            }
        }

        private float D
        {
            get
            {
                if (Regex.IsMatch(Owner.Value.ToLower(), "^[bdfgh]$|ch"))
                    return RadiusConstants.Outer - RadiusConstants.Inner1 * 0.9F;
                else if (Regex.IsMatch(Owner.Value.ToLower(), "^[jklmnp]$"))
                    return RadiusConstants.Outer - RadiusConstants.Inner1 * 1.2F;
                else if (Owner.Value.ToLower() == "a")
                    return RadiusConstants.Outer * 1.2F;
                else if (Owner.Value.ToLower() == "o")
                    return RadiusConstants.Outer * 0.8F;
                else //0,1,2,3,4,5,6,7,8,9,t,sh,s,r,v,w,th,,y,z,ng,qu,x,e,i,u,.,?,!,",',-,,
                    return RadiusConstants.Outer;
            }
        }
        private float X
        {
            get
            {
                var localD = Regex.IsMatch(Owner.Value, "^[aojklmnp]$") ? RadiusConstants.Outer : D;
                var localRadius = Type == ArcType.Outer || Regex.IsMatch(Owner.Value, "^[aojklmnp]$") ? RadiusConstants.Inner1 : Radius;
                return ((localD * localD - localRadius * localRadius + RadiusConstants.Outer * RadiusConstants.Outer) / (localD * 2));
            }
        }

        private float A
        {
            get
            {
                var localD = Regex.IsMatch(Owner.Value, "^[aojklmnp]$") ? RadiusConstants.Outer : D;
                var localRadius = Type == ArcType.Outer || Regex.IsMatch(Owner.Value, "^[aojklmnp]$") ? RadiusConstants.Inner1 : Radius;
                return (float)(1 / localD * Math.Sqrt((localRadius - localD - RadiusConstants.Outer) *
                                    (RadiusConstants.Outer - localRadius - localD) * (RadiusConstants.Outer + localRadius - localD) *
                                    (localD + localRadius + RadiusConstants.Outer)));
            }
        }

        public float StartAngle
        {
            get
            {
                var value = Owner.Value;
                double result;
                switch (Type)
                {
                    case ArcType.Outer:
                        {
                            if (Regex.IsMatch(value, "^[234bdfghtrsvw]$|ch|sh"))
                                result = Angle - Math.PI / 2;
                            else
                                result = Math.PI / 2 - Angle;
                            break;
                        }
                    case ArcType.Inner1:
                        {
                            switch (value)
                            {
                                case "0": result = Math.PI / 3 - (float)Math.PI / 2.0F; break;
                                case "1": result = 5 * Math.PI / 3 - (float)Math.PI / 2.0F; break;
                                //2,3,4,5,6,7,8,9,b,ch,d,f,g,h,j,k,l,m,n,p,t,sh,r,s,v,w,th,y,z,ng,qu,x,a,e,i,o,u,<punctuation>
                                default: result = Angle - (float)Math.PI / 2.0F; break;
                            }
                            break;
                        }
                    default:
                        {
                            result = Angle - (float)Math.PI / 2.0F; break;
                        }
                }
                return (float)((result + Owner.AngleN/*Owner.Position * Owner.Owner.AngleIncrement - Owner.Owner.AngleN*/) % (2 * Math.PI));
            }
        }

        public float RotationAngle
        {
            get
            {
                var value = Owner.Value;
                double result;
                switch (Type)
                {
                    case ArcType.Outer:
                        {
                            if (Regex.IsMatch(value, "^[234bdfghtrsvw]$|ch|sh"))
                                result = -2 * Angle - Math.PI + (Owner.Word.Characters.Count - 1) * Owner.Owner.AngleIncrement;
                            else
                                result = -2 * Math.PI + (Owner.Word.Characters.Count - 1) * Owner.Owner.AngleIncrement;
                            result += Angle - Owner.Previous.Outer.Angle;
                            break;
                        }
                    default:
                        {
                            if (Regex.IsMatch(value, "^[01]$")) result = 0.001;
                            else if (Regex.IsMatch(value, "^[567bdfghtrsvw]$|ch|sh")) result = -1 * (Math.PI + 2 * Angle);
                            else if (Regex.IsMatch(value, "^[89]$")) result = -2 * Angle + Math.PI;
                            else result = 2 * Math.PI;
                            break;
                        }
                }
                return (float)result;
            }
        }

        public Character Owner;

        public Arc(ArcType type, Character owner)
        {
            Type = type;
            Owner = owner;
        }

        public void Draw(Graphics graphics)
        {
            var value = Owner.Value;
            var pen = new Pen(Color.FromArgb(Global.Opacity, Global.Red, Global.Green, Global.Blue), Global.Thickness);
            var brush = new SolidBrush(Color.FromArgb(Global.Opacity, Global.Red, Global.Green, Global.Blue));
            var originX = Owner.XN + (D - RadiusConstants.Outer) * (float)Math.Cos(Owner.AngleN);
            var originY = Owner.YN + (D - RadiusConstants.Outer) * (float)Math.Sin(Owner.AngleN);
            var trueX = (float)(originX - Radius);
            var trueY = (float)(originY - Radius);
            if (Type == ArcType.Inner1)
            {
                if (Regex.IsMatch(value, "^[01]$"))
                {
                    float x2 = Radius * (float)Math.Cos(StartAngle) + Owner.XN;
                    float y2 = Radius * (float)Math.Sin(StartAngle) + Owner.YN;
                    graphics.DrawLine(pen, Owner.XN, Owner.YN, x2, y2);
                }
                #region Dots
                else if (Regex.IsMatch(value, "^[kydlrz]|ch|sh$"))
                {
                    graphics.FillPie(brush,
                        0.75F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI - Math.PI / 6) + originX - Radius * 0.15F,
                        0.75F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI - Math.PI / 6) + originY - Radius * 0.15F,
                        Radius * 0.25F, Radius * 0.25F, 0, 360);
                    graphics.FillPie(brush,
                        0.75F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI + Math.PI / 6) + originX - Radius * 0.15F,
                        0.75F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI + Math.PI / 6) + originY - Radius * 0.15F,
                        Radius * 0.25F, Radius * 0.25F, 0, 360);
                    if (Regex.IsMatch(value, "^[dlrz]$")) // Three Dots
                    {
                        graphics.FillPie(brush,
                            0.75F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI) + originX - Radius * 0.15F,
                            0.75F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI) + originY - Radius * 0.15F,
                            Radius * 0.25F, Radius * 0.25F, 0, 360);
                    }
                }
                #endregion
                #region Lines
                else if (Regex.IsMatch(value, "^[fmsgnvhpwx]$|ng|qu"))
                {
                    if (Regex.IsMatch(value, "^[fmsgnv]$|ng|qu")) // One Line
                    {
                        graphics.DrawLine(pen,
                            1.2F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI) + originX,
                            1.2F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI) + originY,
                            0.8F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI) + originX,
                            0.8F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI) + originY);
                    }
                    if (Regex.IsMatch(value, "^[fmshpwx]$|ng")) // Two Lines
                    {
                        graphics.DrawLine(pen,
                            1.2F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI - Math.PI / 8) + originX,
                            1.2F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI - Math.PI / 8) + originY,
                            0.8F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI - Math.PI / 8) + originX,
                            0.8F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI - Math.PI / 8) + originY);
                        graphics.DrawLine(pen,
                            1.2F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI + Math.PI / 8) + originX,
                            1.2F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI + Math.PI / 8) + originY,
                            0.8F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI + Math.PI / 8) + originX,
                            0.8F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI + Math.PI / 8) + originY);
                    }
                }
                #endregion
                #region Special Cases
                else
                {
                    switch (value)
                    {
                        case "i":
                            {
                                graphics.DrawLine(pen,
                                    2.0F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI) + Owner.XN,
                                    2.0F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI) + Owner.YN,
                                    Radius * (float)Math.Cos(Owner.AngleN + Math.PI) + Owner.XN,
                                    Radius * (float)Math.Sin(Owner.AngleN + Math.PI) + Owner.YN);
                                break;
                            }
                        case "u":
                            {
                                graphics.DrawLine(pen,
                                    Owner.XN - 2.0F * Radius * (float)Math.Cos(Owner.AngleN + Math.PI),
                                    Owner.YN - 2.0F * Radius * (float)Math.Sin(Owner.AngleN + Math.PI),
                                    Owner.XN - Radius * (float)Math.Cos(Owner.AngleN + Math.PI),
                                    Owner.YN - Radius * (float)Math.Sin(Owner.AngleN + Math.PI));
                                break;
                            }
                        case "?":
                            {
                                var rad = Radius * 2F / 3F;
                                graphics.FillPie(brush,
                                    Owner.Word.XN + (D - Radius) * (float)Math.Cos(Owner.AngleN * 1.05) - rad,
                                    Owner.Word.YN + (D - Radius) * (float)Math.Sin(Owner.AngleN * 1.05) - rad,
                                    Radius * 2F / 3F, Radius * 2F / 3F, 0, 360);
                                graphics.FillPie(brush,
                                    Owner.Word.XN + (D - Radius) * (float)Math.Cos(Owner.AngleN * 0.95) - rad,
                                    Owner.Word.YN + (D - Radius) * (float)Math.Sin(Owner.AngleN * 0.95) - rad,
                                    Radius * 2F / 3F, Radius * 2F / 3F, 0, 360);
                                break;
                            }
                        case "!":
                            {
                                var rad = Radius * 2F / 3F;
                                graphics.FillPie(brush,
                                    Owner.Word.XN + (D - Radius) * (float)Math.Cos(Owner.AngleN) - rad,
                                    Owner.Word.YN + (D - Radius) * (float)Math.Sin(Owner.AngleN) - rad,
                                    rad, rad, 0, 360);
                                graphics.FillPie(brush,
                                    Owner.Word.XN + (D - Radius) * (float)Math.Cos(Owner.AngleN * 0.98) - rad,
                                    Owner.Word.YN + (D - Radius) * (float)Math.Sin(Owner.AngleN * 0.98) - rad,
                                    Radius * 2F / 3F, Radius * 2F / 3F, 0, 360);
                                graphics.FillPie(brush,
                                    Owner.Word.XN + (D - Radius) * (float)Math.Cos(Owner.AngleN * 1.02) - rad,
                                    Owner.Word.YN + (D - Radius) * (float)Math.Sin(Owner.AngleN * 1.02) - rad,
                                    Radius * 2F / 3F, Radius * 2F / 3F, 0, 360);
                                break;
                            }
                        case "'":
                            {
                                var ang = Owner.AngleN * 0.99;
                                graphics.DrawLine(pen,
                                    Owner.Word.XN + D * (float)Math.Cos(ang),
                                    Owner.Word.YN + D * (float)Math.Sin(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Cos(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Sin(ang));
                                ang = Owner.AngleN * 1.01;
                                graphics.DrawLine(pen,
                                    Owner.Word.XN + D * (float)Math.Cos(ang),
                                    Owner.Word.YN + D * (float)Math.Sin(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Cos(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Sin(ang));
                                break;
                            }
                        case "-":
                            {
                                var ang = Owner.AngleN * 0.98;
                                graphics.DrawLine(pen,
                                    Owner.Word.XN + D * (float)Math.Cos(ang),
                                    Owner.Word.YN + D * (float)Math.Sin(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Cos(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Sin(ang));
                                ang = Owner.AngleN * 1.02;
                                graphics.DrawLine(pen,
                                    Owner.Word.XN + D * (float)Math.Cos(ang),
                                    Owner.Word.YN + D * (float)Math.Sin(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Cos(ang),
                                    Owner.Word.XN + (D + Radius) * (float)Math.Sin(ang));
                                goto case "\"";
                            }
                        case "\"":
                            {
                                graphics.DrawLine(pen,
                                    Owner.Word.XN + (D + Radius) * (float)Math.Cos(Owner.AngleN),
                                    Owner.Word.YN + (D + Radius) * (float)Math.Sin(Owner.AngleN),
                                    Owner.XN, Owner.YN);
                                break;
                            }
                        case ",":
                            graphics.FillPie(brush, trueX, trueY, Radius * 2, Radius * 2, 0, 360); break;
                    }
                }
                #endregion
            }
            // Main Arc
            if (!Owner.IsPunctuation || Owner.Value == ".")
            {
                if (Type == ArcType.Inner1 || (Type == ArcType.Inner2 && Regex.IsMatch(value, "^[34679]$")) ||
                (Type == ArcType.Inner3 && (value == "4" || value == "7")))
                    graphics.DrawArc(pen, trueX, trueY, Radius * 2, Radius * 2, RadiansToDegrees(StartAngle), RadiansToDegrees(RotationAngle));
                else if (Type == ArcType.Outer && !Owner.IsPunctuation)
                    graphics.DrawArc(pen, Owner.Word.XN - Radius, Owner.Word.YN - Radius, Radius * 2, Radius * 2, RadiansToDegrees(StartAngle), RadiansToDegrees(RotationAngle));
            }
        }

        public static float RadiansToDegrees(double rad)
        {
            return RadiansToDegrees((float)rad);
        }

        public static float RadiansToDegrees(float rad)
        {
            return (float)(rad * 180 / Math.PI);
        }
    }
}
