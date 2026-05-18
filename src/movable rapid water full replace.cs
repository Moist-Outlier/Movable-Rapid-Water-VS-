using System;
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

/*[HarmonyPatch(typeof(Vintagestory.GameContent.BlockLiquidContainerBase),"OnBlockInteractStart")]
public class MovableRapidWater
{
    public static bool Prefix(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (block.Code.Path == "rapidwater-still-7" && hotbarSlot.Itemstack.Collectible.Code.Path == "woodbucket-empty")
        {
            world.BlockAccessor.SetBlock(0, blockSel.Position);
            return true;
        }

        return false;
    }
}*/

public override bool Vintagestory.GameContent.BlockLiquidContainerBase::OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
{
    ItemSlot hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

    if (!hotbarSlot.Empty && hotbarSlot.Itemstack.Collectible.Attributes?.IsTrue("handleLiquidContainerInteract") == true)
    {
        EnumHandHandling handling = EnumHandHandling.NotHandled;
        hotbarSlot.Itemstack.Collectible.OnHeldInteractStart(hotbarSlot, byPlayer.Entity, blockSel, null, true, ref handling);
        if (handling == EnumHandHandling.PreventDefault || handling == EnumHandHandling.PreventDefaultAction) return true;
    }

    if (hotbarSlot.Empty || !(hotbarSlot.Itemstack.Collectible is ILiquidInterface)) return base.OnBlockInteractStart(world, byPlayer, blockSel);


    CollectibleObject obj = hotbarSlot.Itemstack.Collectible;

    bool singleTake = byPlayer.WorldData.EntityControls.ShiftKey;
    bool singlePut = byPlayer.WorldData.EntityControls.CtrlKey;

    if (obj is ILiquidSource objLso && !singleTake)
    {
        if (!objLso.AllowHeldLiquidTransfer) return false;

        var contentStackToMove = objLso.GetContent(hotbarSlot.Itemstack);

        float litres = singlePut ? objLso.TransferSizeLitres : objLso.CapacityLitres;
        int moved = TryPutLiquid(blockSel.Position, contentStackToMove, litres);

        if (moved > 0)
        {
            SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (stack) =>
            {
                objLso.TryTakeContent(stack, moved);
                return moved;
            });
            DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);

            return true;
        }

    }


    if (obj is ILiquidSink objLsi && !singlePut)
    {
        if (!objLsi.AllowHeldLiquidTransfer) return false;

        ItemStack owncontentStack = GetContent(blockSel.Position);

        if (owncontentStack == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

        var liquidStackForParticles = owncontentStack.Clone();

        float litres = singleTake ? objLsi.TransferSizeLitres : objLsi.CapacityLitres;

        int moved = SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (stack) => objLsi.TryPutLiquid(stack, owncontentStack, litres));
        if (moved > 0)
        {
            if (block.Code.Path == "rapidwater-still-7" && hotbarSlot.Itemstack.Collectible.Code.Path == "woodbucket-empty")
            { world.BlockAccessor.SetBlock(0, blockSel.Position); }
            else if (block.Code.Path == "rapidwater-still-7")
            { return false; }

            TryTakeContent(blockSel.Position, moved);
            DoLiquidMovedEffects(byPlayer, liquidStackForParticles, moved, EnumLiquidDirection.Fill);
            return true;
        }
    }

    return base.OnBlockInteractStart(world, byPlayer, blockSel);
}
