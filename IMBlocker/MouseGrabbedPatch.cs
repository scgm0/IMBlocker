using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace IMBlocker;

[HarmonyPatch(typeof(ClientMain), nameof(ClientMain.MouseGrabbed))]
public static class MouseGrabbedPatch {
	static private bool _value;

	[HarmonyPatch(MethodType.Setter)]
	[HarmonyPostfix]
	public static void Postfix(bool value) {
		if (_value == value) {
			return;
		}

		_value = value;
		if (value) {
			IMBlockerModSystem.StateManager?.OnMouseGrabbed();
		}
	}
}