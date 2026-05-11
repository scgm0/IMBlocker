using System;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace IMBlocker;

public sealed class IMEStateManager : IDisposable {
	public IIMEHandler Handler { get; }
	private readonly ICoreClientAPI _api;
	private readonly ModConfig _config;
	private int _focusCount = -2;
	private readonly CancellationTokenSource _cts = new();


	public bool ImGuiWantsIme {
		get;
		set {
			if (value == field) {
				return;
			}

			field = value;
			if (_config.EnableDebugLog) {
				_api.Logger.Debug($"[IMBlocker] ImGuiWantsIme → {value}");
			}

			Update();
		}
	}

	public IMEStateManager(IIMEHandler handler, ICoreClientAPI api, ModConfig config) {
		Handler = handler;
		_api = api;
		_config = config;
		handler.DisableIME();

		Task.Factory.StartNew(SyncState, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	private async Task SyncState() {
		while (!_cts.Token.IsCancellationRequested) {
			Handler.SyncState();
			try {
				await Task.Delay(2000, _cts.Token);
			} catch (TaskCanceledException) {
				break;
			}
		}
	}

	public void OnTextFocusGained(GuiElementEditableTextBase? guiElementEditableTextBase = null) {
		_focusCount++;
		if (_config.EnableDebugLog) {
			_api.Logger.Debug($"[IMBlocker] {guiElementEditableTextBase} Focus ++ → {_focusCount}");
		}

		Update();
	}

	public void OnTextFocusLost(GuiElementEditableTextBase? guiElementEditableTextBase = null) {
		_focusCount--;
		if (_config.EnableDebugLog) {
			_api.Logger.Debug($"[IMBlocker] {guiElementEditableTextBase} Focus -- → {_focusCount}");
		}

		Update();
	}

	public void OnMouseGrabbed() {
		if (_focusCount < 0) {
			return;
		}

		if (_config.EnableDebugLog) {
			_api.Logger.Debug("[IMBlocker] 鼠标锁定，重置焦点计数");
		}

		_focusCount = 0;

		Update();
	}

	public void Update() {
		var needOn = _focusCount > 0 || ImGuiWantsIme;
		if (needOn && !Handler.ImeEnabled) {
			Handler.EnableIME();
			if (_config.EnableDebugLog) {
				_api.Logger.Debug("[IMBlocker] 输入法开启");
			}
		} else if (!needOn && Handler.ImeEnabled) {
			Handler.DisableIME();
			if (_config.EnableDebugLog) {
				_api.Logger.Debug("[IMBlocker] IME OFF");
			}
		}
	}

	public void Dispose() {
		_cts.Cancel();
		_cts.Dispose();
	}
}