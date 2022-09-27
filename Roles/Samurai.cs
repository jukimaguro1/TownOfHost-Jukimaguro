using System.Collections.Generic;
using Hazel;
using UnityEngine;

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
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Samurai);
            KillCooldown = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "SamuraiKillCooldown", 40f, 0f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            SwordCooldown = CustomOption.Create(Id + 12, TabGroup.ImpostorRoles, Color.white, "SamuraiSwordCooldown", 50f, 0f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            SwordScope = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "SwordScope", 1f, 0f, 3f, 1f, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            CanUseVent = CustomOption.Create(Id + 10, TabGroup.ImpostorRoles, Color.white, "SamuraiCanUseVent", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            CanUseSabo = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "SamuraiCanUseSabo", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
        }
        public static void Init()
        {
            playerIdList = new();
        }
    }
}