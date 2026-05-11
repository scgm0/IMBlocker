# 输入法屏蔽

自动根据当前输入上下文调整输入法状态，适配[ImGui](https://mods.vintagestory.at/imgui)

目前支持`Windows`与`Linux`系统

模组配置：`ModConfig/IMBlocker.json`
```
{
  "AutoSwitchIME": true, // 是否自动开启/关闭输入法，意义不明，可以直接不使用本模组
  "EnableDebugLog": false, // 开启调试日志，会在日志中记录更多信息
  "WindowsPreferredEnglish": false // 仅限Windows，开始游戏时将输入法语言切换为英语，语言状态会在开关输入法时保留
}
```

# IMBlocker

Automatically adjust the input method state based on the current input context, compatible with [ImGui](https://mods.vintagestory.at/imgui)

Currently supports `Windows` and `Linux` systems.

Module configuration: `ModConfig/IMBlocker.json`
```
{
  "AutoSwitchIME": true, // Whether to automatically enable/disable the input method, the purpose is unclear; you can simply choose not to use this module
  "EnableDebugLog": false, // Enable debug logging to record more information in the logs
  "WindowsPreferredEnglish": false // Windows only, switches the input method language to English when starting the game, the language state will be retained when toggling the input method
}
```

