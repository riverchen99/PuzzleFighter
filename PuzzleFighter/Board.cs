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

		public void dropCurrent() {
			if (currentPiece.b1.y == currentPiece.b2.y &&
				currentPiece.b1.y + 1 < 14 &&
				grid[currentPiece.b1.x, currentPiece.b1.y + 1] == null &&
				grid[currentPiece.b2.x, currentPiece.b2.y + 1] == null) {
				currentPiece.move(Piece.Direction.Down);
			} else if (currentPiece.b1.y + 1 < 14 &&
				currentPiece.b1.y > currentPiece.b2.y &&
				grid[currentPiece.b1.x, currentPiece.b1.y + 1] == null) {
				currentPiece.move(Piece.Direction.Down);
			} else if (currentPiece.b2.y + 1 < 14 &&
				currentPiece.b2.y > currentPiece.b1.y &&
				grid[currentPiece.b2.x, currentPiece.b2.y + 1] == null) {
				currentPiece.move(Piece.Direction.Down);
			} else {
				lockPiece();
			}
		}

		public void lockPiece() {
			grid[currentPiece.b1.x, currentPiece.b1.y] = currentPiece.b1;
			grid[currentPiece.b2.x, currentPiece.b2.y] = currentPiece.b2;
			currentPiece = new Piece();
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
