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
		int xSize = 6;
		int ySize = 15;
		int gridSize = 25;
		int pieceDropInterval = 1000;
		int blockDropInterval = 50;
		int clearDelay = 100;

		Bitmap Backbuffer;
		Board b1;
		Board b2;
		System.Threading.Timer b1Timer;
		System.Threading.Timer b2Timer;
		TimerStateObject b1StateObject;
		TimerStateObject b2StateObject;
		public PuzzleFighterGame() {
			InitializeComponent();
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
			this.KeyPress += new KeyPressEventHandler(p1_KeyPress);
			this.KeyPress += new KeyPressEventHandler(p2_KeyPress);
			this.ResizeEnd += new EventHandler(PuzzleFighterGame_CreateBackBuffer);
			this.Load += new EventHandler(PuzzleFighterGame_CreateBackBuffer);
			this.Paint += new PaintEventHandler(PuzzleFighterGame_Paint);
			b1 = new Board(xSize, ySize, 1);
			b2 = new Board(xSize, ySize, 2);

			b1StateObject = new TimerStateObject(b1);
			b2StateObject = new TimerStateObject(b2);

			b1Timer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerTask), b1StateObject, 0, pieceDropInterval);
			b1StateObject.TimerReference = b1Timer;

			b2Timer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerTask), b2StateObject, 0, pieceDropInterval);
			b2StateObject.TimerReference = b2Timer;
			/*
			b1Timer.Interval = pieceDropInterval; // ms
			b1Timer.Tick += new EventHandler(b1Timer_Tick);
			b1Timer.Start();
			
			b2Timer.Interval = pieceDropInterval; // ms
			b2Timer.Tick += new EventHandler(b2Timer_Tick);
			b2Timer.Start();
			*/
		}
		/*
		void b1Timer_Tick(object sender, EventArgs e) {
			if (b1.gameOver) {
				b1Timer.Stop();
			}
			if (b1.moveCurrent(Piece.Direction.Down)) {
				updateBoard(b1);
			}
			draw();
		}
		void b2Timer_Tick(object sender, EventArgs e) {
			if (b2.gameOver) {
				b2Timer.Stop();
			}
			if (b2.moveCurrent(Piece.Direction.Down)) {
				updateBoard(b2);
			}
			draw();
		}
		*/
		private class TimerStateObject {
			public System.Threading.Timer TimerReference;
			public Board b;
			public bool hardDrop = false;
			public TimerStateObject(Board b) {
				this.b = b;
			}
		}
		private void TimerTask(object StateObject) {
			TimerStateObject State = (TimerStateObject)StateObject;
			if (State.b.gameOver) { State.TimerReference.Dispose(); }
			if (State.b.currentPiece != null) {
				if (State.b.moveCurrent(Piece.Direction.Down) || State.hardDrop) {
					updateBoard(State.b);
				}
				State.hardDrop = false;
				draw();
			}
		}

		int updateBoard(Board b) {
			Console.WriteLine("thread id:{0} Time:{1} ", Thread.CurrentThread.ManagedThreadId.ToString(), DateTime.Now.ToLongTimeString());
			bool canCombo;
			int sendCount = 0;
			int comboCount = 0;
			b.lockPiece();
			b.updateLockBlocks();
			do {
				// piece locked and dropped to bottom
				canCombo = false;
				b.detect2x2();
				b.expandPowerGems();
				b.conbinePowerGems();
				draw();
				Thread.Sleep(clearDelay);

				int t = b.clearBlocks();
				canCombo = t > 0;
				sendCount += t;
				comboCount += canCombo ? 1 : 0;
				if (canCombo) {
					draw();
					Thread.Sleep(clearDelay);
				}

				while (b.dropOnce() | b.dropPowerGemsOnce()) {
					draw();
					Thread.Sleep(blockDropInterval);
				}
			} while (canCombo);
			Board targetBoard = b.id == b1.id ? b2 : b1;
			targetBoard.lockBuffer += (comboCount - 1) * 3 + sendCount;
			sendLockBlocks(b);
			if (!b.gameOver) {
				b.newPiece();
			}
			return comboCount + sendCount;
		}

		BlockColor[][] pattern1 = new BlockColor[][] {
				new BlockColor[] { BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, BlockColor.Red, },
				new BlockColor[] { BlockColor.Green, BlockColor.Green, BlockColor.Green, BlockColor.Green, BlockColor.Green, BlockColor.Green, },
				new BlockColor[] { BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, },
				new BlockColor[] { BlockColor.Yellow, BlockColor.Yellow, BlockColor.Yellow, BlockColor.Yellow, BlockColor.Yellow, BlockColor.Yellow, },
		};
		BlockColor[][] pattern2 = new BlockColor[][] {
				new BlockColor[] { BlockColor.Red, BlockColor.Yellow,BlockColor.Red, BlockColor.Yellow,BlockColor.Red, BlockColor.Yellow, },
				new BlockColor[] { BlockColor.Red, BlockColor.Yellow,BlockColor.Red, BlockColor.Yellow,BlockColor.Red, BlockColor.Yellow, },
				new BlockColor[] { BlockColor.Green, BlockColor.Green, BlockColor.Green, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, },
				new BlockColor[] { BlockColor.Green, BlockColor.Green, BlockColor.Green, BlockColor.Blue, BlockColor.Blue, BlockColor.Blue, },
		};
		void sendLockBlocks(Board targetBoard) {
			BlockColor[][] pattern = targetBoard.id == 1 ? pattern1 : pattern2;
			int patternRow = 0;
			while (targetBoard.lockBuffer > 0) {
				if (targetBoard.lockBuffer > xSize) {
					targetBoard.sendLockBlocks(xSize, pattern[patternRow % pattern.Length], false);
				} else {
					targetBoard.sendLockBlocks(targetBoard.lockBuffer, pattern[patternRow % pattern.Length], false);
				}
				targetBoard.lockBuffer -= xSize;
				patternRow++;
				if (!targetBoard.dropOnce()) {
					targetBoard.gameOver = true;
				}
				draw();
				Thread.Sleep(blockDropInterval);
			}
			targetBoard.lockBuffer = 0;
			do {
				draw();
				Thread.Sleep(blockDropInterval);
			} while (targetBoard.dropOnce() | targetBoard.dropPowerGemsOnce());

		}
		int colorIndex = 0;
		int typeIndex = 0;
		void p1_KeyPress(object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case 'a':
					b1.moveCurrent(Piece.Direction.Left);
					break;
				case 's':
					b1.moveCurrent(Piece.Direction.Down);
					break;
				case 'd':
					b1.moveCurrent(Piece.Direction.Right);
					break;
				case 'w':
					//updateBoard(b1);
					//b1.lockPiece();
					b1StateObject.hardDrop = true;
					b1Timer.Change(0, pieceDropInterval);
					break;
				case 'f':
					b1.rotateCurrent(Piece.rotateDirection.CW);
					break;
				case 'g':
					b1.rotateCurrent(Piece.rotateDirection.CCW);
					break;
				case 'r':
					b1.currentPiece.b1.color = BlockColor.Red;
					b1.currentPiece.b1.type = BlockType.Normal;
					b1.currentPiece.b2.color = BlockColor.Red;
					b1.currentPiece.b2.type = BlockType.Normal;
					break;
				case '1':
					b1.currentPiece.b1.color = (BlockColor)Block.colorValues.GetValue(colorIndex++ % 4);
					break;
				case '2':
					b1.currentPiece.b2.color = (BlockColor)Block.colorValues.GetValue(colorIndex++ % 4);
					break;
				case '3':
					b1.currentPiece.b1.type = (BlockType)Block.typeValues.GetValue(typeIndex++ % 4);
					break;
				case '4':
					b1.currentPiece.b2.type = (BlockType)Block.typeValues.GetValue(typeIndex++ % 4);
					break;
				case '5':
					b1.currentPiece.b1.unlockTime++;
					break;
				case 'p':
					updateBoard(b1);
					b1.lockBuffer = 15;
					sendLockBlocks(b1);
					break;
			}
			draw();
		}
		void p2_KeyPress(object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case 'j':
					b2.moveCurrent(Piece.Direction.Left);
					break;
				case 'k':
					b2.moveCurrent(Piece.Direction.Down);
					break;
				case 'l':
					b2.moveCurrent(Piece.Direction.Right);
					break;
				case 'i':
					b2StateObject.hardDrop = true;
					b2Timer.Change(0, pieceDropInterval);
					//updateBoard(b2);
					break;
				case ';':
					b2.rotateCurrent(Piece.rotateDirection.CW);
					break;
				case '\'':
					b2.rotateCurrent(Piece.rotateDirection.CCW);
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
		delegate void drawCallback();
		void draw() {
			if (this.InvokeRequired) {
				drawCallback d = new drawCallback(draw);
				this.Invoke(d, new object[] { });
			} else {
				if (Backbuffer != null) {
					using (var g = Graphics.FromImage(Backbuffer)) {
						g.Clear(Color.Black);
						drawBoard(g, b1);
						drawBoard(g, b2);
					}
					Invalidate();
					Update();
				}
			}
		}
		void drawBoard(Graphics g, Board b) {
			int xOffset = b.id == 1 ? 0 : 300;
			int yOffset = 0;
			drawGrid(g, b, xOffset, yOffset);
			if (b.currentPiece != null) {
				drawBlock(g, b.currentPiece.b1, xOffset, yOffset);
				drawBlock(g, b.currentPiece.b2, xOffset, yOffset);
			}
			drawNextPiece(g, b, xOffset, yOffset);
			drawPowerGems(g, b, xOffset, yOffset);
			drawText(g, b, xOffset, yOffset);
		}
		void drawGrid(Graphics g, Board b, int xOffset, int yOffset) {
			Pen p = new Pen(Color.White, 1);
			for (int i = 0; i < xSize; i++) {
				for (int j = 0; j < ySize; j++) {
					g.DrawRectangle(p, xOffset + gridSize * i, yOffset + gridSize * j, gridSize, gridSize);
					if (b.grid[i, j] != null) {
						drawBlock(g, b.grid[i, j], xOffset, yOffset);
					}
				}
			}
			p.Dispose();
		}
		void drawBlock(Graphics g, Block b, int xOffset, int yOffset) {
			SolidBrush brush = new SolidBrush(Color.FromName(b.color.ToString()));
			if (b.type == BlockType.Normal) {
				g.FillRectangle(brush, xOffset + gridSize * b.x, yOffset + gridSize * b.y, gridSize, gridSize);
			} else if (b.type == BlockType.Clear) {
				g.FillEllipse(brush, xOffset + gridSize * b.x, yOffset + gridSize * b.y, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, xOffset + gridSize * b.x + gridSize / 3, yOffset + gridSize * b.y + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, xOffset + gridSize * b.x, yOffset + gridSize * b.y, gridSize, gridSize, -60, -60);
			} else if (b.type == BlockType.Lock) {
				g.FillRectangle(brush, xOffset + gridSize * b.x, yOffset + gridSize * b.y, gridSize, gridSize);
				g.DrawString(b.unlockTime.ToString(), new Font("Comic Sans", 16), new SolidBrush(Color.Tomato), xOffset + gridSize * b.x, yOffset + gridSize * b.y);
			}
			brush.Dispose();
		}
		void drawNextPiece(Graphics g, Board b, int xOffset, int yOffset) {
			SolidBrush brush = new SolidBrush(Color.FromName(b.nextPiece.b1.color.ToString()));
			if (b.nextPiece.b1.type == BlockType.Normal) {
				g.FillRectangle(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize, gridSize, gridSize);
			} else if (b.nextPiece.b1.type == BlockType.Clear) {
				g.FillEllipse(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, xOffset + gridSize * (xSize + 1) + gridSize / 3, yOffset + gridSize + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.nextPiece.b1.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize, gridSize, gridSize, -60, -60);
			}
			brush = new SolidBrush(Color.FromName(b.nextPiece.b2.color.ToString()));
			if (b.nextPiece.b2.type == BlockType.Normal) {
				g.FillRectangle(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 2, gridSize, gridSize);
			} else if (b.nextPiece.b2.type == BlockType.Clear) {
				g.FillEllipse(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 2, gridSize, gridSize);
				brush.Color = Color.White;
				g.FillEllipse(brush, xOffset + gridSize * (xSize + 1) + gridSize / 3, yOffset + gridSize * 2 + gridSize / 3, gridSize / 3, gridSize / 3);
			} else if (b.nextPiece.b2.type == BlockType.Diamond) {
				brush.Color = Color.White;
				g.FillPie(brush, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 2, gridSize, gridSize, -60, -60);
			}
		}
		void drawPowerGems(Graphics g, Board b, int xOffset, int yOffset) {
			Pen pen = new Pen(Color.Tomato, 3);
			foreach (PowerGem p in b.powerGems) {
				g.DrawRectangle(pen, xOffset + gridSize * p.x, yOffset + gridSize * p.y - gridSize * (p.height - 1), gridSize * p.width, gridSize * p.height);
			}
		}
		void drawText(Graphics g, Board b, int xOffset, int yOffset) {
			Font f = new Font("Comic Sans", 16);
			SolidBrush br = new SolidBrush(Color.White);
			g.DrawString(b.score.ToString(), f, br, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 3);
			for (int i = 0; i < xSize; i++) {
				g.DrawString(i.ToString(), f, br, xOffset + i * gridSize, yOffset + ySize * gridSize);
			}
			for (int i = 0; i < ySize; i++) {
				g.DrawString(i.ToString(), f, br, xOffset + xSize * gridSize, yOffset + i * gridSize);
			}
		}
		#endregion
	}
}
