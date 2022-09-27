using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class Samurai
    {
        private static readonly int Id = 200000;
        public static List<byte> playerIdList = new();
        public static CustomOption KillCoolTime;
        public static CustomOption SwordCoolTime;
        public static CustomOption SwordScope;
        public static CustomOption CanUseVent;
        public static CustomOption CanUseSabo;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Samurai);
            CanUseVent = CustomOption.Create(Id + 10, TabGroup.ImpostorRoles, Color.white, "SamuraiCanUseVent", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
            CanUseSabo = CustomOption.Create(Id + 11, TabGroup.ImpostorRoles, Color.white, "SamuraiCanUseSabo", true, Options.CustomRoleSpawnChances[CustomRoles.Samurai]);
        }
    }
}