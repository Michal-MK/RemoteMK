using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteMK.Net {
	public class RMServer {

		public bool sendMouse = false;
		public DLLImport.HookProc MouseHookProcedure;
		public UdpClient c;
		public IntPtr hHook = IntPtr.Zero;

		public RMServer() {
			InterceptKeys ic = new InterceptKeys();
			ic.HandleKey = (k, d) => {
				if (k == Keys.F1 && d) {
					sendMouse ^= true;
					return;
				}
				const int size = 5;
				byte[] send = new byte[size];
				send[0] = d ? (byte)1 : (byte)0;
				BitConverter.GetBytes((int)k).CopyTo(send, 1);
				c.Send(send, size, Constants.BROADCAST, Constants.PORT);
				Console.WriteLine($"{(d ? "Down" : "Up")} {k}");
			};
			if (hHook == IntPtr.Zero) {
				Task.Run(() => {
					MouseHookProcedure = new DLLImport.HookProc(HandleMouse);
					using (Process curProcess = Process.GetCurrentProcess())
					using (ProcessModule curModule = curProcess.MainModule) {
						hHook = DLLImport.SetWindowsHookEx(Constants.WH_MOUSE_LL, MouseHookProcedure,
							DLLImport.GetModuleHandle(curModule.ModuleName), 0);
					}
					Application.Run();
				});
			}
			_ = StartServer();
		}


		private async Task StartServer() {
			c = new UdpClient();
			c.Client.Bind(new IPEndPoint(IPAddress.Any, Constants.PORT));
			while (true) { //TODO
				await Task.Delay(2);
				int posX = Cursor.Position.X;
				int posY = Cursor.Position.Y;
				byte[] send = new byte[8];
				BitConverter.GetBytes(posX).CopyTo(send, 0);
				BitConverter.GetBytes(posY).CopyTo(send, 4);
				if (sendMouse) {
					c.Send(send, 8, Constants.BROADCAST, Constants.PORT);
				}
			}
		}

		public IntPtr HandleMouse(int nCode, IntPtr wParam, IntPtr lParam) {
			if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam) {
				c.Send(new byte[2] { 1, 0 }, 2, Constants.BROADCAST, Constants.PORT);
			}
			if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam) {
				c.Send(new byte[2] { 0, 0 }, 2, Constants.BROADCAST, Constants.PORT);
			}
			if (nCode >= 0 && MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam) {
				c.Send(new byte[2] { 0, 1 }, 2, Constants.BROADCAST, Constants.PORT);

			}
			if (nCode >= 0 && MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam) {
				c.Send(new byte[2] { 0, 0 }, 2, Constants.BROADCAST, Constants.PORT);
			}
			return DLLImport.CallNextHookEx(hHook, nCode, wParam, lParam);
		}


	}
	public partial class KeyboardInput {
		public event EventHandler<KBArgs> KeyBoardKeyPressed;

		private DLLImport.HookProc keyBoardDelegate;
		private IntPtr keyBoardHandle;
		private const Int32 WH_KEYBOARD_LL = 13;

		public KeyboardInput() {
			keyBoardDelegate = KeyboardHookDelegate;
			keyBoardHandle = DLLImport.SetWindowsHookEx(
				WH_KEYBOARD_LL, keyBoardDelegate, IntPtr.Zero, 0);
		}

		private IntPtr KeyboardHookDelegate(Int32 Code, IntPtr wParam, IntPtr lParam) {
			if (Code < 0) {
				return DLLImport.CallNextHookEx(
					keyBoardHandle, Code, wParam, lParam);
			}

			Console.WriteLine($"Code: {Code}, WP: {wParam}, LP: {lParam}");
			KeyBoardKeyPressed?.Invoke(this, new KBArgs { Code = Code, WParam = wParam, LParam = lParam });

			return DLLImport.CallNextHookEx(keyBoardHandle, Code, wParam, lParam);
		}
	}
}
