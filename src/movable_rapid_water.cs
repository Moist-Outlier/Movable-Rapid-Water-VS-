using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace MovableRapidWater
{
    public class HarmonyLoader : ModSystem
    {
        private const string HarmonyID = "MovableRapidWater";
        private Harmony harmony = new Harmony(HarmonyID);

        public override double ExecuteOrder() => 0.04;

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            //ApplyPatch(sapi);
            sapi.Logger.Notification("server start test");
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            ApplyPatch(capi);
            capi.Logger.Notification("client start test");
        }

        void ApplyPatch(ICoreAPI api)
        {
            System.Environment.SetEnvironmentVariable("HARMONY_LOG_FILE","/home/water/Documents/Modding/harmonylog.txt");
            HarmonyLib.FileLog.Log("MRW: " + DateTime.Now.ToString());

            if (!HarmonyLib.Harmony.HasAnyPatches(HarmonyID))
                harmony.PatchAll();
        }

        public override void Dispose()
        { harmony.UnpatchAll(HarmonyID); }
    }

    [HarmonyLib.HarmonyDebug]
    /*[HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerBase),"OnBlockInteractStart")]
    public class LiquidContainerPatch
    {
        public static void RapidCheck(IPlayer byPlayer, BlockSelection blockSel)
        {
            var block = byPlayer.Entity.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
            byPlayer.Entity.World.Api.Logger.Notification("MRW: function called!");
            if (block.Code.Path == "rapidwater-still-7")
                byPlayer.Entity.World.BlockAccessor.SetBlock(0, blockSel.Position);
        }

        [HarmonyLib.HarmonyTranspiler]
        public static IEnumerable<HarmonyLib.CodeInstruction> WaterTranspiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            HarmonyLib.CodeMatcher matcher = new HarmonyLib.CodeMatcher(instructions);
            matcher.MatchStartForward(new CodeMatch(i => i.Calls(AccessTools.Method(typeof(ItemStack),"TryTakeContent"))))
                .SetInstruction(new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2))
                .SetInstructionAndAdvance(new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(LiquidContainerPatch),nameof(LiquidContainerPatch.RapidCheck))));

            return matcher.InstructionEnumeration();
        }
    }*/

    [HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerTopOpened),"OnContainedInteractStart")]
    public static class BucketPatch
    {
        public static void RapidsCheck(IPlayer byPlayer, BlockSelection blockSel)
        {
            var block = byPlayer.Entity.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
            byPlayer.Entity.World.Api.Logger.Notification("MRW: function called!");
            if (block.Code.Path == "rapidwater-still-7")
                byPlayer.Entity.World.BlockAccessor.SetBlock(0, blockSel.Position);
        }

        [HarmonyLib.HarmonyTranspiler]
        public static IEnumerable<HarmonyLib.CodeInstruction> WaterTranspiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            HarmonyLib.CodeMatcher matcher = new HarmonyLib.CodeMatcher(instructions);
            /*matcher.MatchStartForward(new CodeMatch(i => i.Calls(AccessTools.Method(typeof(ItemStack),"TryTakeContent"))))
            .SetInstruction(new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2))
            .SetInstructionAndAdvance(new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(BucketPatch),nameof(BucketPatch.RapidsCheck))));*/

            return matcher.InstructionEnumeration();
        }
    }
}


