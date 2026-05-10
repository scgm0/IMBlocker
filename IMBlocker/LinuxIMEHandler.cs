using System;
using System.Diagnostics;
using System.IO;
using Vintagestory.API.Client;

namespace IMBlocker;

public class LinuxIMEHandler : IIMEHandler {
	private ICoreClientAPI? _api;
	private bool _imeEnabled = true;
	private string? _defaultEngine, _rawKeyboardEngine;
	private bool _isIbus, _isFcitx5;

	public void Initialize(ICoreClientAPI api) {
		_api = api;
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IBUS_ADDRESS"))) {
			_isIbus = true;
			_rawKeyboardEngine = "xkb:us::eng";
			_defaultEngine = Exec("ibus", "engine")?.Trim();
		} else if (File.Exists("/usr/bin/fcitx5-remote")) {
			_isFcitx5 = true;
			_rawKeyboardEngine = "-c";
			_defaultEngine = "-o";
		}
	}

	public void EnableIME() {
		if (!_imeEnabled && _defaultEngine != null) {
			SwitchTo(_defaultEngine);
			_imeEnabled = true;
		}
	}

	public void DisableIME() {
		if (_imeEnabled && _rawKeyboardEngine != null) {
			SwitchTo(_rawKeyboardEngine);
			_imeEnabled = false;
		}
	}

	private void SwitchTo(string? engine) {
		if (_isIbus) {
			Exec("ibus", $"engine {engine}");
		} else if (_isFcitx5) {
			Exec("fcitx5-remote", engine);
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
			p.WaitForExit(200);
			return output;
		} catch (Exception e) {
			_api?.Logger.Error(e.Message);
			return null;
		}
	}
}