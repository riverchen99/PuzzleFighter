using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleFighter {
	public enum BlockColor { Red, Green, Blue, Yellow }
	public enum BlockType { Normal, Clear, Lock, Diamond }
	public class Block {
		public int x { get; set; }
		public int y { get; set; }
		public BlockColor color { get; set; }
		public BlockType type { get; set; }
		public int unlockTime { get; set; }
		public bool inPowerGem { get; set;  }
		public static Array colorValues = Enum.GetValues(typeof(BlockColor));
		public static Array typeValues = Enum.GetValues(typeof(BlockType));
		public static readonly Random random = new Random();
		public Block(int x, int y, BlockColor color, BlockType type, int unlockTime) {
			this.x = x;
			this.y = y;
			this.color = color;
			this.type = type;
			this.unlockTime = unlockTime;
			this.inPowerGem = false;
		}
		public Block(int x, int y) {
			this.x = x;
			this.y = y;
			this.color = (BlockColor)colorValues.GetValue(random.Next(colorValues.Length));
			this.type = random.NextDouble() < .75 ? BlockType.Normal : (random.NextDouble() < .75 ? BlockType.Clear : BlockType.Diamond);
			this.unlockTime = -1;
			this.inPowerGem = false;
		}
	}
}
