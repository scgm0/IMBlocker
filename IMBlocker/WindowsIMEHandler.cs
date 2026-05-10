using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vintagestory.API.Client;

namespace IMBlocker;

public partial class WindowsIMEHandler : IIMEHandler {
	private ICoreClientAPI? _api;
	private IntPtr _hWnd;
	private bool _imeEnabled = true;

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmGetContext(IntPtr hWnd);

	[LibraryImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

	[LibraryImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmDestroyContext(IntPtr hIMC);

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmCreateContext();

	[LibraryImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

	[LibraryImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmGetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT {
		public int x;
		public int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT {
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct COMPOSITIONFORM {
		public uint dwStyle;
		public POINT ptCurrentPos;
		public RECT rcArea;
	}

	public const uint CFS_POINT = 0x0002;

	public unsafe void Initialize(ICoreClientAPI api) {
		_api = api;
		_hWnd = GLFW.GetWin32Window(api.Forms.Window.WindowPtr);
		if (_hWnd == IntPtr.Zero) {
			api.Logger.Error("无法获取窗口句柄");
		}
	}

	public void EnableIME() {
		try {
			if (_imeEnabled || _hWnd == IntPtr.Zero) {
				return;
			}

			var hImc = ImmAssociateContext(_hWnd, ImmCreateContext());
			if (hImc != IntPtr.Zero) {
				ImmDestroyContext(hImc);
			}
			_imeEnabled = true;
			// UpdateImeWindowPosition(_hWnd, 0, 0);
		} catch (Exception e) {
			_api?.Logger.Error(e.Message);
		}
	}

	public void DisableIME() {
		try {
			if (!_imeEnabled || _hWnd == IntPtr.Zero) {
				return;
			}

			var hImc = ImmAssociateContext(_hWnd, IntPtr.Zero);
			ImmDestroyContext(hImc);

			_imeEnabled = false;
		} catch (Exception e) {
			_api?.Logger.Error(e.Message);
		}
	}

	public static void UpdateImeWindowPosition(IntPtr hWnd, int x, int y) {
		var hImc = ImmGetContext(hWnd);
		if (hImc != IntPtr.Zero) {
			var form = new COMPOSITIONFORM();
			ImmGetCompositionWindow(hImc, ref form);
			form.dwStyle = CFS_POINT;
			form.ptCurrentPos = new() { x = x, y = y };

			ImmSetCompositionWindow(hImc, ref form);
		}
		ImmReleaseContext(hWnd, hImc);
	}
}