using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using SmoothedController.HarmonyPatches;
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
		public void OnApplicationStart() {
			BSMLSettings.instance.AddSettingsMenu("Smooth Controller", "SmoothedController.UI.settings.bsml", PluginConfig.Instance);

			var original = AccessTools.Method(typeof(VRController), "Update");
			harmony = new Harmony("Kinsi55.BeatSaber.SmoothedController");
			harmony.Patch(original, new HarmonyMethod(AccessTools.Method(typeof(Smoother), nameof(Smoother.Prefix))));
			if(CompareGameVersions(UnityGame.GameVersion, new AlmostVersion("1.29.4")) >= 0) {
				harmony.Patch(original, null, new HarmonyMethod(AccessTools.Method(typeof(Smoother), nameof(Smoother.Postfix))));
			} else {
				harmony.Patch(original, null, null, new HarmonyMethod(AccessTools.Method(typeof(Smoother), nameof(Smoother.Transpiler))));
			}
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchSelf();
		}

		static int CompareGameVersions(AlmostVersion a, AlmostVersion b) {
			string stringValue = a.StringValue;
			return stringValue == null
				? a.CompareTo(b)
				: new AlmostVersion(stringValue.Substring(0, stringValue.IndexOf('_'))).CompareTo(b);
		}
	}
}
