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
		public PuzzleFighterGame() {
            InitializeComponent();
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);

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
		private void draw() {
			g = this.CreateGraphics();
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
			g.Dispose();
		}

		private void button1_Click(object sender, EventArgs e) {
			draw();
			b.dropBlocks();
		}
	}
}
