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
		int pieceDropInterval = 500;
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
			//this.FormClosing += new FormClosingEventHandler(PuzzleFighterGame_FormClosing);
			b1 = new Board(xSize, ySize, 1);
			b2 = new Board(xSize, ySize, 2);

			b1StateObject = new TimerStateObject(b1);
			b1StateObject.gridSize = gridSize;
			b2StateObject = new TimerStateObject(b2);
			b2StateObject.gridSize = gridSize;

			b1Timer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerTask), b1StateObject, 0, pieceDropInterval/gridSize);
			b1StateObject.TimerReference = b1Timer;

			b2Timer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerTask), b2StateObject, 0, pieceDropInterval/gridSize);
			b2StateObject.TimerReference = b2Timer;
		}

		public class TimerStateObject {
			public System.Threading.Timer TimerReference;
			public Board b;
			public bool hardDrop = false;
			public int gridSize;
			public TimerStateObject(Board b) {
				this.b = b;
				this.b.pieceOffset = -gridSize;
			}
		}
		private void TimerTask(object StateObject) {
			TimerStateObject State = (TimerStateObject)StateObject;
			if (State.b.pieceOffset < 0 && !State.hardDrop) {
				State.b.pieceOffset++;
				draw();
			} else {
				State.b.pieceOffset = -gridSize;
				if (State.b.gameOver) { State.TimerReference.Dispose(); }
				if (State.b.currentPiece != null) {
					if (State.b.moveCurrent(Piece.Direction.Down) || State.hardDrop) {
						updateBoard(State.b);
					}
					State.hardDrop = false;
					draw();
				}
			}
		}
		/*
		private void PuzzleFighterGame_FormClosing(object sender, FormClosingEventArgs e) {
			b1Timer.Dispose();
			b2Timer.Dispose();
		}
		*/
		private void updateBoard(Board b) {
			//Console.WriteLine("thread id:{0} Time:{1} ", Thread.CurrentThread.ManagedThreadId.ToString(), DateTime.Now.ToLongTimeString());
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
			sendCount += comboCount > 1 ? (comboCount-1)*3 : 0;
			//sendCount = (int)(sendCount * .66);
			b.lockBuffer -= sendCount;
			if (b.lockBuffer < 0) {
				targetBoard.lockBuffer -= b.lockBuffer;
				b.lockBuffer = 0;
			}
			sendLockBlocks(b);
			b.checkGameOver();
			if (!b.gameOver) {
				b.newPiece();
			}
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
		private void sendLockBlocks(Board targetBoard) {
			BlockColor[][] pattern = targetBoard.id == 1 ? pattern1 : pattern2;
			int patternRow = 0;
			while (targetBoard.lockBuffer > 0) {
				if (targetBoard.lockBuffer > xSize) {
					targetBoard.sendLockBlocks(xSize, pattern[patternRow % pattern.Length], false);
					targetBoard.lockBuffer -= xSize;
				} else {
					targetBoard.sendLockBlocks(targetBoard.lockBuffer, pattern[patternRow % pattern.Length], false);
					targetBoard.lockBuffer = 0;
				}
				patternRow++;
				if (!targetBoard.dropOnce()) {
					targetBoard.gameOver = true;
				}
				draw();
				Thread.Sleep(blockDropInterval);
			}
			do {
				draw();
				Thread.Sleep(blockDropInterval);
			} while (targetBoard.dropOnce() | targetBoard.dropPowerGemsOnce());
		}
		int colorIndex = 0;
		int typeIndex = 0;
		private void p1_KeyPress(object sender, KeyPressEventArgs e) {
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
					b1StateObject.hardDrop = true;
					//b1Timer.Change(0, pieceDropInterval);
					break;
				case 'f':
					b1.rotateCurrent(Piece.rotateDirection.CW);
					break;
				case 'g':
					b1.rotateCurrent(Piece.rotateDirection.CCW);
					break;
				// debug
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
		private void p2_KeyPress(object sender, KeyPressEventArgs e) {
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
					//b2Timer.Change(0, pieceDropInterval);
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
		private void PuzzleFighterGame_Paint(object sender, PaintEventArgs e) {
			if (Backbuffer != null) {
				e.Graphics.DrawImageUnscaled(Backbuffer, Point.Empty);
			}
		}
		private void PuzzleFighterGame_CreateBackBuffer(object sender, EventArgs e) {
			if (Backbuffer != null) {
				Backbuffer.Dispose();
			}
			Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
		}
		private delegate void drawCallback();
		private void draw() {
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
		private void drawBoard(Graphics g, Board b) {
			int xOffset = b.id == 1 ? 0 : 300;
			int yOffset = 0;
			drawGrid(g, b, xOffset, yOffset);
			if (b.currentPiece != null) {
				drawBlock(g, b.currentPiece.b1, xOffset, yOffset+b.pieceOffset);
				drawBlock(g, b.currentPiece.b2, xOffset, yOffset+b.pieceOffset);
			}
			drawNextPiece(g, b, xOffset, yOffset);
			drawPowerGems(g, b, xOffset, yOffset);
			drawText(g, b, xOffset, yOffset);
		}
		private void drawGrid(Graphics g, Board b, int xOffset, int yOffset) {
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
		private void drawBlock(Graphics g, Block b, int xOffset, int yOffset) {
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
		private void drawNextPiece(Graphics g, Board b, int xOffset, int yOffset) {
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
		private void drawPowerGems(Graphics g, Board b, int xOffset, int yOffset) {
			Pen pen = new Pen(Color.Tomato, 3);
			foreach (PowerGem p in b.powerGems) {
				g.DrawRectangle(pen, xOffset + gridSize * p.x, yOffset + gridSize * p.y - gridSize * (p.height - 1), gridSize * p.width, gridSize * p.height);
			}
		}
		private void drawText(Graphics g, Board b, int xOffset, int yOffset) {
			Font f = new Font("Comic Sans", 16);
			SolidBrush br = new SolidBrush(Color.White);
			g.DrawString(b.score.ToString(), f, br, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 3);
			g.DrawString(b.lockBuffer.ToString(), f, br, xOffset + gridSize * (xSize + 1), yOffset + gridSize * 4);
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
