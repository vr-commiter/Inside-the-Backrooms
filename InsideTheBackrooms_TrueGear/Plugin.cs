using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MyTrueGear;
using System.Threading;

namespace InsideTheBackrooms_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        private static TrueGearMod _TrueGear = null;

        private static HeartbeatType heartbeatType;
        private static int playerID;
        private static bool isRadiation = false;
        private static float anxietyValue = 0;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            HarmonyLib.Harmony.CreateAndPatchAll(typeof(Plugin));
            _TrueGear = new TrueGearMod();
            _TrueGear.Play("HeartBeat");

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerInventory), "TargetOnItemPickup")]
        private static void PlayerInventory_TargetOnItemPickup_Postfix(PlayerInventory __instance)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("TargetOnItemPickup");
            _TrueGear.Play("BackSlotInputItem");

        }


        [HarmonyPostfix, HarmonyPatch(typeof(PlayerInventory), "DropItem")]
        private static void PlayerInventory_DropItem_Postfix(PlayerInventory __instance)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("DropItem");
            _TrueGear.Play("ChestSlotOutputItem");
        }


        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "ServerTakeDamage")]
        private static void PlayerStats_ServerTakeDamage_Postfix(PlayerStats __instance)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("ServerTakeDamage");
            _TrueGear.Play("PoisonDamage");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "Update")]
        private static void PlayerStats_Update_Postfix(PlayerStats __instance)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            if (__instance.Radiation > 25f && !isRadiation)
            {
                isRadiation = true;
                Log.LogInfo("--------------------------------------");
                Log.LogInfo("StartRadiation");
                _TrueGear.StartRadiation();
                Log.LogInfo(__instance.Radiation);
            }
            else if (__instance.Radiation <= 25f && isRadiation)
            {
                isRadiation = false;
                Log.LogInfo("--------------------------------------");
                Log.LogInfo("StopRadiation");
                _TrueGear.StopRadiation();
                Log.LogInfo(__instance.Radiation);
            }
            if (anxietyValue > __instance.anxiety + 20)
            {
                Log.LogInfo("--------------------------------------");
                Log.LogInfo("DecAnxiety");
                _TrueGear.Play("Eating");
                Log.LogInfo(anxietyValue);
                Log.LogInfo(__instance.anxiety);
            }
            anxietyValue = __instance.anxiety;

        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "ServerHeal")]
        private static void PlayerStats_ServerHeal_Postfix(PlayerStats __instance, float healthPoints)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("ServerHeal");
            _TrueGear.Play("Healing");
            Log.LogInfo(healthPoints);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "ServerEnableBoost")]
        private static void PlayerStats_ServerEnableBoost_Postfix(PlayerStats __instance)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("ServerEnableBoost");
        }

        private static bool canAddAnxiety = true;

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "ServerAddAnxiety")]
        private static void PlayerStats_ServerAddAnxiety_Postfix(PlayerStats __instance, float amount)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            if (!canAddAnxiety)
            {
                return;
            }
            canAddAnxiety = false;
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("ServerAddAnxiety");
            _TrueGear.Play("AddAnxiety");
            Log.LogInfo(amount);
            Timer addAnxietyTimer = new Timer(AddAnxietyTimerCallBack, null, 110, Timeout.Infinite);
        }

        private static void AddAnxietyTimerCallBack(object o)
        {
            canAddAnxiety = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HeartbeatAudio), "SetHeartbeat")]
        private static void HeartbeatAudio_SetHeartbeat_Postfix(HeartbeatAudio __instance, HeartbeatType heartbeat)
        {
            if (heartbeatType == heartbeat)
            {
                return;
            }
            heartbeatType = heartbeat;

            Log.LogInfo("--------------------------------------");
            Log.LogInfo("SetHeartbeat");
            Log.LogInfo(heartbeat);
            _TrueGear.StopNormalHeartBeat();
            _TrueGear.StopElevatedHeartBeat();
            _TrueGear.StopStressedHeartBeat();
            _TrueGear.StopTerrifiedHeartBeat();
            switch (heartbeat)
            {
                case HeartbeatType.NORMAL:
                    _TrueGear.StartNormalHeartBeat();
                    break;
                case HeartbeatType.ELEVATED:
                    _TrueGear.StartElevatedHeartBeat();
                    break;
                case HeartbeatType.STRESSED:
                    _TrueGear.StartStressedHeartBeat();
                    break;
                case HeartbeatType.TERRIFIED:
                    _TrueGear.StartTerrifiedHeartBeat();
                    break;
                case HeartbeatType.NONE:
                default:
                    _TrueGear.StopNormalHeartBeat();
                    _TrueGear.StopElevatedHeartBeat();
                    _TrueGear.StopStressedHeartBeat();
                    _TrueGear.StopTerrifiedHeartBeat();
                    break;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "KillPlayer")]
        private static void PlayerStats_KillPlayer_Postfix(PlayerStats __instance)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("PLayerDeath");
            Log.LogInfo("StopRadiation");
            _TrueGear.Play("PlayerDeath");
            _TrueGear.StopRadiation();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "OnDieStateChange")]
        private static void PlayerStats_OnDieStateChange_Postfix(PlayerStats __instance, bool o, bool n)
        {
            if (playerID != __instance.components.Info.Networkm_PlayerID)
            {
                return;
            }
            if (!n)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("OnDieStateChange");
            Log.LogInfo("StopRadiation");
            _TrueGear.Play("PlayerDeath");
            _TrueGear.StopRadiation();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerStats), "OnStartLocalPlayer")]
        private static void PlayerStats_OnStartLocalPlayer_Postfix(PlayerStats __instance)
        {
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("OnStartLocalPlayer");
            _TrueGear.Play("LevelStarted");
            isRadiation = false;
            playerID = __instance.components.Info.Networkm_PlayerID;
            Log.LogInfo(playerID);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(PlayerGear), "OnFlashlightUse")]
        private static void PlayerGear_OnFlashlightUse_Postfix(PlayerGear __instance, bool o, bool n)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("OnFlashlightUse");
            _TrueGear.Play("RightHandPickupItem");
            Log.LogInfo(o);
            Log.LogInfo(n);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerGear), "OnSuitEquip")]
        private static void PlayerGear_OnSuitEquip_Postfix(PlayerGear __instance, bool o, bool n)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("OnSuitEquip");
            Log.LogInfo(o);
            Log.LogInfo(n);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerGear), "OnToolEquiped")]
        private static void PlayerGear_OnToolEquiped_Postfix(PlayerGear __instance, PlayerGear.GearTool old, PlayerGear.GearTool newTool)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            if (newTool == PlayerGear.GearTool.NONE)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("OnToolEquiped");
            _TrueGear.Play("RightHandPickupItem");
            Log.LogInfo(old);
            Log.LogInfo(newTool);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerGear), "Target_DrinkAlmondWater")]
        private static void PlayerGear_Target_DrinkAlmondWater_Postfix(PlayerGear __instance)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("Target_DrinkAlmondWater");
            _TrueGear.Play("Drinking");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerGear), "Target_ConsumeJellyJar")]
        private static void PlayerGear_Target_ConsumeJellyJar_Postfix(PlayerGear __instance)
        {
            if (playerID != __instance.Components.Info.Networkm_PlayerID)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("Target_ConsumeJellyJar");
            _TrueGear.Play("Eating");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BasePlayerController), "TargetExitFromHider")]
        private static void BasePlayerController_TargetExitFromHider_Postfix(BasePlayerController __instance)
        {
            if (!__instance.isLocalPlayer)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("TargetExitFromHider");
            _TrueGear.Play("RightHandPickupItem");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BasePlayerController), "TargetUseHiderProp")]
        private static void BasePlayerController_TargetUseHiderProp_Postfix(BasePlayerController __instance)
        {
            if (!__instance.isLocalPlayer)
            {
                return;
            }
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("TargetUseHiderProp");
            _TrueGear.Play("RightHandPickupItem");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), "PlayMonsterDeathAnimation")]
        private static void PlayerController_PlayMonsterDeathAnimation_Postfix(PlayerController __instance)
        {
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("PlayMonsterDeathAnimation");
            _TrueGear.Play("MonsterDeath");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PauseMenuUI), "ExitBtn_Click")]
        private static void PauseMenuUI_ExitBtn_Click_Postfix(PauseMenuUI __instance)
        {
            Log.LogInfo("--------------------------------------");
            Log.LogInfo("Exit");
            _TrueGear.StopRadiation();
            _TrueGear.StopNormalHeartBeat();
            _TrueGear.StopElevatedHeartBeat();
            _TrueGear.StopStressedHeartBeat();
            _TrueGear.StopTerrifiedHeartBeat();
        }

    }
}
