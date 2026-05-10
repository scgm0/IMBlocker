using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace IMBlocker;

[HarmonyPatch(typeof(ClientMain), nameof(ClientMain.MouseGrabbed))]
public static class MouseGrabbedPatch {

	[HarmonyPatch(MethodType.Setter)]
	[HarmonyPostfix]
	public static void Postfix(bool value) {
		if (value) {
			IMBlockerModSystem.StateManager?.OnMouseGrabbed();
		}
	}
}