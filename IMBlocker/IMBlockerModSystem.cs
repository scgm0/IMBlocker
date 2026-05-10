using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace IMBlocker;

public class IMBlockerModSystem : ModSystem {
	private readonly Harmony _harmony = new("imblocker");
	private IIMEHandler? _imeHandler;
	public static IMEStateManager? StateManager { get; private set; }

	public override void StartClientSide(ICoreClientAPI api) {
		ModConfig config;
		try {
			config = api.LoadModConfig<ModConfig>("IMBlocker.json") ?? new ModConfig();
			api.StoreModConfig(config, "IMBlocker.json");
		} catch (Exception e) {
			config = new();
			api.StoreModConfig(config, "IMBlocker.json");
			api.Logger.Error($"[IMBlocker] 加载配置失败: {e}");
			return;
		}

		if (!config.AutoSwitchIME) {
			api.Logger.Notification("[IMBlocker] 由配置禁用");
			return;
		}

		if (OperatingSystem.IsWindows()) {
			_imeHandler = new WindowsIMEHandler();
		} else if (OperatingSystem.IsLinux()) {
			_imeHandler = new LinuxIMEHandler();
		} else {
			api.Logger.Warning("[IMBlocker] 不支持的平台，已跳过输入法控制");
			return;
		}

		_imeHandler.Initialize(api);

		StateManager = new(_imeHandler, api, config);

		_harmony.PatchAllUncategorized();
		if (api.ModLoader.IsModEnabled("vsimgui")) {
			_harmony.PatchCategory("vsimgui");
			api.Logger.Notification("[IMBlocker] 已启用 vsimgui 兼容");
		}

		api.Logger.Notification("[IMBlocker] 初始化完成，聚焦文本输入将启用输入法");
	}

	public override void Dispose() { _harmony.UnpatchAll(); }
}