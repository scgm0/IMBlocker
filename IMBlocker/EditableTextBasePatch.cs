using HarmonyLib;
using Vintagestory.API.Client;

namespace IMBlocker;

[HarmonyPatch(typeof(GuiElementEditableTextBase))]
public static class EditableTextBasePatch {
	[HarmonyPatch(nameof(GuiElementEditableTextBase.OnFocusGained))]
	[HarmonyPostfix]
	public static void FocusGained(GuiElementEditableTextBase __instance) {
		IMBlockerModSystem.StateManager?.OnTextFocusGained(__instance);
	}

	[HarmonyPatch(nameof(GuiElementEditableTextBase.OnFocusLost))]
	[HarmonyPostfix]
	public static void FocusLost(GuiElementEditableTextBase __instance) {
		IMBlockerModSystem.StateManager?.OnTextFocusLost(__instance);
	}
}