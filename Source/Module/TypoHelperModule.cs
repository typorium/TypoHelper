using System;


namespace Celeste.Mod.TypoHelper {
    public class TypoHelperModule : EverestModule {
        public static TypoHelperModule Instance { get; private set; }

        public override Type SettingsType => typeof(TypoHelperModuleSettings);
        public static TypoHelperModuleSettings Settings => (TypoHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(TypoHelperModuleSession);
        public static TypoHelperModuleSession Session => (TypoHelperModuleSession) Instance._Session;

        public override Type SaveDataType => typeof(TypoHelperModuleSaveData);
        public static TypoHelperModuleSaveData SaveData => (TypoHelperModuleSaveData) Instance._SaveData;

        public static bool EeveeHelper_Loaded = false;
        public static bool FlaglinesAndSuch_Loaded = false;

        public TypoHelperModule()
        {
            Instance = this;

            Logger.SetLogLevel(nameof(TypoHelperModule), LogLevel.Info);
        }

        public override void Load()
        {

            // Hooks
            EntityHookLoader.Load();
            TriggerHookLoader.Load();

            // Helpers
            EeveeHelper_Loaded = Everest.Loader.DependencyLoaded(new(){
                Name = "EeveeHelper",
                Version = new Version(1, 12, 2)
            });
            
            FlaglinesAndSuch_Loaded = Everest.Loader.DependencyLoaded(new(){
                Name = "FlaglinesAndSuch",
                Version = new Version(1, 6, 45)
            });
            
        }

        public override void Unload() {

            // Hooks
            EntityHookLoader.Unload();
            TriggerHookLoader.Unload();
        }
    }
}
