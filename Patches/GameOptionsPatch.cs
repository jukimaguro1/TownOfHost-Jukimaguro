using HarmonyLib;
using AmongUs.GameOptions;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
    class ChanceChangePatch
    {
        public static void Postfix(RoleOptionSetting __instance)
        {
            bool forced = false;
            string DisableText = $" ({GetString("Disabled")})";
            if (__instance.Role.Role == RoleTypes.Scientist)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Scientist);
                if (CustomRoles.Doctor.IsEnable()) forced = true;
            }
            if (__instance.Role.Role == RoleTypes.Engineer)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Engineer);
                if (CustomRoles.Madmate.IsEnable() || CustomRoles.Terrorist.IsEnable()) forced = true;
            }
            if (__instance.Role.Role == RoleTypes.GuardianAngel)
            {
                //+-ボタン, 設定値, 詳細設定ボタンを非表示
                var tf = __instance.transform;
                tf.Find("Count Plus_TMP").gameObject.active
                    = tf.Find("Chance Minus_TMP").gameObject.active
                    = tf.Find("Chance Value_TMP").gameObject.active
                    = tf.Find("Chance Plus_TMP").gameObject.active
                    = tf.Find("More Options").gameObject.active
                    = false;

                if (!__instance.TitleText.text.Contains(DisableText))
                    __instance.TitleText.text += DisableText;
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.GuardianAngel);
            }
            if (__instance.Role.Role == RoleTypes.Shapeshifter)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Shapeshifter);
                if (CustomRoles.SerialKiller.IsEnable() || CustomRoles.Warlock.IsEnable() || CustomRoles.BountyHunter.IsEnable()/* || CustomRoles.ShapeMaster.IsEnable()*/) forced = true;
            }

            if (forced)
            {
                ((TMPro.TMP_Text)__instance.ChanceText).text = "Always";
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
    class SwitchGameModePatch
    {
        public static void Postfix(GameModes gameMode)
        {
            if (gameMode == GameModes.HideNSeek)
            {
                ErrorText.Instance.HnSFlag = true;
                ErrorText.Instance.AddError(ErrorCode.HnsUnload);
                Harmony.UnpatchAll();
                Main.Instance.Unload();
            }
        }
    }
}