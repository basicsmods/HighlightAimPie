using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection.Emit;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using static HarmonyLib.Code;
using static RimWorld.MechClusterSketch;

namespace HighlightAimPie
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("basics.highlightaimpie");
            harmony.PatchAll();
        }
    }

    [StaticConstructorOnStartup]
    public static class UIAssets
    {
        public static readonly Material AimPieMaterialPatch = SolidColorMaterials.SimpleSolidColorMaterial(new UnityEngine.Color(0f, 0.5f, 1f, 0.9f), false);
    }

    [HarmonyPatch(typeof(Stance_Warmup), nameof(Stance_Warmup.StanceDraw), new Type[] { })]
    public class Stance_Warmup_StanceDraw
    {
        static void Postfix(ref Stance_Warmup __instance)
        {
            if (__instance.drawAimPie && Find.Selector.IsSelected(__instance.stanceTracker.pawn))
            {
                // This is the check used in the vanilla method. We don't need to do anything special
                // if this is true since the vanilla method would have called the correct method anyways.
                return;
            }
            // These extra checks cause the AimPie to be drawn even when the shooter isn't selected 
            // IFF the shooter is hostile to the player AND the shooter's target is a friendly pawn / mech.
            // The AimPie won't be drawn if the target is another enemy (like from berserk) or the target
            // is a building (like a turret / wall / door).
            // I should maybe make it ignore friendly non-playerFaction targets as well since I don't really
            // care if the enemies are aiming at pawns who aren't part of my colony.
            if (__instance.drawAimPie && (__instance.stanceTracker.pawn.HostileTo(Faction.OfPlayer) &&
                __instance.focusTarg.Pawn != null && !__instance.focusTarg.Pawn.HostileTo(Faction.OfPlayer)))
            {
                // This is the same call that the default method makes when the default if block is taken.
                GenDraw.DrawAimPie(__instance.stanceTracker.pawn, __instance.focusTarg, (int)((float)__instance.ticksLeft * __instance.pieSizeFactor), 0.2f);
            }
        }
    }


    [HarmonyPatch(typeof(GenDraw), nameof(GenDraw.DrawAimPieRaw), new Type[] { typeof(Vector3), typeof(float), typeof(int) })]
    public class GenDraw_DrawAimPieRaw
    {
        /*static bool Prefix(Vector3 center, float facing, int degreesWide)
        {
            // Log.Message("$basics$ in gendraw_drawaimpieraw prefix.");
            if (degreesWide <= 0)
            {
                return false;
            }
            if (degreesWide > 360)
            {
                degreesWide = 360;
            }
            center += Quaternion.AngleAxis(facing, Vector3.up) * Vector3.forward * 0.8f;
            Material NewMaterial_basics = SolidColorMaterials.SimpleSolidColorMaterial(new UnityEngine.Color(0f, 0.5f, 1f, 0.9f), false);
            UnityEngine.Graphics.DrawMesh(MeshPool.pies[degreesWide], center, Quaternion.AngleAxis(facing + (float)(degreesWide / 2) - 90f, Vector3.up), NewMaterial_basics, 0);
            return false;
        }*/

        /*
        ldarg.2 NULL
        ldc.i4.0 NULL
        bgt.s Label0
        ret NULL
        ldarg.2 NULL [Label0]
        ldc.i4 360
        ble.s Label1
        ldc.i4 360
        starg.s 2
        ldarg.0 NULL [Label1]
        ldarg.1 NULL
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_up()
        call static UnityEngine.Quaternion UnityEngine.Quaternion::AngleAxis(System.Single angle, UnityEngine.Vector3 axis)
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_forward()
        call static UnityEngine.Vector3 UnityEngine.Quaternion::op_Multiply(UnityEngine.Quaternion rotation, UnityEngine.Vector3 point)
        ldc.r4 0.8
        call static UnityEngine.Vector3 UnityEngine.Vector3::op_Multiply(UnityEngine.Vector3 a, System.Single d)
        call static UnityEngine.Vector3 UnityEngine.Vector3::op_Addition(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        starg.s 0
        ldsfld UnityEngine.Mesh[] Verse.MeshPool::pies
        ldarg.2 NULL
        ldelem.ref NULL
        ldarg.0 NULL
        ldarg.1 NULL
        ldarg.2 NULL
        ldc.i4.2 NULL
        div NULL
        conv.r4 NULL
        add NULL
        ldc.r4 90
        sub NULL
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_up()
        call static UnityEngine.Quaternion UnityEngine.Quaternion::AngleAxis(System.Single angle, UnityEngine.Vector3 axis)
        ldsfld UnityEngine.Material Verse.GenDraw::AimPieMaterial
        ldc.i4.0 NULL
        call static System.Void UnityEngine.Graphics::DrawMesh(UnityEngine.Mesh mesh, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Material material, System.Int32 layer)
        ret NULL

        to

        ldarg.2 NULL
        ldc.i4.0 NULL
        bgt.s Label0
        ret NULL
        ldarg.2 NULL [Label0]
        ldc.i4 360
        ble.s Label1
        ldc.i4 360
        starg.s 2
        ldarg.0 NULL [Label1]
        ldarg.1 NULL
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_up()
        call static UnityEngine.Quaternion UnityEngine.Quaternion::AngleAxis(System.Single angle, UnityEngine.Vector3 axis)
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_forward()
        call static UnityEngine.Vector3 UnityEngine.Quaternion::op_Multiply(UnityEngine.Quaternion rotation, UnityEngine.Vector3 point)
        ldc.r4 0.8
        call static UnityEngine.Vector3 UnityEngine.Vector3::op_Multiply(UnityEngine.Vector3 a, System.Single d)
        call static UnityEngine.Vector3 UnityEngine.Vector3::op_Addition(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        starg.s 0
        ldsfld UnityEngine.Mesh[] Verse.MeshPool::pies
        ldarg.2 NULL
        ldelem.ref NULL
        ldarg.0 NULL
        ldarg.1 NULL
        ldarg.2 NULL
        ldc.i4.2 NULL
        div NULL
        conv.r4 NULL
        add NULL
        ldc.r4 90
        sub NULL
        call static UnityEngine.Vector3 UnityEngine.Vector3::get_up()
        call static UnityEngine.Quaternion UnityEngine.Quaternion::AngleAxis(System.Single angle, UnityEngine.Vector3 axis)
        <-->ldsfld UnityEngine.Material Verse.GenDraw::AimPieMaterial
        <-->
        ldsfld UnityEngine.Material CorpsesDeteriorateFaster.UIAssets::AimPieMaterialPatch
        <-->
        ldc.i4.0 NULL
        call static System.Void UnityEngine.Graphics::DrawMesh(UnityEngine.Mesh mesh, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Material material, System.Int32 layer)
        ret NULL
        */
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var newList = new List<CodeInstruction>();

            bool found1 = false;
            for (int i = 0; i < list.Count; i++)
            {

                if (list[i].ToString() == "ldsfld UnityEngine.Material Verse.GenDraw::AimPieMaterial")
                {
                    // ldloc.0
                    found1 = true;
                    newList.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(UIAssets), nameof(UIAssets.AimPieMaterialPatch))));
                }
                else
                {
                    newList.Add(list[i]);
                }
            }

            if (!found1)
            {
                Log.Error("$basics$ Highlight AimTime Icon: found1 failed. Patch not applied.");
                return list;
            }

            Log.Message("$basics$ Highlight AimTime Icon: Patch applied successfully.");
            return newList;
            /*
            Log.Message("$basics$ start");
            Log.Message(newList.Count.ToString());
            foreach (var instruction in newList)
            {
                Log.Message(instruction.ToString());
            }
            Log.Message("$basics$ end");

            return newList;
            */


            /*
            Log.Message("$basics$ start");
			foreach ( var instruction in list )
			{
				Log.Message(instruction.ToString());
			}
            Log.Message("$basics$ end");
			*/
            //return list;
        }
    }
}
