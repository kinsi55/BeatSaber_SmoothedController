using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.XR;

namespace SmoothedController.HarmonyPatches {
	class wrapper {
		public Vector3 smoothedPosition = Vector3.zero;
		public Quaternion smoothedRotation = Quaternion.identity;
		public float angleVelocitySnap = 1f;
	}

	[HarmonyPatch(typeof(VRController), "Update")]
	public static class Smoother {
		public static bool enabled = true;

		static Dictionary<XRNode, wrapper> idk = new Dictionary<XRNode, wrapper>();

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!ILUtil.CheckIL(instructions, new Dictionary<int, OpCode>() {
				{50, OpCodes.Ldarg_0},
				{51, OpCodes.Ldfld},
				{68, OpCodes.Ret}
			})) return instructions;

			ILUtil.InsertFn(50, ref instructions, AccessTools.Method(typeof(Smoother), nameof(YES)));

			return instructions;
		}

		static float posSmoth = 20f - Mathf.Clamp(PluginConfig.Instance.PositionSmoothing, 0f, 20f);
		static float rotSmoth = 20f - Mathf.Clamp(PluginConfig.Instance.RotationSmoothing, 0f, 20f);

		static VRController instance;
		static void YES() {
			if(instance == null || !enabled || !PluginConfig.Instance.Enabled)
				return;

			wrapper wrapperI = null;

			if(!idk.TryGetValue(instance.node, out wrapperI))
				idk.Add(instance.node, wrapperI = new wrapper());

			var angDiff = Quaternion.Angle(wrapperI.smoothedRotation, instance.transform.localRotation);
			wrapperI.angleVelocitySnap = Math.Min(wrapperI.angleVelocitySnap + angDiff, 90f);

			var snapMulti = Mathf.Clamp(wrapperI.angleVelocitySnap / PluginConfig.Instance.SmallMovementThresholdAngle, 0.1f, 2.5f);

			if(wrapperI.angleVelocitySnap > 0.1) {
				wrapperI.angleVelocitySnap -= Math.Max(0.4f, wrapperI.angleVelocitySnap / 1.7f);
			}

			if(PluginConfig.Instance.PositionSmoothing > 0f) {
				wrapperI.smoothedPosition = Vector3.Lerp(wrapperI.smoothedPosition, instance.transform.localPosition, posSmoth * Time.deltaTime * snapMulti);
				instance.transform.localPosition = wrapperI.smoothedPosition;
			}

			if(PluginConfig.Instance.RotationSmoothing > 0f) {
				wrapperI.smoothedRotation = Quaternion.Lerp(wrapperI.smoothedRotation, instance.transform.localRotation, rotSmoth * Time.deltaTime * snapMulti);
				instance.transform.localRotation = wrapperI.smoothedRotation;
			}
		}

		static void Prefix(VRController __instance, VRControllerTransformOffset ____transformOffset) {
			// Check if the VRController's gameObject name starts with "C" (Controller) so that sabers (LeftHand / RightHand) are not smoothed lmao
			if(__instance.gameObject.name[0] != 'C') {
				instance = null;
				return;
			}

			instance = __instance;
		}
	}
}
