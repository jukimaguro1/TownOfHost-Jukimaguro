using System.Collections.Generic;
using Hazel;
using UnityEngine;
using InnerNet;

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
        public static List<byte> SwordedPlayer;
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
            IsSword = false;
            SwordedPlayer = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            SwordedPlayer.Add(playerId);
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void ApplyGameOptions(GameOptionsData opt) => opt.RoleOptions.ShapeshifterCooldown = SwordCooldown.GetFloat();
        public static void ApplyKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
        public static bool IsSword;
        public static void SamuraiKill()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.IsAlive() && pc.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                {
                    if (Getsword(PlayerControl.LocalPlayer, pc))
                    {
                        RPC.BySamuraiKillRPC(PlayerControl.LocalPlayer.PlayerId, pc.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BySamuraiKillRPC, SendOption.Reliable, -1);
                        Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        Writer.Write(pc.PlayerId);
                        IsSword = true;
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
        }
        public static bool Getsword(PlayerControl source, PlayerControl player)
        {
            Vector3 position = source.transform.position;
            Vector3 playerposition = player.transform.position;
            var r = SwordScope.GetFloat();
            if ((position.x + r >= playerposition.x) && (playerposition.x >= position.x - r))
            {
                if ((position.y + r >= playerposition.y) && (playerposition.y >= position.y - r))
                {
                    if ((position.z + r >= playerposition.z) && (playerposition.z >= position.z - r))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void Shapeshift(PlayerControl shapeshifter, PlayerControl player, bool shapeshifting)
        {
            if (!SwordedPlayer.Contains(player.PlayerId))
            {
                if (AmongUsClient.Instance.AmHost || !IsSword)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.IsAlive() && pc.PlayerId != player.PlayerId)
                        {
                            if (Getsword(player, pc))
                            {
                                UseSword();
                                SamuraiKill();
                            }
                        }
                    }
                }
                SwordedPlayer.Add(player.PlayerId);
                Utils.CustomSyncAllSettings();
                Utils.NotifyRoles();
            }
        }
        public static void UseSword()
        {
            IsSword = true;
        }
    }
}