using CocoDrawParser;
using System.Collections.Generic;
using System.Drawing;

namespace Lander
{
    public class Pad
    {
        public int X1 { get; set; }
        public int Fuel { get; set; }
    }
    public class Landscape
    {
        //lines 550f.
        //except that I'm not starting the lander on an initial horizontal vector.
        public int SX { get; set; }
        public int SY { get; set; }
        public Pad[] Pads { get; set; }

        public static Landscape Draw550(Graphics g, Pen p)
        {
            //a city...?
            var landscape = new Landscape()
            {
                Pads = new Pad[] {
                    new Pad() { X1 = 126, Fuel = 250 },
                    new Pad() { X1=49, Fuel = 900},
                    new Pad() { X1 = 161, Fuel = 1300 }
                },
                SX = 3,
                SY = 3
            };
            //CIRCLE<0,140>,30,,1,.75,0 is a partial circular curve from .75 to the start.
            //DRR: trying to match the endpoint to the vert-down line. see DRAW subsequent...
            g.DrawArc(p, 0, 80, 60, 60, 90, 90);

            var td = new TurtleDrawer(g, p);
            td.Draw("S4BM30,140D20E15R25F15D10R5");//starts with l.560's (30,140)-(30,160)

            var b = new SolidBrush(p.Color);
            float y3 = 90, y4 = 120;
            for (int u7 = 170; u7 >= 50; u7 -= 10)
            {
                g.FillRectangle(b, y3, u7, y4 - y3, 10); //(y4, u7 + 10)pset,bf
                y3 += 1.2F;
                y4 -= 1.2F;
            }

            td.Draw("BM120,170E10R7D20R24U20R10F10R12U60R11U40R24D40R11D60R15");
            g.DrawLine(p, 255, 170, 255, 171); //DRR: patch edge for the PAINT, later

            g.DrawLine(p, 205, 69, 227, 69);
            g.DrawLine(p, 206, 68, 226, 68);
            return landscape;
        }

        public static Landscape Draw640(Graphics g, Pen p)
        {
            //rough terrain
            var landscape = new Landscape()
            {
                Pads = new Pad[] {
                    new Pad() { X1 = 100, Fuel = 200 },
                    new Pad() { X1=160, Fuel =1150},
                    new Pad() { X1 = 168, Fuel = 700 }
                },
                SX = 3,
                SY = 3
            };
            var td = new TurtleDrawer(g, p);
            td.Draw("S12BM0,145F1D2F3D8R8U12R2U4E7R8F5R3F6R2D1R1E2F3R2E3R1E2F3R1F3R2D6R8U6R2E3F2R4");
            return landscape;
        }

        public static Landscape Draw660(Graphics g, Pen p)
        {
            //tower on terrain
            var landscape = new Landscape()
            {
                Pads = new Pad[] {
                    new Pad() { X1 = 43, Fuel = 1100 },
                    new Pad() { X1=94, Fuel = 700},
                    new Pad() { X1 = 132, Fuel = 500 }
                },
                SX = 128,
                SY = 5
            };
            //CIRCLE<0,140>,30,,1,.75,0 is a partial circular curve from .75 to the start.
            var b = new SolidBrush(p.Color);
            var td = new TurtleDrawer(g, p);
            td.Draw("S4BM0,110R4D1R1D1R1D1F3R1F1R2F2R2F2R5F3D1F1R1D36R44U24E1R1E2R2E2R2E1R1E1R1E2R3E4U2E2U1E2R2E4R1E1R25U22R3F2R5F2R5R1F1R3F1R5F2R4F2R1F3R3F4D3F3R6D2F2R3F2R3D1R2D1R2D1F4R2F3R2F3D3F3R2D4F1D1F1D5R25U8U13E1R1E8R4");

            g.DrawRectangle(p, 38, 62, 25, 100); //- 63,162 pset,b
            g.DrawLine(p, 37, 63, 64, 63);
            g.DrawLine(p, 37, 64, 64, 64);

            for (int x8 = 62; x8 <= 142; x8 += 25)
            {
                g.DrawLine(p, 38, x8, 63, x8 + 25);
                g.DrawLine(p, 63, x8, 38, x8 + 25);
                g.DrawLine(p, 38, x8, 63, x8);
            }

            td.Draw("BM120,1701E10R7D20R24U20" +
                "R10F10R12U60R11U40R24D40R11D60R15");

            return landscape;
        }
    }
}
