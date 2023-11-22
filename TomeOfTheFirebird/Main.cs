﻿using HarmonyLib;
using Kingmaker;
using System;
using System.Linq;
using TabletopTweaks.Core.Utilities;
using TomeOfTheFirebird.ModLogic;
using UnityModManagerNet;

namespace TomeOfTheFirebird
{
    static class Main
    {

        public static bool IsInGame => Game.Instance.Player?.Party.Any() ?? false;

        public static bool Enabled;
        public static ModContextTomeOfTheFirebird TotFContext;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Enabled = true;

                Harmony harmony = new Harmony(modEntry.Info.Id);
                TotFContext = new ModContextTomeOfTheFirebird(modEntry);
                TotFContext.ModEntry.OnSaveGUI = OnSaveGUI;
               // TotFContext.ModEntry.OnGUI = UMMSettingsUI.OnGUI;

#if DEBUG
                   TotFContext.Debug = true;
                   TotFContext.Blueprints.Debug = true;
#endif

                harmony.PatchAll();
                
                PostPatchInitializer.Initialize(TotFContext);

                return true;
            }
            catch (Exception e)
            {
               Main.TotFContext.Logger.LogError(e, e.Message);
                return false;
            }
        }

        
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            TotFContext.SaveAllSettings();



        }

        public static void LogDebug(string msg)
        {
#if DEBUG
            Main.TotFContext.Logger.Log(msg);
#endif

        }

        
    }
}

