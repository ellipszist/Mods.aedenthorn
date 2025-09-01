using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace BirthdayFriendship
{
    public partial class ModEntry
    {
        public class Billboard_GetBirthdays_Patch
        {
            public static void Postfix(Dictionary<int, List<NPC>> __result)
            {
                __result.Values.ToList().ForEach(npcList => npcList.RemoveAll(npc => !CheckBirthday(npc)));
            }
        }
        public class NPC_Birthday_Season_Patch
        {
            public static bool Prefix(NPC __instance, ref string __result)
            {
                if(Config.ModCheck && !CheckBirthday(__instance))
                {
                    __result = "foobar";
                    return false;
                }
                return true;
            }
        }
    }
}