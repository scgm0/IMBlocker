using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vintagestory.API.Client;

namespace IMBlocker;

public sealed partial class WindowsIMEHandler : IIMEHandler {
	private ICoreClientAPI? _api;
	private IntPtr _hWnd;
	private volatile bool _imeEnabled = true;
	public bool ImeEnabled => _imeEnabled;

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmGetContext(IntPtr hWnd);

	[LibraryImport("imm32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

	[LibraryImport("imm32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmDestroyContext(IntPtr hIMC);

	[LibraryImport("imm32.dll")]
	static private partial IntPtr ImmCreateContext();

	[LibraryImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static private partial bool ImmSetConversionStatus(IntPtr himc, int fdwConversion, int fdwSentence);

	public unsafe void Initialize(ICoreClientAPI api) {
		_api = api;
		_hWnd = GLFW.GetWin32Window(api.Forms.Window.WindowPtr);
		if (_hWnd == IntPtr.Zero) {
			api.Logger.Error("无法获取窗口句柄");
			return;
		}

		var hImc = ImmGetContext(_hWnd);
		if (hImc != IntPtr.Zero) {
			ImmSetConversionStatus(hImc, IMBlockerModSystem.Config.WindowsPreferredEnglish ? 0 : 1, 0);
		}

		ImmReleaseContext(_hWnd, hImc);
	}

	public void SyncState() {
		var imeEnabled = _imeEnabled;
		if (_hWnd != IntPtr.Zero) {
			var hImc = ImmGetContext(_hWnd);
			if (IMBlockerModSystem.Config.EnableDebugLog) {
				_api?.Logger.Debug($"[IMBlocker] 输入法上下文句柄 → {hImc} 现输入法状态 → {_imeEnabled}");
			}

			if (hImc != IntPtr.Zero) {
				imeEnabled = true;
				ImmReleaseContext(_hWnd, hImc);
			} else {
				imeEnabled = false;
			}
		}

		if (imeEnabled == _imeEnabled) {
			return;
		}

		_imeEnabled = imeEnabled;
		if (IMBlockerModSystem.Config.EnableDebugLog) {
			_api?.Logger.Debug($"[IMBlocker] 输入法状态同步 → {_imeEnabled}");
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
			if (hImc != IntPtr.Zero) {
				ImmDestroyContext(hImc);
			}

			_imeEnabled = false;
		} catch (Exception e) {
			_api?.Logger.Error(e.Message);
		}
	}
}