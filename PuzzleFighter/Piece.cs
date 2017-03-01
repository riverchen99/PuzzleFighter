using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleFighter {
	public class Piece {
		public Block b1 { get; set; }
		public Block b2 { get; set; }
		public enum Direction { Up, Down, Left, Right }
		public enum rotateDirection { CW, CCW }
		public static int[][] directionVectors = new int[4][] { new int[2] { 0, -1 }, new int[2] { 0, 1 }, new int[2] { -1, 0 }, new int[2] { 1, 0 } };
		public Piece(Block b1, Block b2) {
			this.b1 = b1;
			this.b2 = b2;
		}
		public Piece(int x1, int y1, int x2, int y2) {
			this.b1 = new Block(x1, y1);
			this.b2 = new Block(x2, y2);
		}
		public Piece(int xSize, int ySize) {
			this.b1 = new Block(xSize / 2, 0);
			this.b2 = new Block(xSize / 2, 1);
		}
		public override String ToString() {
			return "block 1: " + b1.x + ", " + b1.y + ", " + b1.color + ", " + b1.type + "\n" +
				"block 2: " + b2.x + ", " + b2.y + ", " + b2.color + ", " + b2.type;
		}
		public void move(Direction d) {
			b1.x += directionVectors[(int)d][0];
			b1.y += directionVectors[(int)d][1];
			b2.x += directionVectors[(int)d][0];
			b2.y += directionVectors[(int)d][1];
		}
	}
}
