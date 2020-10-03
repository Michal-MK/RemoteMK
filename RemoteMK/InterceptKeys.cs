using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteMK {
	public class InterceptKeys {
		private DLLImport.HookProc _proc;
		private static IntPtr _hookID = IntPtr.Zero;
		public Action<Keys, bool> HandleKey;

		public InterceptKeys() {
			Task.Run(() => {
				_proc = HookCallback;
				_hookID = SetHook(_proc);
				Application.Run();
				DLLImport.UnhookWindowsHookEx(_hookID);
			});
		}

		private IntPtr SetHook(DLLImport.HookProc proc) {
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule) {
				return DLLImport.SetWindowsHookEx(Constants.WH_KEYBOARD_LL, proc,
					DLLImport.GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if (nCode >= 0) {
				int vkCode = Marshal.ReadInt32(lParam);
				HandleKey?.Invoke((Keys)vkCode, wParam == (IntPtr)Constants.WM_KEYDOWN);
			}

			return DLLImport.CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}