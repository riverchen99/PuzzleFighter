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
		public Piece(Block b1, Block b2) {
			this.b1 = b1;
			this.b2 = b2;
		}
		public Piece() {
			this.b1 = new Block(3, 0);
			this.b2 = new Block(3, 1);
		}
		public override String ToString() {
			return "block 1: " + b1.x + ", " + b1.y + ", " + b1.color + ", " + b1.type + "\n" + 
				"block 2: " + b2.x + ", " + b2.y + ", " + b2.color + ", " + b2.type;
		}
		public void move(Direction d) {
			switch (d) {
				case Direction.Down:
					b1.y++;
					b2.y++;
					break;
				case Direction.Left:
					b1.x--;
					b2.x--;
					break;
				case Direction.Right:
					b1.x++;
					b2.x++;
					break;
				default:
					break;
			}
		}
	}
}
