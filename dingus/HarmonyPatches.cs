using System.Reflection;
using HarmonyLib;

namespace dingus
{
    public class HarmonyPatches
    {
        public const   string  InstanceId = Constants.GUID;
        private static Harmony instance;

        public static bool IsPatched { get; private set; }

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                    instance = new Harmony(InstanceId);

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}