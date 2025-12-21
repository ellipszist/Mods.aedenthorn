using DMT.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static partial class Utils
    {
        public static bool DoTriggerActions(Farmer who, GameLocation location, Point tilePosition, List<(DynamicTileProperty prop, Tile tile)> properties)
        {
            List<string> triggered = new();

            foreach (var item in properties)
            {
                bool found = true;
                try
                {
                    var value = item.prop.Value;
                    var tile = item.tile;
                    switch (item.prop.Key)
                    {
                        case Keys.ActionKey:
                            Actions.DoAction(who, value);
                            break;
                        case Keys.AddLayerKey:
                            Actions.DoAddLayer(location, value);
                            break;
                        case Keys.AddQuestKey:
                            Actions.DoAddQuest(who, value);
                            break;
                        case Keys.AddTilesheetKey:
                            Actions.DoAddTileSheet(location, value);
                            break;
                        case Keys.AnimationKey:
                            Actions.DoAnimate(location, value, false);
                            break;
                        case Keys.AnimationOffKey:
                            Actions.DoAnimate(location, value, true);
                            break;
                        case Keys.BuffKey:
                            Actions.DoAddBuff(who, value);
                            break;
                        case Keys.ChangeIndexKey:
                            Actions.DoChangeIndex(location, value, tile, tilePosition);
                            break;
                        case Keys.ChangeMultipleIndexKey:
                            Actions.DoChangeMultipleIndexes(location, value, tile, tilePosition);
                            break;
                        case Keys.ChangePropertiesKey:
                            Actions.DoChangeProperties(value, tile);
                            break;
                        case Keys.ChangeMultiplePropertiesKey:
                            Actions.DoChangeMultipleProperties(location, value, tile);
                            break;
                        case Keys.ChestKey:
                            Actions.DoSpawnChest(location, value);
                            break;
                        case Keys.EmoteKey:
                            Actions.DoEmote(who, value);
                            break;
                        case Keys.EventKey:
                            Actions.DoPlayEvent(value);
                            break;
                        case Keys.ExplosionKey:
                            Actions.DoExplode(who, location, value, tilePosition);
                            break;
                        case Keys.GiveKey:
                            Actions.DoGive(who, value);
                            break;
                        case Keys.HealthKey:
                            Actions.DoUpdateHealth(who, value);
                            break;
                        case Keys.HealthPerSecondKey:
                            Actions.DoUpdateHealthPerSecond(who, value);
                            break;
                        case Keys.HealthPerSecondContKey:
                            Actions.DoUpdateHealthPerSecondCont(who, value);
                            break;
                        case Keys.InvalidateKey:
                            Actions.DoInvalidateAsset(value);
                            break;
                        case Keys.MailBoxKey:
                            Actions.DoAddMailForTomorrow(who, value);
                            break;
                        case Keys.MailKey:
                            Actions.DoAddMailflag(who, value);
                            break;
                        case Keys.MailRemoveKey:
                            Actions.DoRemoveMailflag(who, value);
                            break;
                        case Keys.MessageKey:
                            Actions.DoShowMessage(value);
                            break;
                        case Keys.MusicKey:
                            Actions.DoPlayMusic(value);
                            break;
                        case Keys.PushKey:
                            Actions.DoPushTiles(who, tile, tilePosition);
                            break;
                        case Keys.PushOthersKey:
                            Actions.DoPushOtherTiles(who, value, tile, tilePosition);
                            break;
                        case Keys.RemoveQuestKey:
                            Actions.DoRemoveQuest(who, value);
                            break;
                        case Keys.StaminaKey:
                            Actions.DoUpdateStamina(who, value);
                            break;
                        case Keys.StaminaPerSecondKey:
                            Actions.DoUpdateStaminaPerSecond(who, value);
                            break;
                        case Keys.StaminaPerSecondContKey:
                            Actions.DoUpdateStaminaPerSecondCont(who, value);
                            break;
                        case Keys.SoundKey:
                            Actions.DoPlaySound(location, value);
                            break;
                        case Keys.TakeKey:
                            Actions.DoTake(who, value);
                            break;
                        case Keys.TeleportKey:
                            Actions.DoTeleport(who, value);
                            break;
                        case Keys.TeleportTileKey:
                            Actions.DoTeleportTile(who, value);
                            break;
                        case Keys.WarpKey:
                            Actions.DoWarp(who, value);
                            break;
                        case Keys.FriendsKey:
                            Actions.DoFriendshipChange(who, value);
                            break;
                        default:
                            if (Keys.ModKeys.Contains(item.prop.key))
                            {
                                Actions.ModActions[item.prop.Key]?.Invoke(who, value, tile, tilePosition);
                            }
                            else
                                found = false;
                            break;
                    }
                    if (found)
                    {
                        triggered.Add(item.prop.key);
                        if (item.prop.Invalidate != "None" && item.prop.Invalidate != null)
                        {
                            if (item.prop.Invalidate.Equals(Invalidate.OnNewDay + ""))
                            {
                                InvalidateOnNewDay.Add(location);
                            }
                            else if (item.prop.Invalidate.Equals(Invalidate.OnLocationChanged + ""))
                            {
                                InvalidateOnLocationChanged.Add(location);
                            }
                            else if (item.prop.Invalidate.StartsWith(Invalidate.OnTimeChanged + ""))
                            {
                                int time = 10;
                                string timeString = item.prop.Invalidate.Substring((Invalidate.OnTimeChanged + "").Length);
                                if (!string.IsNullOrEmpty(timeString))
                                {
                                    int.TryParse(timeString, out time);
                                }
                                if (InvalidateOnTimeChanged.TryGetValue(location, out int oldTime))
                                {
                                    if (time < oldTime)
                                        InvalidateOnTimeChanged[location] = time;
                                }
                                else
                                {
                                    InvalidateOnTimeChanged[location] = time;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Monitor.Log($"Error while trying to run action {item.prop.Key}", LogLevel.Error);
                    context.Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.Message}", LogLevel.Error);
                }
            }
            if (triggered.Any())
            {
                //Context.Monitor.Log($"Triggered at {tilePosition} by {who?.displayName ?? "no one"}: {string.Join(',', triggered)}");
                return true;
            }
            return false;
        }

    }
}
