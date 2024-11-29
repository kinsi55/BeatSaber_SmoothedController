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
	static class Smoother {
		public static bool enabled = true;
		static internal float posSmooth = 1f;
		static internal float rotSmoth = 1f;

		public static bool isSaber = true;

		static readonly ConditionalWeakTable<VRController, wrapper> mapper
			= new ConditionalWeakTable<VRController, wrapper>();

		static void Postfix(VRController __instance) {
			if(__instance.gameObject.name[0] != 'C')
				return;

			if(!enabled || !PluginConfig.Instance.Enabled)
				return;

			var nodeType = __instance.node;

			if(nodeType != XRNode.LeftHand && nodeType != XRNode.RightHand)
				return;

			if(!mapper.TryGetValue(__instance, out var wrapperI))
				mapper.Add(__instance, wrapperI = new wrapper());

			var pos = __instance.transform.localPosition;
			var rot = __instance.transform.localRotation;

			var angDiff = Quaternion.Angle(wrapperI.smoothedRotation, rot);
			wrapperI.angleVelocitySnap = Math.Min(wrapperI.angleVelocitySnap + angDiff, 90f);

			var snapMulti = Mathf.Clamp(wrapperI.angleVelocitySnap / PluginConfig.Instance.SmallMovementThresholdAngle, 0.1f, 2.5f);

			if(wrapperI.angleVelocitySnap > 0.1) 
				wrapperI.angleVelocitySnap -= Math.Max(0.4f, wrapperI.angleVelocitySnap / 1.7f);

			wrapperI.smoothedPosition =
				posSmooth > 0f ?
				Vector3.Lerp(wrapperI.smoothedPosition, pos, posSmooth * Time.deltaTime * snapMulti) :
				pos;

			wrapperI.smoothedRotation =
				rotSmoth > 0f ?
				Quaternion.Lerp(wrapperI.smoothedRotation, rot, rotSmoth * Time.deltaTime * snapMulti) :
				rot;

#if DEBUG
			if(nodeType == XRNode.RightHand)
				Console.WriteLine("Smoothing {0}. Targetpos {1}\tSmoothedPos {2}\tposSmooth {3}\tsnapMulti {4}", __instance.gameObject.name, pos, wrapperI.smoothedPosition, posSmooth, snapMulti);

#endif
			__instance.transform.SetLocalPositionAndRotation(wrapperI.smoothedPosition, wrapperI.smoothedRotation);
		}
	}

	//[HarmonyPatch]
	//public static class Smoother_ {
	//	public static bool enabled = true;

	//	static Dictionary<XRNode, wrapper> idk 
	//		= new Dictionary<XRNode, wrapper>();

	//	static internal float posSmoth = 1f;
	//	static internal float rotSmoth = 1f;

	//	[HarmonyPatch(typeof(UnityXRHelper), nameof(UnityXRHelper.GetNodePose))]
	//	[HarmonyPatch(typeof(OculusVRHelper), nameof(OculusVRHelper.GetNodePose))]
	//	static void Postfix(IVRPlatformHelper __instance, XRNode nodeType, ref Vector3 pos, ref Quaternion rot) {
	//		if(!enabled || !PluginConfig.Instance.Enabled || SaberSmoothFilter.isSaber)
	//			return;

	//		if(nodeType != XRNode.LeftHand && nodeType != XRNode.RightHand)
	//			return;

	//		if(!idk.TryGetValue(nodeType, out var wrapperI))
	//			idk.Add(nodeType, wrapperI = new wrapper());

	//		var angDiff = Quaternion.Angle(wrapperI.smoothedRotation, rot);
	//		wrapperI.angleVelocitySnap = Math.Min(wrapperI.angleVelocitySnap + angDiff, 90f);

	//		var snapMulti = Mathf.Clamp(wrapperI.angleVelocitySnap / PluginConfig.Instance.SmallMovementThresholdAngle, 0.1f, 2.5f);

	//		if(wrapperI.angleVelocitySnap > 0.1) {
	//			wrapperI.angleVelocitySnap -= Math.Max(0.4f, wrapperI.angleVelocitySnap / 1.7f);
	//		}

	//		if(PluginConfig.Instance.PositionSmoothing > 0f) {
	//			wrapperI.smoothedPosition = Vector3.Lerp(wrapperI.smoothedPosition, pos, posSmoth * Time.deltaTime * snapMulti);
	//			pos = wrapperI.smoothedPosition;
	//		}

	//		if(PluginConfig.Instance.RotationSmoothing > 0f) {
	//			wrapperI.smoothedRotation = Quaternion.Lerp(wrapperI.smoothedRotation, rot, rotSmoth * Time.deltaTime * snapMulti);
	//			rot = wrapperI.smoothedRotation;
	//		}
	//	}
	//}
}
