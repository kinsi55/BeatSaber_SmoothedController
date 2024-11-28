using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using SmoothedController.HarmonyPatches;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SmoothedController {
	internal class PluginConfig {
		public static PluginConfig Instance { get; set; }
		public virtual bool Enabled { get; set; } = true;
		public virtual float PositionSmoothing { get; set; } = 3f;
		public virtual float RotationSmoothing { get; set; } = 12f;
		public virtual float SmallMovementThresholdAngle { get; set; } = 6f;

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload() {
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed() {
			Smoother.posSmoth = 20.5f - Mathf.Clamp(PositionSmoothing, 0f, 20f);
			Smoother.rotSmoth = 20.5f - Mathf.Clamp(RotationSmoothing, 0f, 20f);
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(PluginConfig other) {
			// This instance's members populated from other
		}
	}
}