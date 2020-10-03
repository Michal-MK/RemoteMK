using RemoteMK.Net;
using System;

namespace RemoteMK {
	public class Program {
		public static RMClient c;
		public static RMServer s;

		public static void Main(string[] args) {
			if (args.Length > 0 && args[0].StartsWith("-c")) {
				c = new RMClient();
			}
			if (args.Length > 0 && args[0].StartsWith("-s")) {
				s = new RMServer();

			}
			Console.ReadLine();
		}
	}
}