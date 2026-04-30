using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Monsters;

namespace CustomMonsters
{
	public partial class ModEntry : Mod
    {
        public static string ChangeSpawnSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.SpawnSound == null) ? value : data.SpawnSound;
        }
        public static string ChangeDamageSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DamageSound == null) ? value : (value == "rockGolemHit" && m is Bat ? data.DeathSound ?? value : data.DamageSound);
        }
        public static string ChangeDamageSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DamageSound2 == null) ? value : data.DamageSound2;
        }
        public static string ChangeDeathSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DeathSound == null) ? value : data.DeathSound;
        }
        public static string ChangeDeathSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DeathSound2 == null) ? value : data.DeathSound2;
        }
        public static string ChangeMoveSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.MoveSound == null) ? value : data.MoveSound;
        }
        public static string ChangeCrumbleSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.CrumbleSound == null) ? value : data.CrumbleSound;
        }
        public static string ChangeUncrumbleSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.UncrumbleSound == null) ? value : data.UncrumbleSound;
        }
        public static string ChangeHitSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ShellSound == null) ? value : data.ShellSound;
        }
        public static string ChangeBreakSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.BreakSound == null) ? value : data.BreakSound;
        }
        public static int ChangeReviveTimer(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ReviveTimer == null) ? value : data.ReviveTimer.Value;
        }
        public static string ChangeSpritePath(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.Sprite == null) ? value : data.Sprite;
        }
        public static string ChangeMoveSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.MoveSound2 == null) ? value : data.MoveSound2;
        }
        public static string ChangeProjectileSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileSound == null) ? value : data.ProjectileSound;
        }
        public static string ChangeProjectileSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileSound2 == null) ? value : data.ProjectileSound2;
        }
        public static string ChangeProjectileDebuff(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileDebuff == null) ? value : data.ProjectileDebuff;
        }
        public static int ChangeProjectileDamage(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileDamage == null) ? value : data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileCount(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileCount == null) ? value : data.ProjectileCount.Value;
        }
        public static float ChangeProjectileTimer(float value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileTimer == null) ? value : data.ProjectileTimer.Value;
        }
        public static int ChangeProjectileDamage2(int value, DinoMonster.BreathProjectile bp)
        {
            return (!Config.ModEnabled || bp is not CustomBreathProjectile cbp|| cbp.data.ProjectileDamage == null) ? value : cbp.data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileIndex(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileIndex == null) ? value : data.ProjectileIndex.Value;
        }
        public static string ChangeArmorSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ArmorSound == null) ? value : data.ArmorSound;
        }
        public static Color ChangeSprinkleColor(Color value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.SprinkleColor == null) ? value : MakeColor(data.SprinkleColor);
        }
        public static int ChangeLightType(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.LightType == -1) ? value : data.LightType;
        }
        public static Color MakeColor(string value)
        {
            return new Color(
                int.Parse(value.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(value.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(value.Substring(5, 2), System.Globalization.NumberStyles.HexNumber)
            );
        }
    }
}
