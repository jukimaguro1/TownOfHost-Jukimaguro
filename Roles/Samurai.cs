using System.Collections.Generic;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Samurai
    {
        private static readonly int Id = 200000;
        public static List<byte> playerIdList = new();
        public static CustomOption KillCooldown;
        public static CustomOption SwordCooldown;
        public static CustomOption SwordScope;
        public static CustomOption CanUseVent;
        public static CustomOption CanUseSabo;
        public static Dictionary<byte, bool> Slashed = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Samurai);
            KillCooldown = CustomOption.Create(Id + 10, TabGroup.ImpostorRoles, Color.white, "KillCooldown", 40f, 0f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            SwordCooldown = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "SamuraiSwordCooldown", 50f, 0f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            SwordScope = CustomOption.Create(Id + 12, TabGroup.ImpostorRoles, Color.white, "SwordScope", 1f, 0f, 3f, 1f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            CanUseVent = CustomOption.Create(Id + 13, TabGroup.ImpostorRoles, Color.white, "CanVent", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            CanUseSabo = CustomOption.Create(Id + 14, TabGroup.ImpostorRoles, Color.white, "CanUseSabotage", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
        }
        public static void Init()
        {
            playerIdList = new();
            Slashed = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            Slashed.Add(playerId, false);
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void ApplyGameOptions(GameOptionsData opt, byte playerId)
        {
            opt.RoleOptions.ShapeshifterCooldown = SwordCooldown.GetFloat();
            opt.RoleOptions.ShapeshifterDuration = 1;
        }
        public static void ApplyKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
        public static void SamuraiKill()
        {
            foreach (var samurai in PlayerControl.AllPlayerControls)
            {
                if (!(samurai.Is(CustomRoles.Samurai) && samurai.IsAlive())) continue;
                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    if (samurai == target) continue;
                    if (Vector2.Distance(samurai.GetTruePosition(), target.GetTruePosition()) <= SwordScope.GetFloat())
                    {
                        samurai.RpcMurderPlayer(target);
                    }
                }

            }
        }
        public static void Shapeshift(PlayerControl shapeshifter, PlayerControl player, bool shapeshifting)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsAlive() && pc.PlayerId != player.PlayerId && !Slashed[shapeshifter.PlayerId])
                    {
                        SamuraiKill();
                        Slashed[shapeshifter.PlayerId] = true;
                    }
                }
            }
            Utils.CustomSyncAllSettings(); //シェイプシフトしたらクールダウンを設定しなおし
            Utils.NotifyRoles();
        }

        public static void GetAbilityButtonText(HudManager __instance) => __instance.AbilityButton.OverrideText($"{GetString("SlashButtonText")}");
    }
}