using System.Collections.Generic;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Publisher
    {
        private static readonly int Id = 7000;
        public static List<byte> playerIdList = new();
        public static Dictionary<byte, byte> Killer = new();
        public static List<byte> Target = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Publisher);
        }
        public static void Init()
        {
            playerIdList = new();
            Killer = new();
            Target = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }
        public static bool IsEnable() => playerIdList.Count > 0;
        public static PlayerControl GetPublisherKiller(byte targetId)
        {
            var target = Utils.GetPlayerById(targetId);
            if (target == null) return null;
            Killer.TryGetValue(targetId, out var killerId);
            var killer = Utils.GetPlayerById(killerId);
            return killer;
        }
        public static void PublisherUseAbility(PlayerControl reporter, PlayerControl target)
        {
            if (target = null) return;

            new LateTask(() =>
            {
                if (!(target.Is(CustomRoles.Publisher) && target.Data.IsDead)) return;
                if (reporter == target) return;
                if (!target.Data.IsDead) return;
                var killer = GetPublisherKiller(target.PlayerId);

                //動作
                string publishermessage = string.Format(GetString("PublisherKiller"), killer.GetRealName(true));
                Utils.SendMessage($"{publishermessage}");
            }, 3f, "UsePublisherAbility");
        }
    }
}