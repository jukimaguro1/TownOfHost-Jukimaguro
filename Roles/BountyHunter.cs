using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class BountyHunter
    {
        private static readonly int Id = 1000;
        public static List<byte> playerIdList = new();

        private static OptionItem TargetChangeTime;
        private static OptionItem SuccessKillCooldown;
        private static OptionItem FailureKillCooldown;
        private static OptionItem ShowTargetArrow;

        public static Dictionary<byte, PlayerControl> Targets = new();
        public static Dictionary<byte, float> ChangeTimer = new();

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.BountyHunter);
            TargetChangeTime = FloatOptionItem.Create(Id + 10, "BountyTargetChangeTime", new(10f, 900f, 2.5f), 60f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.BountyHunter])
                .SetValueFormat(OptionFormat.Seconds);
            SuccessKillCooldown = FloatOptionItem.Create(Id + 11, "BountySuccessKillCooldown", new(0f, 180f, 2.5f), 2.5f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.BountyHunter])
                .SetValueFormat(OptionFormat.Seconds);
            FailureKillCooldown = FloatOptionItem.Create(Id + 12, "BountyFailureKillCooldown", new(0f, 180f, 2.5f), 50f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.BountyHunter])
                .SetValueFormat(OptionFormat.Seconds);
            ShowTargetArrow = BooleanOptionItem.Create(Id + 13, "BountyShowTargetArrow", false, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.BountyHunter]);
        }
        public static void Init()
        {
            playerIdList = new();
            Targets = new();
            ChangeTimer = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);

            if (AmongUsClient.Instance.AmHost)
                ResetTarget(Utils.GetPlayerById(playerId));
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        private static void SendRPC(byte bountyId, byte targetId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBountyTarget, SendOption.Reliable, -1);
            writer.Write(bountyId);
            writer.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void ReceiveRPC(MessageReader reader)
        {
            byte bountyId = reader.ReadByte();
            byte targetId = reader.ReadByte();
            var target = Utils.GetPlayerById(targetId);
            if (target != null) Targets[bountyId] = target;
        }
        //public static void SetKillCooldown(byte id, float amount) => Main.AllPlayerKillCooldown[id] = amount;
        public static void ApplyGameOptions(GameOptionsData opt) => opt.RoleOptions.ShapeshifterCooldown = TargetChangeTime.GetFloat();

        public static void OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            if (GetTarget(killer) == target)
            {//ターゲットをキルした場合
                Logger.Info($"{killer?.Data?.PlayerName}:ターゲットをキル", "BountyHunter");
                Main.AllPlayerKillCooldown[killer.PlayerId] = SuccessKillCooldown.GetFloat();
                killer.CustomSyncSettings();//キルクール処理を同期
                ResetTarget(killer);
            }
            else
            {
                Logger.Info($"{killer?.Data?.PlayerName}:ターゲット以外をキル", "BountyHunter");
                Main.AllPlayerKillCooldown[killer.PlayerId] = FailureKillCooldown.GetFloat();
                killer.CustomSyncSettings();//キルクール処理を同期
            }
        }
        public static void OnReportDeadBody()
        {
            ChangeTimer.Clear();
        }
        public static void FixedUpdate(PlayerControl player)
        {
            if (!player.Is(CustomRoles.BountyHunter)) return; //以下、バウンティハンターのみ実行

            if (GameStates.IsInTask && ChangeTimer.ContainsKey(player.PlayerId))
            {
                if (!player.IsAlive())
                    ChangeTimer.Remove(player.PlayerId);
                else
                {
                    var target = GetTarget(player);
                    if (ChangeTimer[player.PlayerId] >= TargetChangeTime.GetFloat())//時間経過でターゲットをリセットする処理
                    {
                        ResetTarget(player);//ターゲットの選びなおし
                        Utils.NotifyRoles(SpecifySeer: player);
                    }
                    if (ChangeTimer[player.PlayerId] >= 0)
                        ChangeTimer[player.PlayerId] += Time.fixedDeltaTime;

                    //BountyHunterのターゲット更新
                    if (Main.PlayerStates[target.PlayerId].IsDead)
                    {
                        ResetTarget(player);
                        Logger.Info($"{player.GetNameWithRole()}のターゲットが無効だったため、ターゲットを更新しました", "BountyHunter");
                        Utils.NotifyRoles(SpecifySeer: player);
                    }
                }
            }
        }
        public static PlayerControl GetTarget(PlayerControl player)
        {
            if (player == null) return null;
            if (Targets == null) Targets = new();

            if (!Targets.TryGetValue(player.PlayerId, out var target))
                target = ResetTarget(player);
            return target;
        }
        public static PlayerControl ResetTarget(PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return null;

            ChangeTimer[player.PlayerId] = 0f;
            Logger.Info($"{player.GetNameWithRole()}:ターゲットリセット", "BountyHunter");
            player.RpcResetAbilityCooldown(); ;//タイマー（変身クールダウン）のリセットと

            List<PlayerControl> cTargets = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                // 死者/切断者/インポスターを除外
                if (!Main.PlayerStates[pc.PlayerId].IsDead && !pc.GetCustomRole().IsImpostor())
                    cTargets.Add(pc);
            }
            if (cTargets.Count >= 2 && Targets.TryGetValue(player.PlayerId, out var p)) cTargets.RemoveAll(x => x.PlayerId == p.PlayerId); //前回のターゲットは除外

            var rand = IRandom.Instance;
            if (cTargets.Count <= 0)
            {
                Logger.Error("ターゲットの指定に失敗しました:ターゲット候補が存在しません", "BountyHunter");
                return null;
            }
            var target = cTargets[rand.Next(0, cTargets.Count)];
            Targets[player.PlayerId] = target;
            Logger.Info($"{player.GetNameWithRole()}のターゲットを{target.GetNameWithRole()}に変更", "BountyHunter");

            //RPCによる同期
            SendRPC(player.PlayerId, target.PlayerId);
            return target;
        }
        public static void GetAbilityButtonText(HudManager __instance) => __instance.AbilityButton.OverrideText($"{GetString("BountyHunterChangeButtonText")}");
        public static void DisplayTarget(PlayerControl bounty, TMPro.TextMeshPro LowerInfoText)
        {
            var target = GetTarget(bounty);
            LowerInfoText.text = target == null ? "null" : $"{GetString("BountyCurrentTarget")}:{GetTarget(bounty).name}";
            LowerInfoText.enabled = target != null || DebugModeManager.IsDebugMode;
        }
        public static void AfterMeetingTasks()
        {
            foreach (var id in playerIdList)
            {
                if (!Main.PlayerStates[id].IsDead)
                {
                    Utils.GetPlayerById(id)?.RpcResetAbilityCooldown();
                    ChangeTimer[id] = 0f;
                }
            }
        }

        public static string UtilsGetTargetArrow(bool isMeeting, PlayerControl seer)
        {
            //ミーティング以外では矢印表示
            if (isMeeting) return "";
            string SelfSuffix = "";
            foreach (var arrow in Main.targetArrows)
            {
                var target = Utils.GetPlayerById(arrow.Key.Item2);
                bool BountyTarget = GetTarget(seer) == target;
                if (arrow.Key.Item1 == seer.PlayerId && !Main.PlayerStates[arrow.Key.Item2].IsDead && (target.GetCustomRole().IsImpostor() || BountyTarget))
                    SelfSuffix += BountyTarget ? Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), arrow.Value) : arrow.Value;
            }
            return SelfSuffix;
        }
        public static string PCGetTargetArrow(PlayerControl seer, PlayerControl target)
        {
            var update = false;
            string Suffix = "";
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                bool BountyTarget = GetTarget(target) == pc;
                bool foundCheck =
                    pc != target && (pc.GetCustomRole().IsImpostor() || BountyTarget);

                //発見対象じゃ無ければ次
                if (!foundCheck) continue;

                update = FixedUpdatePatch.CheckArrowUpdate(target, pc, update, pc.GetCustomRole().IsImpostor());
                var key = (target.PlayerId, pc.PlayerId);
                var arrow = Main.targetArrows[key];
                if (BountyTarget) arrow = Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), arrow);
                if (target.AmOwner)
                {
                    //MODなら矢印表示
                    Suffix += arrow;
                }
            }
            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
            {
                //更新があったら非Modに通知
                Utils.NotifyRoles(SpecifySeer: target);
            }
            return Suffix;
        }
    }
}