using HarmonyLib;
using ImGuiNET;

namespace IMBlocker;

[HarmonyPatchCategory("vsimgui")]
[HarmonyPatch("ImGuiNET.ImGui", "Render")] 
public static class ImGuiCompat {
	[HarmonyPostfix]
	public static void Render() {
		IMBlockerModSystem.StateManager?.ImGuiWantsIme = ImGui.GetIO().WantTextInput;
	}
}