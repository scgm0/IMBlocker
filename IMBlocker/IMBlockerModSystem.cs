using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace IMBlocker;

public class IMBlockerModSystem : ModSystem {
	private readonly Harmony _harmony = new("imblocker");
	private IIMEHandler? _imeHandler;
	public static IMEStateManager? StateManager { get; private set; }
	public static ModConfig Config { get; private set; } = new();

	public override void StartClientSide(ICoreClientAPI api) {
		try {
			Config = api.LoadModConfig<ModConfig>("IMBlocker.json") ?? new ModConfig();
			api.StoreModConfig(Config, "IMBlocker.json");
		} catch (Exception e) {
			Config = new();
			api.StoreModConfig(Config, "IMBlocker.json");
			api.Logger.Error($"[IMBlocker] 加载配置失败: {e}");
			return;
		}

		if (!Config.AutoSwitchIME) {
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

		StateManager = new(_imeHandler, api, Config);

		_harmony.PatchAllUncategorized();
		if (api.ModLoader.IsModEnabled("vsimgui")) {
			_harmony.PatchCategory("vsimgui");
			api.Logger.Notification("[IMBlocker] 已启用 vsimgui 兼容");
		}

		api.Logger.Notification("[IMBlocker] 初始化完成，聚焦文本输入将启用输入法");
	}

	public override void Dispose() {
		StateManager?.Dispose();
		_harmony.UnpatchAll();
	}
}