using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace SmoothedController {
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }
		internal static Harmony harmony { get; private set; }

		[Init]
		public void Init(IPALogger logger, Config conf) {
			Instance = this;
			Log = logger;
			PluginConfig.Instance = conf.Generated<PluginConfig>();
		}

		[OnStart]
		async public void OnApplicationStart() {
			await MainMenuAwaiter.WaitForMainMenuAsync();
			MainMenuAwaiter.MainMenuInitializing += reinitializeSettings;
			reinitializeSettings();

			harmony = new Harmony("Kinsi55.BeatSaber.SmoothedController");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		private void reinitializeSettings() {
			BSMLSettings.Instance.AddSettingsMenu("Smooth Controller", "SmoothedController.UI.settings.bsml", PluginConfig.Instance);
			HarmonyPatches.Smoother.posSmoth = 20.5f - Mathf.Clamp(PluginConfig.Instance.PositionSmoothing, 0f, 20f);
			HarmonyPatches.Smoother.rotSmoth = 20.5f - Mathf.Clamp(PluginConfig.Instance.RotationSmoothing, 0f, 20f);
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchSelf();
		}
	}
}
