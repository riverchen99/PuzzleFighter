using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleFighter {
	public class PowerGem {
		public int x { get; set; }
		public int y { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public BlockColor color { get; set; }
		public PowerGem(int x, int y, BlockColor color) {
			this.x = x;
			this.y = y;
			this.width = 2;
			this.height = 2;
			this.color = color;
		}
	}
	public class PowerGemEqualityComparer : IEqualityComparer<PowerGem> {
		public bool Equals(PowerGem a, PowerGem b) {
			return a.x == b.x && a.y == b.y;
		}

		public int GetHashCode(PowerGem a) {
			return a.GetHashCode();
		}
	}
}
