using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace PuzzleFighter {
	public class Board {
		public Block[,] grid { get; set; }
		public Piece currentPiece { get; set; }
		public Piece nextPiece { get; set; }
		public int xSize { get; set; }
		public int ySize { get; set; }
		
		public Board(int xSize, int ySize) { // x: 0 -> 5, y: 0 - 14 (two hidden)
			this.xSize = xSize;
			this.ySize = ySize;
			grid = new Block[xSize, ySize];
			currentPiece = new Piece(xSize, ySize);
			nextPiece = new Piece(xSize, ySize);
		}

		public void update() {
			bool changed;
			do {
				changed = dropBlocks();
				clearBlocks();
			} while (changed);
		}

		public void moveCurrent(Piece.Direction d) {
			if (checkValid(currentPiece.b1.x + Piece.directionVectors[(int)d][0], currentPiece.b1.y + Piece.directionVectors[(int)d][1]) &&
				checkValid(currentPiece.b2.x + Piece.directionVectors[(int)d][0], currentPiece.b2.y + Piece.directionVectors[(int)d][1]) &&
				grid[currentPiece.b1.x + Piece.directionVectors[(int)d][0], currentPiece.b1.y + Piece.directionVectors[(int)d][1]] == null &&
				grid[currentPiece.b2.x + Piece.directionVectors[(int)d][0], currentPiece.b2.y + Piece.directionVectors[(int)d][1]] == null) {
				currentPiece.move(d);
			} else if (d == Piece.Direction.Down) {
				lockPiece();
			}
		}

		public void rotateCurrent(Piece.rotateDirection d) {
			int[] pieceVector = new int[2] { currentPiece.b1.x - currentPiece.b2.x, currentPiece.b1.y - currentPiece.b2.y };
			Complex i = new Complex(pieceVector[0], pieceVector[1]);
			Complex delta = d == Piece.rotateDirection.CCW ? Complex.Multiply(i, new Complex(0, 1)) : Complex.Multiply(i, new Complex(0, -1));
			if (checkValid(currentPiece.b2.x + (int)delta.Real, currentPiece.b2.y + (int)delta.Imaginary) &&
				checkValid(currentPiece.b1.x, currentPiece.b1.y) &&
				grid[currentPiece.b2.x + (int)delta.Real, currentPiece.b2.y + (int)delta.Imaginary] == null &&
				grid[currentPiece.b1.x, currentPiece.b1.y] == null) {
				currentPiece.b1.x = currentPiece.b2.x + (int)delta.Real;
				currentPiece.b1.y = currentPiece.b2.y + (int)delta.Imaginary;
			} else if (	checkValid(currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary) &&
						checkValid(currentPiece.b1.x, currentPiece.b1.y) &&
						grid[currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary]== null &&
						grid[currentPiece.b2.x - (int)delta.Real, currentPiece.b2.y - (int)delta.Imaginary] == null) {
				currentPiece.b1.x = currentPiece.b2.x;
				currentPiece.b1.y = currentPiece.b2.y;
				currentPiece.b2.x -= (int)delta.Real;
				currentPiece.b2.y -= (int)delta.Imaginary;
			}
		}

		public bool checkValid(int x, int y) {
			return (x >= 0 && x < xSize && y >= 0 && y < ySize) ;
		}

		public void lockPiece() {
			grid[currentPiece.b1.x, currentPiece.b1.y] = currentPiece.b1;
			grid[currentPiece.b2.x, currentPiece.b2.y] = currentPiece.b2;
			update();
			if (grid[xSize/2, 0] == null && grid[xSize/2, 1] ==  null) {
				currentPiece = nextPiece; 
				nextPiece = new Piece(xSize, ySize);
			} else {
				// game over; 
			}
		}

		public bool dropBlocks() {
			bool changed = false;
			for (int i = 0; i < xSize; i++) {
				for (int j = ySize-2; j >= 0; j--) {
					if (grid[i, j] != null) {
						int k = 0;
						while (j+k < ySize-1 && grid[i, j + k + 1] == null) { k++; }
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

		private ArrayList toRemove;
		private ArrayList connected;

		public void clearBlocks() {
			toRemove = new ArrayList();
			for (int i = 0; i < xSize; i++) {
				for (int j = 0; j < ySize; j++) {
					if (grid[i, j] != null && grid[i, j].type == BlockType.Clear) {
						connected = new ArrayList();
						clearConnected(grid[i, j]);
					}
				}
			}
			foreach (Block b in toRemove) {
				grid[b.x, b.y] = null;
			}
		}
		
		public void clearConnected(Block b) {
			connected.Add(b);
			foreach (int[] v in Piece.directionVectors) {
				if (checkValid(b.x + v[0], b.y + v[1]) &&
					grid[b.x + v[0], b.y + v[1]] != null &&
					grid[b.x + v[0], b.y + v[1]].color == b.color &&
					(grid[b.x + v[0], b.y + v[1]].type == BlockType.Normal || grid[b.x + v[0], b.y + v[1]].type == BlockType.Clear) &&
					!connected.Contains(grid[b.x + v[0], b.y + v[1]])) {
					clearConnected(grid[b.x + v[0], b.y + v[1]]);		
				}
			}
			if (connected.Count > 1) {
				foreach (Block bl in connected) {
					toRemove.Add(bl);
				}
			}
		}

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