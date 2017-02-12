using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleFighter {
	public class Board {
		public Block[,] grid { get; set; }
		public Piece currentPiece { get; set; }
		public int xSize { get; set; }
		public int ySize { get; set; }
		
		public Board(int xSize, int ySize) { // x: 0 -> 5, y: 0 - 14 (two hidden)
			this.xSize = xSize;
			this.ySize = ySize;
			grid = new Block[xSize, ySize];
			currentPiece = new Piece();
		}

		public void update() {
			;
		}

		public void moveCurrent(Piece.Direction d) {
			try {
				if (grid[currentPiece.b1.x + Piece.directionVectors[(int)d, 0], currentPiece.b1.y + Piece.directionVectors[(int)d, 1]] == null &&
					grid[currentPiece.b2.x + Piece.directionVectors[(int)d, 0], currentPiece.b2.y + Piece.directionVectors[(int)d, 1]] == null) {
					currentPiece.move(d);
				} else if (d == Piece.Direction.Down) { lockPiece(); }
			} catch {
				if (d == Piece.Direction.Down) { lockPiece(); }
			}
		}

		public void rotateCurrent() {

		}

		public void lockPiece() {
			grid[currentPiece.b1.x, currentPiece.b1.y] = currentPiece.b1;
			grid[currentPiece.b2.x, currentPiece.b2.y] = currentPiece.b2;
			// clear blocks
			dropBlocks();
			if (grid[xSize/2, 0] == null && grid[xSize/2, 1] ==  null) {
				currentPiece = new Piece(xSize, ySize);
			} else { // game over 
			}
		}

		public void dropBlocks() {
			for (int i = 0; i < xSize; i++) {
				for (int j = ySize-2; j >= 0; j--) {
					if (grid[i, j] != null) {
						int k = 0;
						while (j+k < ySize-1 && grid[i, j + k + 1] == null) { k++; }
						if (k > 0) {
							grid[i, j].y = j + k;
							grid[i, j + k] = grid[i, j];
							grid[i, j] = null;
						}
					}
				}
			}
		}

		public void clearBlocks() {
			// do things
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
