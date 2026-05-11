using System;
using System.Diagnostics;
using Vintagestory.API.Client;

namespace IMBlocker;

public sealed class LinuxIMEHandler : IIMEHandler {
	private ICoreClientAPI? _api;
	private volatile bool _imeEnabled = true;
	private string? _defaultEngine, _rawKeyboardEngine;
	public LinuxIMEType IMEType { get; private set; } = LinuxIMEType.Unknown;

	public bool ImeEnabled => _imeEnabled;

	public void Initialize(ICoreClientAPI api) {
		_api = api;
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IBUS_ADDRESS"))) {
			IMEType = LinuxIMEType.Ibus;
			_rawKeyboardEngine = "xkb:us::eng";
			_defaultEngine = "libpinyin";
		} else if (!string.IsNullOrEmpty(Exec("pgrep", "-l fcitx5"))) {
			IMEType = LinuxIMEType.Fcitx5;
			_rawKeyboardEngine = "-c";
			_defaultEngine = "-o";
		}

		if (IMEType == LinuxIMEType.Unknown) {
			api.Logger.Error("[IMBlocker] 不支持的输入法");
		}
	}

	public void SyncState() {
		if (IMEType == LinuxIMEType.Unknown) {
			return;
		}

		var imeEnabled = _imeEnabled;
		try {
			switch (IMEType) {
				case LinuxIMEType.Ibus: {
					var output = Exec("ibus", "engine");
					if (!string.IsNullOrWhiteSpace(output)) {
						imeEnabled = output.Trim() != _rawKeyboardEngine;
					}

					break;
				}
				case LinuxIMEType.Fcitx5: {
					var output = Exec("fcitx5-remote", null);
					if (!string.IsNullOrWhiteSpace(output)) {
						imeEnabled = output.Trim() == "2";
					}

					break;
				}
				default: {
					return;
				}
			}
		} catch {
			// ignored
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
		if (_imeEnabled || _defaultEngine == null) {
			return;
		}

		SwitchTo(_defaultEngine);
		_imeEnabled = true;
	}

	public void DisableIME() {
		if (!_imeEnabled || _rawKeyboardEngine == null) {
			return;
		}

		SwitchTo(_rawKeyboardEngine);
		_imeEnabled = false;
	}

	private void SwitchTo(string? engine) {
		switch (IMEType) {
			case LinuxIMEType.Ibus: {
				Exec("ibus", $"engine {engine}");
				break;
			}
			case LinuxIMEType.Fcitx5: {
				Exec("fcitx5-remote", engine);
				break;
			}
			case LinuxIMEType.Unknown:
			default: {
				return;
			}
		}
	}

	private string? Exec(string file, string? args) {
		try {
			using var p = new Process();
			p.StartInfo = new(file, args ?? string.Empty) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			p.Start();
			var output = p.StandardOutput.ReadToEnd();
			p.WaitForExit(100);
			return output;
		} catch (Exception e) {
			_api?.Logger.Error(e.Message);
			return null;
		}
	}
}