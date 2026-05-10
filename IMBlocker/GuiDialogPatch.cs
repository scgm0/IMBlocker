using HarmonyLib;
using Vintagestory.API.Client;

namespace IMBlocker;

[HarmonyPatch(typeof(GuiDialog))]
public static class GuiDialogPatch {
	[HarmonyPatch(nameof(GuiDialog.UnFocus))]
	[HarmonyPostfix]
	public static void UnFocus(GuiDialog __instance) {
		foreach (var (_, composer) in __instance.Composers) {
			composer.UnfocusOwnElements();
		}
	}
}