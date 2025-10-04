using Celeste.Mod.TypoHelper.Triggers;


namespace Celeste.Mod.TypoHelper
{


    public static class TriggerHookLoader
    {


        public static void Load()
        {
            PlayerNoWind.Load();
        }
        

        public static void Unload()
        {
            PlayerNoWind.Unload();
        }
    }
}