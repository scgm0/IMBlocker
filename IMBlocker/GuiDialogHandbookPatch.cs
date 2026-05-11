using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace IMBlocker;

[HarmonyPatch(typeof(GuiDialogHandbook))]
public static class GuiDialogHandbookPatch {
	[HarmonyPatch(nameof(GuiDialogHandbook.OpenDetailPageFor))]
	[HarmonyPostfix]
	public static void OpenDetailPageFor(GuiDialogHandbook __instance, bool __result, GuiComposer ___overviewGui) {
		if (__result) {
			var input = ___overviewGui.GetTextInput("searchField");
			if (input.HasFocus) {
				input.OnFocusLost();
			}
		}
	}
}