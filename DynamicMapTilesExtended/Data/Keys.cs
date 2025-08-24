using HarmonyLib;
using System.Reflection;

namespace DMT.Data
{
    public static class Keys
    {
        public const string AddLayerKey = "DMT/addLayer";
        public const string AddTilesheetKey = "DMT/addTilesheet";
        public const string ChangeIndexKey = "DMT/changeIndex";
        public const string ChangeMultipleIndexKey = "DMT/changeMultipleIndex";
        public const string ChangePropertiesKey = "DMT/changeProperties";
        public const string ChangeMultiplePropertiesKey = "DMT/changeMultipleProperties";
        public const string ExplodeKey = "DMT/explode";
        public const string ExplosionKey = "DMT/explosion";
        public const string PushKey = "DMT/push";
        public const string PushableKey = "DMT/pushable";
        public const string PushAlsoKey = "DMT/pushAlso";
        public const string PushOthersKey = "DMT/pushOthers";
        public const string SoundKey = "DMT/sound";
        public const string TeleportKey = "DMT/teleport";
        public const string TeleportTileKey = "DMT/teleportTile";
        public const string GiveKey = "DMT/give";
        public const string TakeKey = "DMT/take";
        public const string ChestKey = "DMT/chest";
        public const string MessageKey = "DMT/message";
        public const string EventKey = "DMT/event";
        public const string MailKey = "DMT/mail";
        public const string MailRemoveKey = "DMT/mailRemove";
        public const string MailBoxKey = "DMT/mailbox";
        public const string InvalidateKey = "DMT/invalidate";
        public const string MusicKey = "DMT/music";
        public const string HealthKey = "DMT/health";
        public const string StaminaKey = "DMT/stamina";
        public const string HealthPerSecondKey = "DMT/healthPerSecond";
        public const string StaminaPerSecondKey = "DMT/staminaPerSecond";
        public const string HealthPerSecondContKey = "DMT/healthPerSecondContinuous";
        public const string StaminaPerSecondContKey = "DMT/staminaPerSecondContinuous";
        public const string BuffKey = "DMT/buff";
        public const string SpeedKey = "DMT/speed";
        public const string MoveKey = "DMT/move";
        public const string EmoteKey = "DMT/emote";
        public const string AnimationKey = "DMT/animation";
        public const string AnimationOffKey = "DMT/animationOff";
        public const string SlipperyKey = "DMT/slippery";
        public const string WarpKey = "DMT/warp";
        public const string FriendsKey = "DMT/friends";

        private static List<string?> allKeys;

        public static List<string?> AllKeys
        {
            get
            {
                if (allKeys == null)
                {
                    allKeys = new();
                    foreach (var t in AccessTools.GetDeclaredFields(typeof(Keys)))
                    {
                        if(t.IsLiteral)
                            allKeys.Add((string?)typeof(Keys).GetField(t.Name)?.GetValue(null));
                    }
                }
                return allKeys;
            }
        }

        public static readonly HashSet<string> ModKeys = [];
    }
}
