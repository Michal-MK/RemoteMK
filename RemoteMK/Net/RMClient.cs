using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using WindowsInput;

namespace RemoteMK.Net {
	public class RMClient {

		InputSimulator sim;

		public RMClient() {
			sim = new InputSimulator();
			StartClient();
		}

		private void StartClient() {
			UdpClient c = new UdpClient();
			c.Client.Bind(new IPEndPoint(IPAddress.Any, Constants.PORT));
			IPEndPoint ep = new IPEndPoint(0, 0);
			while (true) {
				byte[] rec = c.Receive(ref ep);
				switch (rec.Length) {
					case 8: {
						int posX = BitConverter.ToInt32(rec, 0);
						int posY = BitConverter.ToInt32(rec, 4);
						Cursor.Position = new Point(posX, posY);
						break;
					}
					case 5: {
						if (rec[0] == 1)
							sim.Keyboard.KeyDown((WindowsInput.Native.VirtualKeyCode)BitConverter.ToInt32(rec, 1));
						else
							sim.Keyboard.KeyUp((WindowsInput.Native.VirtualKeyCode)BitConverter.ToInt32(rec, 1));
						break;
					}
					case 2: {
						if (rec[0] == 1) {
							sendMouseClick(Cursor.Position);
						}
						else if (rec[1] == 1) {
							sendMouseRightclick(Cursor.Position);
						}
						break;
					}
				}
			}
		}

		static void sendMouseRightclick(Point p) {
			DLLImport.mouse_event(Constants.MOUSEEVENTF_RIGHTDOWN | Constants.MOUSEEVENTF_RIGHTUP, (uint)p.X, (uint)p.Y, 0, UIntPtr.Zero);
		}

		static void sendMouseClick(Point p) {
			DLLImport.mouse_event(Constants.MOUSEEVENTF_LEFTDOWN | Constants.MOUSEEVENTF_LEFTUP, (uint)p.X, (uint)p.Y, 0, UIntPtr.Zero);
		}

	}
}
