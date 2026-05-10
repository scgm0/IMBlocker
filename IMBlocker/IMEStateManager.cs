using Vintagestory.API.Client;

namespace IMBlocker;

public class IMEStateManager {
	private readonly IIMEHandler _handler;
	private readonly ICoreClientAPI _api;
	private readonly ModConfig _config;
	private int _focusCount = -2;
	private bool _imeOn;

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
		_handler = handler;
		_api = api;
		_config = config;
		handler.DisableIME();
		_imeOn = false;
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
		if (_focusCount == 0) {
			return;
		}

		if (_config.EnableDebugLog) {
			_api.Logger.Debug("[IMBlocker] 鼠标锁定，重置焦点计数");
		}

		_focusCount = 0;

		Update();
	}

	private void Update() {
		var needOn = _focusCount > 0 || ImGuiWantsIme;
		if (needOn && !_imeOn) {
			_handler.EnableIME();
			_imeOn = true;
			if (_config.EnableDebugLog) {
				_api.Logger.Debug("[IMBlocker] 输入法开启");
			}
		} else if (!needOn && _imeOn) {
			_handler.DisableIME();
			_imeOn = false;
			if (_config.EnableDebugLog) {
				_api.Logger.Debug("[IMBlocker] IME OFF");
			}
		}
	}
}