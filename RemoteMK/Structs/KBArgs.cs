using System;

namespace RemoteMK {
	public struct KBArgs {
		public int Code { get; set; }
		public IntPtr WParam { get; set; }
		public IntPtr LParam { get; set; }
	}
}
