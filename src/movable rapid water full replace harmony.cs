//using HarmonyLib;
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

internal class Plugin
{
    private Harmony harmony;
    private Assembly executingAssembly = Assembly.GetExecutingAssembly();

    [Init]
    public Plugin(PluginMetadata pluginMetadata)
    {
        harmony = new Harmony(pluginMetadata.Id);
    }

    [OnStart]
    public void OnApplicationStart() => harmony.PatchAll(executingAssembly);

    [OnExit]
    public void OnApplicationQuit() => harmony.UnpatchSelf();
}

[HarmonyLib::HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerBase),"OnBlockInteractStart")]
//[HarmonyLib::HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerTopOpened),"OnContainedInteractStart")]
public class MovableRapidWater
{
    public static bool Prefix(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (!hotbarSlot.Empty && hotbarSlot.Itemstack.Collectible.Attributes?.IsTrue("handleLiquidContainerInteract") == true)
        {
            EnumHandHandling handling = EnumHandHandling.NotHandled;
            hotbarSlot.Itemstack.Collectible.OnHeldInteractStart(hotbarSlot, byPlayer.Entity, blockSel, null, true, ref handling);
            if (handling == EnumHandHandling.PreventDefault || handling == EnumHandHandling.PreventDefaultAction) __result = true;
        }

        if (hotbarSlot.Empty || !(hotbarSlot.Itemstack.Collectible is ILiquidInterface)) __result = base.OnBlockInteractStart(world, byPlayer, blockSel);


        CollectibleObject obj = hotbarSlot.Itemstack.Collectible;

        bool singleTake = byPlayer.WorldData.EntityControls.ShiftKey;
        bool singlePut = byPlayer.WorldData.EntityControls.CtrlKey;

        if (obj is ILiquidSource objLso && !singleTake)
        {
            if (!objLso.AllowHeldLiquidTransfer) __result = false;

            var contentStackToMove = objLso.GetContent(hotbarSlot.Itemstack);

            float litres = singlePut ? objLso.TransferSizeLitres : objLso.CapacityLitres;
            int moved = TryPutLiquid(blockSel.Position, contentStackToMove, litres);

            if (moved > 0)
            {
                SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (stack) =>
                {
                    objLso.TryTakeContent(stack, moved);
                    __result = moved;
                });
                DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);

                __result = true;
            }
        }


        if (obj is ILiquidSink objLsi && !singlePut)
        {
            if (!objLsi.AllowHeldLiquidTransfer) __result = false;

            ItemStack owncontentStack = GetContent(blockSel.Position);

            if (owncontentStack == null) __result = base.OnBlockInteractStart(world, byPlayer, blockSel);

            var liquidStackForParticles = owncontentStack.Clone();

            float litres = singleTake ? objLsi.TransferSizeLitres : objLsi.CapacityLitres;

            int moved = SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (stack) => objLsi.TryPutLiquid(stack, owncontentStack, litres));
            if (moved > 0)
            {
                var block = byEntity.World.BlockAccessor.GetBlock(blockSel.position, BlockLayersAccess.Default);

                if (block.Code.Path == "rapidwater-still-7" && hotbarSlot.Itemstack.Collectible.Code.Path == "woodbucket-empty")
                {
                    TryTakeContent(blockSel.Position, moved);
                    DoLiquidMovedEffects(byPlayer, liquidStackForParticles, moved, EnumLiquidDirection.Fill);
                    world.BlockAccessor.SetBlock(0, blockSel.Position);
                    __result = true;
                }
                else if (block.Code.Path == "rapidwater-still-7")
                { __result = false; }
                //"vanilla" code
                TryTakeContent(blockSel.Position, moved);
                DoLiquidMovedEffects(byPlayer, liquidStackForParticles, moved, EnumLiquidDirection.Fill);
                __result = true;
            }
        }

        __result =  base.OnBlockInteractStart(world, byPlayer, blockSel);
    }


    /*public bool Prefix(BlockEntityContainer be, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (hotbarSlot.Empty || hotbarSlot.Itemstack.Collectible is not ILiquidInterface heldObj) __result false;

        bool singleTake = byPlayer.WorldData.EntityControls.ShiftKey;
        bool singlePut = byPlayer.WorldData.EntityControls.CtrlKey;

        if (!singleTake && heldObj is ILiquidSource liquidSource && liquidSource.AllowHeldLiquidTransfer)
        {
            ItemStack contentStackToMove = liquidSource.GetContent(hotbarSlot.Itemstack);
            int moved = TryPutLiquid(slot.Itemstack, contentStackToMove, singlePut ? liquidSource.TransferSizeLitres : liquidSource.CapacityLitres);

            if (moved > 0)
            {
                SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, delegate (ItemStack stack)
                {
                    liquidSource.TryTakeContent(stack, moved);
                    __result moved;
                });
                DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);
                be.MarkDirty();
                __result true;
            }
        }

        if (!singlePut && heldObj is ILiquidSink liquidSink && liquidSink.AllowHeldLiquidTransfer)
        {
            if (GetContent(slot.Itemstack) is ItemStack owncontentStack)
            {
                var heldLiquidContainer = liquidSink as BlockLiquidContainerBase;
                float litres = singleTake ? liquidSink.TransferSizeLitres : liquidSink.CapacityLitres;

                int moved = heldLiquidContainer?.SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (ItemStack stack) => liquidSink.TryPutLiquid(stack, owncontentStack, litres)) ??
                liquidSink.TryPutLiquid(hotbarSlot.Itemstack, owncontentStack, litres);

                if (moved > 0)
                {
                    var block = byEntity.World.BlockAccessor.GetBlock(blockSel.position, BlockLayersAccess.Default);

                    if (block.Code.Path == "rapidwater-still-7" && hotbarSlot.Itemstack.Collectible.Code.Path == "woodbucket-empty")
                    {
                        TryTakeContent(blockSel.Position, moved);
                        DoLiquidMovedEffects(byPlayer, liquidStackForParticles, moved, EnumLiquidDirection.Fill);
                        world.BlockAccessor.SetBlock(0, blockSel.Position);
                        __result = true;
                    }
                    else if (block.Code.Path == "rapidwater-still-7")
                    { __result = false; }
                    TryTakeContent(slot.Itemstack, moved);
                    heldLiquidContainer?.DoLiquidMovedEffects(byPlayer, owncontentStack, moved, EnumLiquidDirection.Fill);
                    be.MarkDirty();
                    __result true;
                }
            }
        }

        __result false;
    }*/
}


