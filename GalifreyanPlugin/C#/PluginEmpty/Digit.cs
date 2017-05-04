using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallifreyan
{
    public class Digit
    {

        public static int Size = 255;

        public static int NumberOfDigits = 0;

        public static float GlobalAngle = 0;

        public static float AngleIncrement { get { return (float)(2 * Math.PI / NumberOfDigits); } }

        public const float SmallCircleMultiplier = 0.4F;

        private const float bkgdMult = 3.7F;

        public static float BackgroundMultiplier
        {
            get
            {
                return bkgdMult * SmallCircleMultiplier;
            }
        }

        public int Value;
        public int Position;

        private float AngleN
        {
            get
            {
                return (float)((Math.PI / 2 + Position * AngleIncrement - GlobalAngle)%(2*Math.PI));
            }
        }

        public float XN
        {
            get { return (float)(Arc.RadiusConstants.Outer * Math.Cos(AngleN) + X); }
        }

        public float YN
        {
            get { return (float)(Arc.RadiusConstants.Outer * Math.Sin(AngleN) + Y); }
        }

        public float X
        {
            get { return (Size * BackgroundMultiplier)/2;  }
        }

        public float Y
        {
            get { return X; }
        }

        public Arc Outer;
        public Arc Inner1;
        public Arc Inner2;
        public Arc Inner3;

        public Digit(int value,int position)
        {
            Value = value;
            Position = position;


            Outer = new Arc(Arc.ArcType.Outer,this);
            Inner1 = new Arc(Arc.ArcType.Inner1,this);
            Inner2 = new Arc(Arc.ArcType.Inner2,this);
            Inner3 = new Arc(Arc.ArcType.Inner3,this);
        }

        public void Draw(Graphics graphics)
        {
            Outer.Draw(graphics);
            Inner1.Draw(graphics);
            Inner2.Draw(graphics);
            Inner3.Draw(graphics);
        }
    }

    public class Arc
    {
        public static float Thickness = 2.0F;
        public static int Opacity = 255;
        public static int Red = 0;
        public static int Green = 0;
        public static int Blue = 0;
        public enum ArcType { Outer, Inner1, Inner2, Inner3 }

        private ArcType Type;

        public abstract class RadiusConstants
        {
            public static float Outer { get { return (float)(Digit.Size / 2); } }
            public static float Inner1 { get { return (float)(Digit.SmallCircleMultiplier * Outer); } }
            public static float Inner2 { get { return (float)(Inner1 * 0.7176); } }
            public static float Inner3 { get { return (float)(Inner2 * 0.6066); } }

        }

        public float Radius
        {
            get
            {
                switch(Type)
                {
                    default:
                    case ArcType.Outer:
                        return RadiusConstants.Outer;
                    case ArcType.Inner1:
                        return RadiusConstants.Inner1;
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
                switch(Type)
                {
                    default:
                    case ArcType.Outer:
                        return (float)Math.Atan(X * 2 / A);
                    case ArcType.Inner1:
                    case ArcType.Inner2:
                    case ArcType.Inner3:
                        return (float)Math.Atan(-(D - X) * 2 / A);
                }
            }
        }

        private float D { get { return RadiusConstants.Outer; } }
        private float X
        {
            get 
            {
                switch(Type)
                {
                    case ArcType.Outer:
                        {
                            return ((D * D - RadiusConstants.Inner1 * RadiusConstants.Inner1 + RadiusConstants.Outer * RadiusConstants.Outer) / (D * 2));
                        }
                    default:
                        {
                            return ((D * D - Radius * Radius + RadiusConstants.Outer * RadiusConstants.Outer) / (D * 2));
                        }
            }
            }
        }

        private float A
        {
            get
            { 
                switch(Type)
                {
                    case ArcType.Outer:
                        {
                            return (float)(1 / D * Math.Sqrt((RadiusConstants.Inner1 - D - RadiusConstants.Outer) * 
                                                                (RadiusConstants.Outer - RadiusConstants.Inner1 - D) * 
                                                                (RadiusConstants.Outer + RadiusConstants.Inner1 - D) * 
                                                                (D + RadiusConstants.Inner1 + RadiusConstants.Outer)
                                                            ));
                        }
                    default:
                        {
                            return (float)(1 / D * Math.Sqrt((Radius - D - RadiusConstants.Outer) * 
                                (RadiusConstants.Outer - Radius - D) * (RadiusConstants.Outer + Radius - D) * (D + Radius + RadiusConstants.Outer)));
                        }
                }
                
            }
        }

        public float StartAngle
        {
            get
            {
                int digit = Owner.Value;
                double result;
                switch (Type)
                {
                    case ArcType.Outer:
                        {
                            if (digit < 2 || digit > 4)
                            {
                                result = Math.PI - Angle;
                            }
                            else
                            {
                                result = Angle;
                            }
                            break;
                        }
                    case ArcType.Inner1:
                        {
                            if (digit == 0)
                            {
                                result = Math.PI / 3;
                            }
                            else if (digit == 1)
                            {
                                result = 5 * Math.PI / 3;
                            }
                            else
                            {
                                result = Angle;
                            }
                            break;
                        }
                    default:
                        {
                            result = Angle;
                            break;
                        }
                }
                return (float)(((float)result + Owner.Position * Digit.AngleIncrement - Digit.GlobalAngle)%(2*Math.PI));
            }
        }

        public float RotationAngle
        {
            get
            {
                int digit = Owner.Value;
                double result;
                switch(Type)
                {
                    case ArcType.Outer:
                        {
                            if (digit < 2 || digit > 4)
                            {
                                result = -2 * Math.PI + (Digit.NumberOfDigits - 1) * Digit.AngleIncrement;
                            }
                            else 
                            {
                                result = -2 * Angle - Math.PI + (Digit.NumberOfDigits - 1) * Digit.AngleIncrement;
                            }
                            break;
                        }
                    default:
                        {
                            if (digit < 2)
                            {
                                result =  0.001;
                            }
                            else if (digit < 5)
                            {
                                result = 2 * Math.PI;
                            }
                            else if (digit < 8)
                            {
                                result = -1 * (Math.PI + 2 * Angle);
                            }
                            else
                            {
                                result =  -2 * Angle + Math.PI;
                            }
                            break;
                        }
                }
                return (float)result;
            }
        }

        public Digit Owner;

        public Arc(ArcType type, Digit owner)
        {
            Type = type;
            Owner = owner;
        }

        public void Draw(Graphics graphics)
        {
            int digit = Owner.Value;
            var pen = new Pen(Color.FromArgb(Opacity,Red,Green,Blue),Thickness);
            var trueX = Owner.XN - Radius;
            var trueY = Owner.YN - Radius;
            switch (Type)
            {
                case ArcType.Outer:
                    {
                        graphics.DrawArc(pen,Owner.X - Radius,Owner.Y - Radius,Radius*2,Radius*2,RadiansToDegrees(StartAngle),RadiansToDegrees(RotationAngle));
                        break;
                    }
                case ArcType.Inner1:
                    {
                        if (digit < 2)
                        {
                            float x2 = Radius*(float)Math.Cos(StartAngle) + Owner.XN;
                            float y2 = Radius*(float)Math.Sin(StartAngle) + Owner.YN;
                            graphics.DrawLine(pen, Owner.XN, Owner.YN, x2, y2);
                        }
                        else
                        {
                            graphics.DrawArc(pen, trueX, trueY, Radius * 2, Radius * 2, RadiansToDegrees(StartAngle), RadiansToDegrees(RotationAngle));
                        }
                        break;
                    }
                case ArcType.Inner2:
                    {
                        switch(digit)
                        {
                            case 3:
                            case 4:
                            case 6:
                            case 7:
                            case 9:
                                {
                                    graphics.DrawArc(pen, trueX, trueY, Radius * 2, Radius * 2, RadiansToDegrees(StartAngle), RadiansToDegrees(RotationAngle));
                                    break;
                                }
                        }
                        break;
                    }
                case ArcType.Inner3:
                    {
                        switch(digit)
                        {
                            case 4:
                            case 7:
                                {
                                    graphics.DrawArc(pen, trueX, trueY, Radius * 2, Radius * 2, RadiansToDegrees(StartAngle), RadiansToDegrees(RotationAngle));
                                    break;
                                }
                        }
                        break;
                    }
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
