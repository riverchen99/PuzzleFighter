using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuzzleFighter {
    public partial class PuzzleFighterGame : Form {
		Board b;
		int xSize = 6;
		int ySize = 15;
		int gridSize = 25;
		Bitmap Backbuffer;
		Timer GameTimer = new Timer();

		public PuzzleFighterGame() {
            InitializeComponent();

			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

			this.KeyPress += new KeyPressEventHandler(PuzzleFighterGame_KeyPress);

			this.ResizeEnd += new EventHandler(PuzzleFighterGame_CreateBackBuffer);
			this.Load += new EventHandler(PuzzleFighterGame_CreateBackBuffer);
			this.Paint += new PaintEventHandler(PuzzleFighterGame_Paint);

			GameTimer.Interval = 1000; // ms
			GameTimer.Tick += new EventHandler(GameTimer_Tick);
			GameTimer.Start();

			b = new Board(xSize, ySize);
        }

		void GameTimer_Tick(object sender, EventArgs e) {
			// handle logic
			b.moveCurrent(Piece.Direction.Down);
			draw();
		}

		//input
		void PuzzleFighterGame_KeyPress(object sender, KeyPressEventArgs e) {
			Console.WriteLine(e.KeyChar);
			switch (e.KeyChar) {
				case 'a':
					b.moveCurrent(Piece.Direction.Left);
					break;
				case 's':
					b.moveCurrent(Piece.Direction.Down);
					break;
				case 'd':
					b.moveCurrent(Piece.Direction.Right);
					break;
				case 'w':
					b.lockPiece();
					break;
			}
			draw();
		}

		// graphics
		void PuzzleFighterGame_Paint(object sender, PaintEventArgs e) {
			if (Backbuffer != null) {
				e.Graphics.DrawImageUnscaled(Backbuffer, Point.Empty);
			}
		}
		void PuzzleFighterGame_CreateBackBuffer(object sender, EventArgs e) {
			if (Backbuffer != null) {
				Backbuffer.Dispose();
			}
			Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
		}
		void draw() {
			if (Backbuffer != null) {
				using (var g = Graphics.FromImage(Backbuffer)) {
					g.Clear(Color.Black);
					Pen p = new Pen(Color.White, 1);
					for (int i = 0; i < xSize; i++) {
						for (int j = 0; j < ySize; j++) {
							g.DrawRectangle(p, new Rectangle(gridSize * i, gridSize * j, gridSize, gridSize));
							if (b.grid[i, j] != null) {
								renderBlock(g, b.grid[i, j]);
							}
						}
					}
					renderBlock(g, b.currentPiece.b1);
					renderBlock(g, b.currentPiece.b2);
				}
				Invalidate();
			}
		}
		void renderBlock(Graphics g, Block b) {
			SolidBrush brush = new SolidBrush(Color.FromName(b.color.ToString()));
			if (b.type == BlockType.Normal) {
				g.FillRectangle(brush, new Rectangle(gridSize * b.x, gridSize * b.y, gridSize, gridSize));
			} else if (b.type == BlockType.Clear) {
				g.FillEllipse(brush, new Rectangle(gridSize * b.x, gridSize * b.y, gridSize, gridSize));
			}
			brush.Dispose();
		}
	}
}
