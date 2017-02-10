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
		Graphics g;
		int xSize = 6;
		int ySize = 15;
		Bitmap Backbuffer;
		public PuzzleFighterGame() {
            InitializeComponent();
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
			this.Load += new EventHandler(Form1_CreateBackBuffer);
			this.Paint += new PaintEventHandler(Form1_Paint);
			b = new Board(xSize, ySize);
			b.grid[0, 0] = new Block(0, 0);
			b.grid[0, 1] = new Block(0, 1);
			b.printGrid();
			
			Console.WriteLine();
			//b.dropBlocks();
			b.printGrid();
			
			/*
			Console.WriteLine(b.currentPiece);
			Console.WriteLine();
			while (true) {
				b.dropCurrent();
				Console.WriteLine(b.currentPiece);
				Console.WriteLine();
			}
			*/
        }
        void Form1_KeyPress(object sender, KeyPressEventArgs e) {
            Console.WriteLine(e.KeyChar);
        }
		void Form1_Paint(object sender, PaintEventArgs e) {
			if (Backbuffer != null) {
				e.Graphics.DrawImageUnscaled(Backbuffer, Point.Empty);
			}
		}
		void Form1_CreateBackBuffer(object sender, EventArgs e) {
			if (Backbuffer != null) {
				Backbuffer.Dispose();
			}
			Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
		}
		void draw() {
			if (Backbuffer != null) {
				using (var g = Graphics.FromImage(Backbuffer)) {
					g.Clear(Color.Black);
					SolidBrush brush;
					Pen p = new Pen(Color.White, 1);
					for (int i = 0; i < xSize; i++) {
						for (int j = 0; j < ySize; j++) {
							g.DrawRectangle(p, new Rectangle(10 * i, 10 * j, 10, 10));
							if (b.grid[i, j] != null) {
								brush = new SolidBrush(Color.FromName(b.grid[i, j].color.ToString()));
								if (b.grid[i, j].type == BlockType.Normal) {
									g.FillRectangle(brush, new Rectangle(10 * i, 10 * j, 10, 10));
								} else if (b.grid[i, j].type == BlockType.Clear) {
									g.FillEllipse(brush, new Rectangle(10 * i, 10 * j, 10, 10));
								}
								brush.Dispose();
							}
						}
					}
				}
				Invalidate();
			}
		}
		private void button1_Click(object sender, EventArgs e) {
			draw();
			b.dropBlocks();
		}
	}
}
