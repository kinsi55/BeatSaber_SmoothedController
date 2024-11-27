using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR;

namespace SmoothedController.HarmonyPatches {
	class wrapper {
		public Vector3 smoothedPosition = Vector3.zero;
		public Quaternion smoothedRotation = Quaternion.identity;
		public float angleVelocitySnap = 1f;
	}

	[HarmonyPatch(typeof(VRController), "Update")]
	static class SaberSmoothFilter {
		public static bool isSaber = true;

		static void Prefix(VRController __instance) {
			isSaber = __instance.gameObject.name[0] != 'C';
		}
	}

	[HarmonyPatch]
	public static class Smoother {
		public static bool enabled = true;

		static Dictionary<XRNode, wrapper> idk 
			= new Dictionary<XRNode, wrapper>();

		static IEnumerable<MethodBase> TargetMethods() {
			var oldUnity = AccessTools.Method("OpenVRHelper:GetNodePose");

			if(oldUnity != null) {
				yield return oldUnity;
			} else {
				yield return AccessTools.Method("UnityXRHelper:GetNodePose");
				yield return AccessTools.Method("OculusVRHelper:GetNodePose");
			}
		}
		public static float posSmoth;
		public static float rotSmoth;

		static void Postfix(IVRPlatformHelper __instance, XRNode nodeType, ref Vector3 pos, ref Quaternion rot) {
			if(!enabled || !PluginConfig.Instance.Enabled || SaberSmoothFilter.isSaber)
				return;

			if(nodeType != XRNode.LeftHand && nodeType != XRNode.RightHand)
				return;

			if(!idk.TryGetValue(nodeType, out var wrapperI))
				idk.Add(nodeType, wrapperI = new wrapper());

			var angDiff = Quaternion.Angle(wrapperI.smoothedRotation, rot);
			wrapperI.angleVelocitySnap = Math.Min(wrapperI.angleVelocitySnap + angDiff, 90f);

			var snapMulti = Mathf.Clamp(wrapperI.angleVelocitySnap / PluginConfig.Instance.SmallMovementThresholdAngle, 0.1f, 2.5f);

			if(wrapperI.angleVelocitySnap > 0.1) {
				wrapperI.angleVelocitySnap -= Math.Max(0.4f, wrapperI.angleVelocitySnap / 1.7f);
			}

			if(PluginConfig.Instance.PositionSmoothing > 0f) {
				wrapperI.smoothedPosition = Vector3.Lerp(wrapperI.smoothedPosition, pos, posSmoth * Time.deltaTime * snapMulti);
				pos = wrapperI.smoothedPosition;
			}

			if(PluginConfig.Instance.RotationSmoothing > 0f) {
				wrapperI.smoothedRotation = Quaternion.Lerp(wrapperI.smoothedRotation, rot, rotSmoth * Time.deltaTime * snapMulti);
				rot = wrapperI.smoothedRotation;
			}
		}
	}
}
