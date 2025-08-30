using System;
using System.Drawing;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using CocoDrawParser;

namespace Lander
{
    /// <summary>
    /// The main screen of the Lander game.
    /// </summary>
    /// <remarks>
    /// Line-numbers correspond with the lines in Sullivan's text.
    /// </remarks>
    public partial class MainScreen : Form
    {
        #region "initialise consts"
        //these are static so do 'em here.

        //Sullivan uses all 256 x 192 so the middle is 127, 97.

        //constants from the config properties. So, the player can tweak these.
        //screen 1,0 l. 170:PCLS 1: green (/yellow,blue,red). Also PMODE 0,1 for lowres, two colors.
        //eight possible screens but Sullivan uses only the one. There's no "back buffering".

        //Sullivan implemented a terminal-velocity and a crash noise.
        //that means: an atmosphere; downstream from that, a "silhouette" aesthetic of lightcolor air, dark solids.
        //one of the backgrounds can only be a cityscape.
        //he may have picked Venus because, CoCo green. Mars and Earth are associated with red and blue skies. 

        //anyway these pens can be switched if the terminal velocity is greater than Mars'
        private readonly static Pen PEN_FORE = new Pen(new SolidBrush(Color.Black));
        private readonly static Pen PEN_BACK = new Pen(new SolidBrush(Color.OrangeRed));

        private readonly SoundPlayer _kaboom;

        private readonly Bitmap _bmpLanderBallistic;
        private readonly Bitmap _bmpLanderThrust;

        //this is our one changeable static
        private static int _highscore = 0;
        private Bitmap _bmpLandscape;
        private Bitmap _bmpLander;

        private Landscape _landscape;//for pads

        #endregion

        #region "main screen turn on"

        private Graphics _g;

        //you
        private int _score;
        private PlayerLocation _lander;

        public MainScreen()
        {
            InitializeComponent();
            _kaboom = new SoundPlayer("Kaboom.wav");
            _g = pictureBox1.CreateGraphics();
            //line 110, 130
            _bmpLanderBallistic = new Bitmap(14, 17);
            var g = Graphics.FromImage(_bmpLanderBallistic);
            var landerImage = "U3E2R2NU1R2F2D3G2R2D1L2F2D2NR1NL1U2H2L4G2D2NR1NL1U2E2L2U1R2NR4H2";
            var td = new TurtleDrawer(g, PEN_FORE);
            ////GET<97,91>-<111,108> therefore ...
            td.Draw("BM3,9S4" + landerImage);
            //... and paint 102,100 -> 5,8
            Painter.FloodFill(_bmpLanderBallistic, new Point(5, 8), PEN_FORE.Color, Color.Honeydew);

            //line 120, 140
            _bmpLanderThrust = new Bitmap(14, 17);
            g = Graphics.FromImage(_bmpLanderThrust);
            td = new TurtleDrawer(g, PEN_FORE);
            td.Draw("BM3,9S4" + landerImage + "D3BR3D2R1ND1R1U2");
            Painter.FloodFill(_bmpLanderThrust, new Point(5, 8), PEN_FORE.Color, Color.Honeydew);
            Clipboard.SetImage(_bmpLanderThrust);
            //did this to keep the turtle
        }

        /// <summary>
        /// Gets rollin'
        /// </summary>
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStart.Visible = false;
            btnStart.Click -= btnStart_Click;
            //line 170.

            lblMouseInstr.Visible = false;
            btnStart.Visible = false;
            Application.DoEvents();//enforce all those invisibilities before the main screen shows
            (pictureBox1.CreateGraphics()).Clear(PEN_BACK.Color);

            /*
            //line 170b, once we get Volume in our song-beeper
            Song playAB = new Song(60);
            playAB.Notes.Add(new Note(Duration.Quarter, Pitch.A, 3));
            playAB.Play();*/

            //if we want one
            Splash();

            //150, clear, draw the board.
            _bmpLandscape = new Bitmap(256, 192);
            var g = Graphics.FromImage(_bmpLandscape);
            g.Clear(PEN_BACK.Color);
            _landscape = MakeBoard(g);
            Painter.FloodFill(_bmpLandscape, new Point(_bmpLandscape.Width / 2, _bmpLandscape.Height - 1), PEN_BACK.Color, PEN_FORE.Color);

            //line 260, kinda.
            _score = 0;
            timer1.Interval = 100;

            //lines 100, 180
            //this is Level One. Can be tweaked!
            _lander = new PlayerLocation(4000, _landscape.SX, _landscape.SY, .1, 2); //.1, 2 or .2, 2.4
            _bmpLander = _bmpLanderBallistic;

            //lines 180f, main loop.
            this.pictureBox1.Paint += pictureBox1_Paint;
            this.KeyDown += this.MainScreen_KeyDown;
            this.KeyUp += this.MainScreen_KeyUp;
            timer1.Start();
        }
        #endregion

        #region "setup"
        private void Splash()
        {
            //up to line 140, if we did one
            _g.Clear(Color.Beige);
        }

        private static Landscape MakeBoard(Graphics backGraphics)
        {
            var r = new Random();//nondeterministic.
            switch (r.Next(3))
            {
                case 0:
                    return Landscape.Draw550(backGraphics, PEN_FORE);
                case 1:
                    return Landscape.Draw640(backGraphics, PEN_FORE);
                default:
                    return Landscape.Draw660(backGraphics, PEN_FORE);
            }
        }

        #endregion

        #region "events"

        //player actions

        private void MainScreen_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _lander.IncrementU(-1);
                    break;
                case Keys.Right:
                    _lander.IncrementU(1);
                    break;
                case Keys.Up:
                    if (_lander.HasFuel)
                        _bmpLander = _bmpLanderThrust;
                    break;
            }
        }
        private void MainScreen_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    _bmpLander = _bmpLanderBallistic;
                    break;
            }
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.DrawImage(_bmpLandscape, 0, 0);
            //didn't land or crash? draw, depending on if the thrust is on. lines 240b=280.
            int q = (int)_lander.I, w = (int)_lander.X;
            g.DrawImage(_bmpLander, q, w);
        }

        /// <summary>
        /// gravity takes its toll
        /// </summary>
        /// <remarks>
        ///line 610-630
        ///</remarks>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_bmpLander.Equals(_bmpLanderThrust))
            {
                if (_lander.HasFuel)
                    _lander.Thrust();
                else
                    _bmpLander = _bmpLanderBallistic; //might as well take away MainScreen_KeyUp entirely
            }

            //line 220-240a
            _lander.IncrementVectors();


            if (_lander.I < 1)
            {
                LoseLife();
                return;
            }
            Color pixelL = _bmpLandscape.GetPixel((int)_lander.I - 1, (int)_lander.X + 17),
                pixelR = _bmpLandscape.GetPixel((int)_lander.I + 15, (int)_lander.X + 17);
            if (pixelL == PEN_FORE.Color || pixelR == PEN_FORE.Color)
            {
                if (_lander.TooFast) // || check ppoint. line 350. this is if too fast when running at the pad
                {
                    LoseLife();
                    return;
                }
                //land: lines 350b-370
                foreach (var pad in _landscape.Pads)
                {
                    var targetVert = (int)(_lander.X - 1);
                    if (pad.X1 == targetVert || pad.X1 == targetVert - 1)
                    {
                        AWinnerIsYou(pad.Fuel);
                        return;
                    }
                }
                LoseLife();
                return;
            }
            pictureBox1.Refresh();
        }

        #endregion

        #region "good and bad"

        private void LoseLife()
        {
            timer1.Stop();
            //lines 380 put q, w+t+1... why??
            //assumes atmo.
            _kaboom.Play();
            for (byte y9 = 2; y9 <= 13; y9++)
            {
                _g.DrawEllipse(PEN_FORE, (float)(_lander.I + 7 - y9), (float)(_lander.X + 10 - y9), y9 << 1, y9 << 1);
                Thread.Sleep(10);
            }
            for (byte y9 = 2; y9 <= 13; y9++)
            {
                _g.DrawEllipse(PEN_BACK, (float)(_lander.I + 7 - y9), (float)(_lander.X + 10 - y9), y9 << 1, y9 << 1);
                Thread.Sleep(10);
            }

        //moon-rattling kaboom

        //lines 410-420
        //this fuel/score decrement is based on T, decidedly negative
        int fu = _lander.Z - 2 * 10;//random 20
            _score += _lander.Z;
            if (_score < 0) _score = 0; //lines 410 and 490
            _lander.IncrementF(fu);

            if (!_lander.HasFuel) // also line 410 but buggy in original
                YouFailIt();
            else
            {
                //still alive?
                StringBuilder endMsg = new StringBuilder();
                endMsg.AppendLine($"YOU LOST {_lander.Z} POINTS");
                endMsg.AppendLine("IN THE CRASH");
                endMsg.AppendLine($"YOU LOST {fu} FUEL UNITS");
                endMsg.AppendLine($"YOUR SCORE IS {_score}");
                endMsg.AppendLine($"YOU HAVE {_lander.Fuel} FUEL UNITS");

                DialogResult whut = MessageBox.Show(endMsg.ToString(), "CRASH", MessageBoxButtons.OK);

                //to line 500...
                var g = Graphics.FromImage(_bmpLandscape);
                g.Clear(PEN_BACK.Color);
                _landscape = MakeBoard(g);
                Painter.FloodFill(_bmpLandscape, new Point(_bmpLandscape.Width / 2, _bmpLandscape.Height - 1), PEN_BACK.Color, PEN_FORE.Color);

                _lander.Relocate(_landscape.SX, _landscape.SY);
                _bmpLander = _bmpLanderBallistic;

                timer1.Start();
            }
        }

        /// <summary>
        /// score! 
        /// </summary>
        /// <remarks>
        /// lines 430-440, 470-480.
        /// </remarks>
        private void AWinnerIsYou(int padFuel)
        {
            timer1.Stop();
            //on rnd4, play something nice
            //line 460
            _score += _lander.S1;
            int fu = padFuel + _lander.S1 + 25;// Random(50)
            _lander.IncrementF(fu);
            var td = new TurtleDrawer(_g, PEN_FORE);
            td.Draw("S4BM5,20R10NR10D20BR20U20R15D20NL15BR10NR15U20BR15D20BR10NR15U20R15BR10D10ND10R15NU10D10BR10U20R10F5D10G5NL10BR15NR15U20R15D20BR10NU20E10F10U20BR10ND20F20U20BR10D15BD3D2");
            //play the rest of the tune and line 440
            Thread.Sleep(2000);

            StringBuilder endMsg = new StringBuilder();
            endMsg.AppendLine($"YOU GAINED {_lander.S1} POINTS");
            endMsg.AppendLine($"AND {fu + _lander.S1} FUEL UNITS");
            endMsg.AppendLine($"YOUR SCORE IS {_score}");
            endMsg.AppendLine($"YOU HAVE {_lander.Fuel} FUEL UNITS");

            DialogResult whut = MessageBox.Show(endMsg.ToString(), "TOUCHDOWN!", MessageBoxButtons.OK);

            //off to line 500.
            var g = Graphics.FromImage(_bmpLandscape);
            g.Clear(PEN_BACK.Color);
            _landscape = MakeBoard(g);
            Painter.FloodFill(_bmpLandscape, new Point(_bmpLandscape.Width / 2, _bmpLandscape.Height - 1), PEN_BACK.Color, PEN_FORE.Color);

            _lander.Relocate(_landscape.SX, _landscape.SY);
            _bmpLander = _bmpLanderBallistic;
            timer1.Start();
        }

        /// <summary>
        /// Game Over, man.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        private void YouFailIt()
        {
            timer1.Stop();

            //lines 290-340
            if (_score > _highscore)
                _highscore = _score;

            StringBuilder endMsg = new StringBuilder();
            endMsg.AppendLine("YOU LOST THE SHIP");
            endMsg.Append("YOUR FINAL SCORE WAS ");
            endMsg.Append(_score);
            endMsg.Append(Environment.NewLine);
            endMsg.Append("HIGH SCORE=");
            endMsg.Append(_highscore);
            endMsg.Append(Environment.NewLine);
            endMsg.AppendLine("DO YOU WANT TO PLAY");
            endMsg.AppendLine("AGAIN");

            DialogResult whut = MessageBox.Show(endMsg.ToString(), "Game Over", MessageBoxButtons.YesNo);
            //line 330
            switch (whut)
            {
                case DialogResult.Yes:
                    //THENGOTO 70
                    btnStart.Visible = true;
                    lblMouseInstr.Visible = true;
                    //_backBuffer = null;
                    _g.Clear(Color.LightGreen);
                    break;
                default://THEN END
                    this.Close();
                    break;
            }

            //off to line 150
        }
        #endregion

        private class PlayerLocation
        {
            // based on timer interval of 1000.
            private double _maximumUp = -0.9, _thrustPower = 0.7, _horizBoost = 0.5;//0.09, 0.07, 0.05
            private int _fuelPerThrust = 30; //15 assumed a tenth the power

            private double _u { get; set; }
            private double _t { get; set; }
            private double _gravity { get; set; }
            private double _terminalVelocity { get; set; } // MI
            public int Fuel { get; private set; }
            public double I { get; private set; }
            public double X { get; private set; } //actually vertical

            public bool HasFuel => Fuel > 0;

            public bool TooFast
            =>
                    //line 350. t is vert speed, u is horiz speed.
                     _t > 1.75 || Math.Abs(_u) > 1.4;
                
            
            public int S1 =>
                    //line 460
                     (int)((1.9 - _t) * 75);
                
            
            public int Z
            =>
                    //line 410. except that this is a negative value at this point!
                     -1 * (int)(40 * Math.Abs(_t));


            public PlayerLocation(int f, int sx, int sy, double gravity, double mi)
            {
                Fuel = f;
                Relocate(sx,sy);
                _gravity = gravity;
                _terminalVelocity = mi;
            }
            public void Relocate(int sx, int sy)
            {
                I = sx;
                X = sy;
                _t = 1; //every landscape starts with this initial downfall
                _u = 0;
            }

            public void IncrementF(int fu)
            {
                //line 480. note: fu is doubled to your account!
                Fuel += fu + S1;
            }

            public void IncrementVectors()
            {
                //lines 230-240a... apply gravity.
                _t += _gravity;
                if (_t > _terminalVelocity) _t = _terminalVelocity;
                X += _t;

                if (X < 6) X = 6; //allowed to hit the ceiling. this is transplanted from the Thrust sub.

                //meanwhile, the horizontal.
                I += _u;
            }

            public void IncrementU(double value)
            {
                //190-210, before checking for the thruster - boost horiz speed.
                _u += value * _horizBoost;
                if (Math.Abs(_u) > 1.5) _u = Math.Sign(_u) * 1.5;
            }

            public void Thrust()
            {
                if (Fuel <= 0) return;
                //lines 260-270
                _t -= _thrustPower; //cut speed
                if (_t < _maximumUp) _t = _maximumUp;
                Fuel -= _fuelPerThrust;
            }
        }
    }
}
