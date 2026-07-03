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
    public class MovableRapidWaterModSystem : ModSystem
    {
        private const string HarmonyID = "MovableRapidWater";
        private Harmony harmony = new Harmony(HarmonyID);

        public override double ExecuteOrder() => 0.04;

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            ApplyPatch(sapi);
            //sapi.Logger.Notification("server start test");
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            ApplyPatch(capi);
            //capi.Logger.Notification("client start test");
        }

        void ApplyPatch(ICoreAPI api)
        {
            System.Environment.SetEnvironmentVariable("HARMONY_LOG_FILE","/home/water/Documents/Modding/Vintage_Story/harmonylog.txt");
            HarmonyLib.FileLog.Log("MRW: " + DateTime.Now.ToString());

            if (!HarmonyLib.Harmony.HasAnyPatches(HarmonyID))
            {
                HarmonyLib.FileLog.Log("MRW: Applying patches.");
                harmony.PatchAll();
            }
        }

        public override void Dispose()
        { harmony.UnpatchAll(HarmonyID); }

        //[HarmonyLib.HarmonyDebug]
        [HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerBase),"OnBlockInteractStart")]
        public class LiquidContainerPatch
        {
            //for the transpiler
            public static void RapidsCheck(ref IPlayer byPlayer, ref BlockSelection blockSel)
            {
                var block = byPlayer.Entity.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
                byPlayer.Entity.World.Api.Logger.Notification("MRW: function called!");
                //if (block.Code.Path == "game:rapidwater-still-7")
                if (block.Code.Path.Contains("rapidwater-still"))
                    byPlayer.Entity.World.BlockAccessor.SetBlock(0, blockSel.Position);
            }

            public static void Prefix(ref IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
            {
                world.Api.Logger.Notification("Liquid Container Base reached.");
                HarmonyLib.FileLog.Log("Liquid container base reached.");
            }
            /*[HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> WaterTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions);
                matcher.MatchStartForward(new CodeMatch(i => i.Calls(AccessTools.Method(typeof(BlockLiquidContainerBase),nameof(BlockLiquidContainerBase.TryTakeContent),[typeof(BlockPos), typeof(int)]))));

                if (matcher.InstructionAt(1).opcode != System.Reflection.Emit.OpCodes.Pop)
                    throw new InvalidOperationException($"[MovableRapidWater] Expected pop directly after TryTakeContent(); got {matcher.InstructionAt(1).opcode}. Refusing to patch.");

                matcher.Advance(2); // After the pop we have a clean stack
                matcher.Insert
                (
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2), // IPlayer
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_3), // BlockSelection
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(LiquidContainerPatch),nameof(RapidsCheck)))
                );

                return matcher.InstructionEnumeration();
            }
            [HarmonyLib.HarmonyTranspiler]
            public static IEnumerable<HarmonyLib.CodeInstruction> WaterTranspiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
            {
                HarmonyLib.CodeMatcher matcher = new HarmonyLib.CodeMatcher(instructions);
                matcher.MatchStartForward(new CodeMatch(i => i.Calls(AccessTools.Method(typeof(ItemStack),"TryTakeContent"))))
                    .SetInstruction(new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2))
                    .SetInstructionAndAdvance(new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(LiquidContainerPatch),nameof(LiquidContainerPatch.RapidCheck))));

                return matcher.InstructionEnumeration();
            }*/
        }

        [HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerTopOpened),"OnContainedInteractStart")]
        public static class BucketPatch
        {
            //for the transpiler
            public static void RapidsCheck(IPlayer byPlayer, BlockSelection blockSel)
            {
                var block = byPlayer.Entity.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
                byPlayer.Entity.World.Api.Logger.Notification("MRW: function called!");
                //if (block.Code.Path == "game:rapidwater-still-7")
                if (block.Code.Path.Contains("rapidwater-still"))
                    byPlayer.Entity.World.BlockAccessor.SetBlock(0, blockSel.Position);
            }

            public static void Prefix(BlockEntityContainer be, ItemSlot slot, ref IPlayer byPlayer, BlockSelection blockSel)
            {
                byPlayer.Entity.World.Api.Logger.Notification("Liquid Container Top Opened reached.");
                HarmonyLib.FileLog.Log("Liquid container top opened reached.");
            }

            /*[HarmonyLib.HarmonyTranspiler]
            public static IEnumerable<HarmonyLib.CodeInstruction> WaterTranspiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
            {
                HarmonyLib.CodeMatcher matcher = new HarmonyLib.CodeMatcher(instructions);

                matcher.MatchStartForward(new CodeMatch(i => i.Calls(AccessTools.Method(typeof(BlockLiquidContainerBase),"TryTakeContent"))))
                .SetInstruction(new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_2))
                .SetInstructionAndAdvance(new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(BucketPatch),nameof(BucketPatch.RapidsCheck))));

                return matcher.InstructionEnumeration();
            }

            public static void Postfix(ref IPlayer byPlayer, ref BlockSelection blockSel)
            {
                byPlayer.Entity.World.Api.Logger.Notification("MRW: function called!");
                var block = byPlayer.Entity.World.BlockAccessor.GetBlock(blockSel.Position);
                if (block.LiquidCode == "game:rapidwater")
                    byPlayer.Entity.World.BlockAccessor.SetBlock(0, blockSel.Position);
            }*/
        }
    }
}


