using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleFighter {
	class PowerGem {
		public int x { get; set; }
		public int y { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		public PowerGem(int x, int y) {
			this.x = x;
			this.y = y;
			this.width = 2;
			this.height = 2;
		}
	}
}
