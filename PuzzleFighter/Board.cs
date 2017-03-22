using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace PuzzleFighter {
	public class Board {
		public Block[,] grid { get; set; }
		public Piece currentPiece { get; set; }
		public Piece nextPiece { get; set; }
		public int xSize { get; set; }
		public int ySize { get; set; }
		public int score { get; set; }
		public int pieceCount { get; set; }
		public bool gameOver { get; set; }
		public int id { get; set; }
		public int lockBuffer { get; set; }
		public int pieceOffset { get; set; }
		private List<Block> connected;
		private List<Block> connectedLockBlocks;

		public Board(int xSize, int ySize, int id) { // x: 0 -> 5, y: 0 - 14 (two hidden)
			this.xSize = xSize;
			this.ySize = ySize;
			grid = new Block[xSize, ySize];
			currentPiece = new Piece(xSize, ySize);
			nextPiece = new Piece(xSize, ySize);
			score = 0;
			pieceCount = 1;
			powerGems = new HashSet<PowerGem>(new PowerGemEqualityComparer());
			gameOver = false;
			this.id = id;
			lockBuffer = 0;
			pieceOffset = 0;
		}

		public bool checkValid(int x, int y) {
			return (x >= 0 && x < xSize && y >= 0 && y < ySize);
		}

		#region movement
		public bool moveCurrent(Piece.Direction d) {
			if (currentPiece != null) {
				if (checkValid(currentPiece.b1.x + Piece.directionVectors[(int)d][0], currentPiece.b1.y + Piece.directionVectors[(int)d][1]) &&
					checkValid(currentPiece.b2.x + Piece.directionVectors[(int)d][0], currentPiece.b2.y + Piece.directionVectors[(int)d][1]) &&
					grid[currentPiece.b1.x + Piece.directionVectors[(int)d][0], currentPiece.b1.y + Piece.directionVectors[(int)d][1]] == null &&
					grid[currentPiece.b2.x + Piece.directionVectors[(int)d][0], currentPiece.b2.y + Piece.directionVectors[(int)d][1]] == null) {
					currentPiece.move(d);
				} else if (d == Piece.Direction.Down) {
					return true;
				}
			}
			return false;
		}
		public void rotateCurrent(Piece.rotateDirection d) {
			if (currentPiece != null) {
				int[] pieceVector = new int[2] { currentPiece.b1.x - currentPiece.b2.x, currentPiece.b1.y - currentPiece.b2.y };
				Complex i = new Complex(pieceVector[0], pieceVector[1]);
				Complex delta = d == Piece.rotateDirection.CCW ? Complex.Multiply(i, new Complex(0, 1)) : Complex.Multiply(i, new Complex(0, -1));
				if (checkValid(currentPiece.b2.x + (int)delta.Real, currentPiece.b2.y + (int)delta.Imaginary) &&
					checkValid(currentPiece.b1.x, currentPiece.b1.y) &&
					grid[currentPiece.b2.x + (int)delta.Real, currentPiece.b2.y + (int)delta.Imaginary] == null &&
					grid[currentPiece.b1.x, currentPiece.b1.y] == null) {
					currentPiece.b1.x = currentPiece.b2.x + (int)delta.Real;
					currentPiece.b1.y = currentPiece.b2.y + (int)delta.Imaginary;
				} else if (checkValid(currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary) &&
							checkValid(currentPiece.b1.x, currentPiece.b1.y) &&
							grid[currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary] == null &&
							grid[currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary] == null) {
					currentPiece.b1.x = currentPiece.b2.x;
					currentPiece.b1.y = currentPiece.b2.y;
					currentPiece.b2.x -= (int)delta.Real;
					currentPiece.b2.y -= (int)delta.Imaginary;
				} else {
					int tx = currentPiece.b2.x;
					int ty = currentPiece.b2.y;
					currentPiece.b2.x = currentPiece.b1.x;
					currentPiece.b2.y = currentPiece.b1.y;
					currentPiece.b1.x = tx;
					currentPiece.b1.y = ty;
				}
			}
		}
		#endregion

		public void newPiece() {
			currentPiece = nextPiece;
			nextPiece = new Piece(xSize, ySize);
			if (pieceCount % 25 == 0) {
				if (Block.random.NextDouble() < .5) { nextPiece.b1.type = BlockType.Diamond; } else { nextPiece.b2.type = BlockType.Diamond; }
			}
			pieceCount++;
		}
		public bool checkGameOver() {
			gameOver = !(grid[xSize / 2, 0] == null && grid[xSize / 2, 1] == null);
			return gameOver;
		}
		#region drop
		public void lockPiece() {
			grid[currentPiece.b1.x, currentPiece.b1.y] = currentPiece.b1;
			grid[currentPiece.b2.x, currentPiece.b2.y] = currentPiece.b2;
			currentPiece = null;
			dropBlocks(); // inefficient, fix later
			checkGameOver();
		}
		public bool dropBlocks() {
			bool changed = false;
			for (int i = 0; i < xSize; i++) {
				for (int j = ySize - 2; j >= 0; j--) {
					if (grid[i, j] != null && !grid[i, j].inPowerGem) {
						int k = 0;
						while (j + k < ySize - 1 && grid[i, j + k + 1] == null) { k++; }
						if (k > 0) {
							grid[i, j].y = j + k;
							grid[i, j + k] = grid[i, j];
							grid[i, j] = null;
							changed = true;
						}
					}
				}
			}
			return changed;
		}
		public bool dropPowerGems() {
			bool changed = false;
			foreach (PowerGem p in powerGems) {
				int minFallHeight = Int32.MaxValue;
				for (int i = p.x; i < p.x + p.width; i++) {
					int k = 0;
					while (p.y + k < ySize - 1 && grid[i, p.y + k + 1] == null) { k++; }
					if (k < minFallHeight) {
						minFallHeight = k;
					}
				}
				if (minFallHeight > 0 && minFallHeight < Int32.MaxValue) {
					for (int i = p.x; i < p.x + p.width; i++) {
						for (int j = p.y; j > p.y - p.height; j--) {
							grid[i, j].y = j + minFallHeight;
							grid[i, j + minFallHeight] = grid[i, j];
							grid[i, j] = null;
							changed = true;
						}
					}
					p.y = p.y + minFallHeight;
				}
			}
			return changed;
		}
		public bool dropOnce() {
			bool changed = false;
			for (int i = 0; i < xSize; i++) {
				for (int j = ySize - 2; j >= 0; j--) {
					if (grid[i, j] != null && !grid[i, j].inPowerGem) {
						if (grid[i, j + 1] == null) {
							grid[i, j].y++;
							grid[i, j + 1] = grid[i, j];
							grid[i, j] = null;
							changed = true;
						}
					}
				}
			}
			return changed;
		}
		public bool dropPowerGemsOnce() {
			bool changed = false;
			foreach (PowerGem p in powerGems) {
				bool canDrop = true;
				if (p.y + 1 < ySize) {
					for (int i = p.x; i < p.x + p.width; i++) {
						if (grid[i, p.y + 1] != null) {
							canDrop = false;
						}
					}
					if (canDrop) {
						for (int i = p.x; i < p.x + p.width; i++) {
							for (int j = p.y; j > p.y - p.height; j--) {
								grid[i, j].y = j + 1;
								grid[i, j + 1] = grid[i, j];
								grid[i, j] = null;
								changed = true;
							}
						}
						p.y = p.y + 1;
					}
				}
			}
			return changed;
		}
		#endregion

		#region clearing
		public int clearBlocks() {
			int sendAmount = 0;
			HashSet<Block> blocksToRemove = new HashSet<Block>();
			HashSet<PowerGem> gemsToRemove = new HashSet<PowerGem>();
			for (int i = 0; i < xSize; i++) {
				for (int j = 0; j < ySize; j++) {
					if (grid[i, j] != null && grid[i, j].type == BlockType.Clear && !blocksToRemove.Contains(grid[i, j])) {
						connected = new List<Block>();
						connectedLockBlocks = new List<Block>();
						getConnected(grid[i, j]);
						if (connected.Count > 1) {
							foreach (Block bl in connected) {
								blocksToRemove.Add(bl);
							}
							foreach (Block bl in connectedLockBlocks) {
								blocksToRemove.Add(bl);
							}
						}
					} else if (grid[i, j] != null && grid[i, j].type == BlockType.Diamond) {
						blocksToRemove.Add(grid[i, j]);
						if (checkValid(i, j + 1) && grid[i, j + 1] != null && grid[i, j + 1].type != BlockType.Diamond) {
							for (int a = 0; a < xSize; a++) {
								for (int b = 0; b < ySize; b++) {
									if (grid[a, b] != null && grid[a, b].color == grid[i, j + 1].color && !blocksToRemove.Contains(grid[a, b])) {
										blocksToRemove.Add(grid[a, b]);
									}
								}
							}
						}
					}
				}
			}
			foreach (Block b in blocksToRemove) {
				foreach (PowerGem p in powerGems) {
					if (b.x == p.x && b.y == p.y) {
						gemsToRemove.Add(p);
					}
				}
				grid[b.x, b.y] = null;
				score++;
				sendAmount++;
			}
			//sendAmount /= 2;
			foreach (PowerGem p in gemsToRemove) {
				powerGems.Remove(p);
				sendAmount += (p.width * p.height * 2);
			}
			return sendAmount;
		}
		public void getConnected(Block b) {
			connected.Add(b);
			foreach (int[] v in Piece.directionVectors) {
				if (checkValid(b.x + v[0], b.y + v[1]) &&
					grid[b.x + v[0], b.y + v[1]] != null &&
					grid[b.x + v[0], b.y + v[1]].color == b.color &&
					(grid[b.x + v[0], b.y + v[1]].type == BlockType.Normal || grid[b.x + v[0], b.y + v[1]].type == BlockType.Clear) &&
					!connected.Contains(grid[b.x + v[0], b.y + v[1]])) {
					getConnected(grid[b.x + v[0], b.y + v[1]]);
				} else if (checkValid(b.x + v[0], b.y + v[1]) &&
					grid[b.x + v[0], b.y + v[1]] != null &&
					grid[b.x + v[0], b.y + v[1]].type == BlockType.Lock &&
					!connected.Contains(grid[b.x + v[0], b.y + v[1]])) {
					connectedLockBlocks.Add(grid[b.x + v[0], b.y + v[1]]);
				}
			}
		}
		#endregion

		#region powergems
		public HashSet<PowerGem> powerGems;
		public bool detect2x2() {
			bool changed = false;
			for (int i = 0; i < xSize - 1; i++) {
				for (int j = ySize - 1; j > 0; j--) {
					if (grid[i, j] != null &&
						grid[i + 1, j] != null &&
						grid[i, j - 1] != null &&
						grid[i + 1, j - 1] != null &&
						grid[i, j].color == grid[i + 1, j].color &&
						grid[i, j].color == grid[i, j - 1].color &&
						grid[i, j].color == grid[i + 1, j - 1].color &&
						!grid[i, j].inPowerGem &&
						!grid[i + 1, j].inPowerGem &&
						!grid[i, j - 1].inPowerGem &&
						!grid[i + 1, j - 1].inPowerGem &&
						grid[i, j].type == BlockType.Normal &&
						grid[i + 1, j].type == BlockType.Normal &&
						grid[i, j - 1].type == BlockType.Normal &&
						grid[i + 1, j - 1].type == BlockType.Normal) {
						if (powerGems.Add(new PowerGem(i, j, grid[i, j].color))) {
							grid[i, j].inPowerGem = true;
							grid[i + 1, j].inPowerGem = true;
							grid[i, j - 1].inPowerGem = true;
							grid[i + 1, j - 1].inPowerGem = true;
							changed = true;
						}
					}
				}
			}
			return changed;
		}
		public bool expandPowerGems() {
			bool changed = false;
			foreach (PowerGem p in powerGems) {
				bool expandUp = true;
				bool expandDown = true;
				for (int i = 0; i < p.width; i++) {
					if (!checkValid(p.x + i, p.y - p.height) ||
						grid[p.x + i, p.y - p.height] == null ||
						grid[p.x + i, p.y - p.height].color != p.color ||
						grid[p.x + i, p.y - p.height].inPowerGem ||
						grid[p.x + i, p.y - p.height].type != BlockType.Normal) {
						expandUp = false;
					}
					if (!checkValid(p.x + i, p.y + 1) ||
						grid[p.x + i, p.y + 1] == null ||
						grid[p.x + i, p.y + 1].color != p.color ||
						grid[p.x + i, p.y + 1].inPowerGem ||
						grid[p.x + i, p.y + 1].type != BlockType.Normal) {
						expandDown = false;
					}
				}
				if (expandUp) {
					for (int i = 0; i < p.width; i++) {
						grid[p.x + i, p.y - p.height].inPowerGem = true;
					}
					p.height++;
				}
				if (expandDown) {
					for (int i = 0; i < p.width; i++) {
						grid[p.x + i, p.y + 1].inPowerGem = true;
					}
					p.y++;
					p.height++;
				}

				bool expandRight = true;
				bool expandLeft = true;
				for (int i = 0; i < p.height; i++) {
					if (!checkValid(p.x + p.width, p.y - i) ||
						grid[p.x + p.width, p.y - i] == null ||
						grid[p.x + p.width, p.y - i].color != p.color ||
						grid[p.x + p.width, p.y - i].inPowerGem ||
						grid[p.x + p.width, p.y - i].type != BlockType.Normal) {
						expandRight = false;
					}
					if (!checkValid(p.x - 1, p.y - i) ||
						grid[p.x - 1, p.y - i] == null ||
						grid[p.x - 1, p.y - i].color != p.color ||
						grid[p.x - 1, p.y - i].inPowerGem ||
						grid[p.x - 1, p.y - i].type != BlockType.Normal) {
						expandLeft = false;
					}
				}
				if (expandRight) {
					for (int i = 0; i < p.height; i++) {
						grid[p.x + p.width, p.y - i].inPowerGem = true;
					}
					p.width++;
				}
				if (expandLeft) {
					for (int i = 0; i < p.height; i++) {
						grid[p.x - 1, p.y - i].inPowerGem = true;
					}
					p.x--;
					p.width++;
				}
				changed = expandUp || expandDown || expandLeft || expandRight;
			}
			return changed;
		}
		public bool conbinePowerGems() {
			bool changed = false;
			List<PowerGem> toRemove = new List<PowerGem>();
			foreach (PowerGem p1 in powerGems) {
				foreach (PowerGem p2 in powerGems) {
					if (p1.color == p2.color && p1.x == p2.x && p1.y - p1.height == p2.y && p1.width == p2.width) {
						toRemove.Add(p2);
						p1.height += p2.height;
					}
					if (p1.color == p2.color && p1.y == p2.y && p1.x + p1.width == p2.x && p1.height == p2.height && p1.color == p2.color) {
						toRemove.Add(p2);
						p1.width += p2.width;
					}
				}
			}
			foreach (PowerGem p in toRemove) {
				powerGems.Remove(p);
				changed = true;
			}
			return changed;
		}
		#endregion

		#region lockblocks
		public void updateLockBlocks() {
			for (int i = 0; i < xSize; i++) {
				for (int j = 0; j < ySize; j++) {
					if (grid[i, j] != null && grid[i, j].type == BlockType.Lock && grid[i, j].unlockTime > 1) {
						grid[i, j].unlockTime--;
					} else if (grid[i, j] != null && grid[i, j].type == BlockType.Lock && grid[i, j].unlockTime == 1) {
						grid[i, j].type = BlockType.Normal;
					}
				}
			}
		}

		public void sendLockBlocks(int n, BlockColor[] pattern, bool diamond) {
			for (int i = 0; i < xSize; i++) {
				if (grid[i, 0] == null && n > 0) {
					grid[i, 0] = new Block(i, 0, pattern[i], BlockType.Lock, diamond ? 3 : 5);
					n--;
				}
			}
		}
		#endregion

		public void printGrid() {
			for (int j = 0; j < ySize; j++) {
				for (int i = 0; i < xSize; i++) {
					Console.Write(grid[i, j] == null ? 0 : 1);
				}
				Console.WriteLine();
			}
		}
	}
}
