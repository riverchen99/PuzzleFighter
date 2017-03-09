using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuzzleFighter {
	public partial class PuzzleFighterGame : Form {
		Board b;
		int xSize = 6;
		int ySize = 15;
		int gridSize = 25;
		Bitmap Backbuffer;
		System.Windows.Forms.Timer GameTimer = new System.Windows.Forms.Timer();

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
			if (b.moveCurrent(Piece.Direction.Down)) {
				updateBoard();
			}
			draw();
		}

		void updateBoard() {
			bool canCombo;
			b.lockPiece();
			b.dropBlocks();
			b.updateLockBlocks();
			do {
				// piece locked and dropped to bottom
				canCombo = false;
				b.detect2x2();
				b.expandPowerGems();
				b.conbinePowerGems();
				draw();
				Thread.Sleep(100);

				canCombo = b.clearBlocks();
				draw();
				Thread.Sleep(100);

				do {
					draw();
					Thread.Sleep(50);
				} while (b.dropOnce() | b.dropPowerGemsOnce());
			} while (canCombo);
		}

		void sendLockBlocks(int n) {
			BlockColor[][] pattern = new BlockColor[][] 
			{
				new BlockColor[] { BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red },
				new BlockColor[] { BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue }
			};

			int patternRow = 0;
			while (n > 0) {
				if (n > xSize) {
					b.sendLockBlocks(xSize, pattern[patternRow], false);
				} else {
					b.sendLockBlocks(n, pattern[patternRow], false);
				}
				n -= xSize;
				if (!b.dropOnce()) {
					Console.WriteLine("game over");
				}
				draw();
				Thread.Sleep(50);
			}
			do {
				draw();
				Thread.Sleep(50);
			} while (b.dropOnce() | b.dropPowerGemsOnce());

		}
		int colorIndex = 0;
		int typeIndex = 0;
		void PuzzleFighterGame_KeyPress(object sender, KeyPressEventArgs e) {
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
					updateBoard();
					break;
				case 'j':
					b.rotateCurrent(Piece.rotateDirection.CW);
					break;
				case 'k':
					b.rotateCurrent(Piece.rotateDirection.CCW);
					break;
				case 'r':
					b.currentPiece.b1.color = BlockColor.Red;
					b.currentPiece.b1.type = BlockType.Normal;
					b.currentPiece.b2.color = BlockColor.Red;
					b.currentPiece.b2.type = BlockType.Normal;
					break;
				case '1':
					b.currentPiece.b1.color = (BlockColor)Block.colorValues.GetValue(colorIndex++ % 4);
					break;
				case '2':
					b.currentPiece.b2.color = (BlockColor)Block.colorValues.GetValue(colorIndex++ % 4);
					break;
				case '3':
					b.currentPiece.b1.type = (BlockType)Block.typeValues.GetValue(typeIndex++ % 4);
					break;
				case '4':
					b.currentPiece.b2.type = (BlockType)Block.typeValues.GetValue(typeIndex++ % 4);
					break;
				case '5':
					b.currentPiece.b1.unlockTime++;
					break;
				case 'p':
					updateBoard();
					sendLockBlocks(15);
					break;
			}
			draw();
		}

		#region graphics
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
					drawGrid(g);
					drawBlock(g, b.currentPiece.b1);
					drawBlock(g, b.currentPiece.b2);
					drawNextPiece(g);
					drawPowerGems(g);
					drawText(g);
				}
				Invalidate();
				Update();
			}
		}
		void drawGrid(Graphics g) {
			Pen p = new Pen(Color.White, 1);
			for (int i = 0; i < xSize; i++) {
				for (int j = 0; j < ySize; j++) {
					g.DrawRectangle(p, gridSize * i, gridSize * j, gridSize, gridSize);
					if (b.grid[i, j] != null) {
						drawBlock(g, b.grid[i, j]);
					}
				}
			}
			p.Dispose();
		}
		void drawBlock(Graphics g, Block b) {
			SolidBrush brush = new SolidBrush(Color.FromName(b.color.ToString()));
			if (b.type == BlockType.Normal) {
				g.FillRectangle(brush, gridSize * b.x, gridSize * b.y, gridSize, gridSize);
			} else if (b.type == BlockType.Clear) {
				g.FillEllipse(brush, gridSize * b.x, gridSize * b.y, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, gridSize * b.x + gridSize / 3, gridSize * b.y + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, gridSize * b.x, gridSize * b.y, gridSize, gridSize, -60, -60);
			} else if (b.type == BlockType.Lock) {
				g.FillRectangle(brush, gridSize * b.x, gridSize * b.y, gridSize, gridSize);
				g.DrawString(b.unlockTime.ToString(), new Font("Comic Sans", 16), new SolidBrush(Color.Tomato), gridSize * b.x, gridSize * b.y);
			}
			brush.Dispose();
		}
		void drawNextPiece(Graphics g) {
			SolidBrush brush = new SolidBrush(Color.FromName(b.nextPiece.b1.color.ToString()));
			if (b.nextPiece.b1.type == BlockType.Normal) {
				g.FillRectangle(brush, gridSize * (xSize + 1), gridSize, gridSize, gridSize);
			} else if (b.nextPiece.b1.type == BlockType.Clear) {
				g.FillEllipse(brush, gridSize * (xSize + 1), gridSize, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, gridSize * (xSize + 1) + gridSize / 3, gridSize + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.nextPiece.b1.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, gridSize * (xSize + 1), gridSize, gridSize, gridSize, -60, -60);
			}
			brush = new SolidBrush(Color.FromName(b.nextPiece.b2.color.ToString()));
			if (b.nextPiece.b2.type == BlockType.Normal) {
				g.FillRectangle(brush, gridSize * (xSize + 1), gridSize * 2, gridSize, gridSize);
			} else if (b.nextPiece.b2.type == BlockType.Clear) {
				g.FillEllipse(brush, gridSize * (xSize + 1), gridSize * 2, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, gridSize * (xSize + 1) + gridSize / 3, gridSize*2 + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.nextPiece.b2.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, gridSize * (xSize + 1), gridSize * 2, gridSize, gridSize, -60, -60);
			}
		}
		void drawPowerGems(Graphics g) {
			Pen pen = new Pen(Color.Tomato, 3);
			foreach (PowerGem p in b.powerGems) {
				g.DrawRectangle(pen, gridSize * p.x, gridSize * p.y - gridSize * (p.height - 1), gridSize * p.width, gridSize * p.height);
			}
		}
		void drawText(Graphics g) {
			Font f = new Font("Comic Sans", 16);
			SolidBrush br = new SolidBrush(Color.White);
			g.DrawString(b.score.ToString(), f, br, gridSize * (xSize + 1), gridSize * 3);
			for (int i = 0; i < xSize; i++) {
				g.DrawString(i.ToString(), f, br, i * gridSize, ySize * gridSize);
			}
			for (int i = 0; i < ySize; i++) {
				g.DrawString(i.ToString(), f, br, xSize * gridSize, i * gridSize);
			}
		}
		#endregion
	}
}
