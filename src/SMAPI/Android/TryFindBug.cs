using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewModdingAPI.Android;

[HarmonyPatch]
static internal class TryFindBug
{
    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(Game1), "_update")]
    //static void _update(GameTime gameTime)
    //{
    //    Console.WriteLine("prefix Game1._update(): ");
    //}

    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(DebugTools), "BeforeGameUpdate")]
    //static void BeforeGameUpdate(Game game, ref GameTime gameTime)
    //{
    //    Console.WriteLine("Prefix BeforeGameUpdate()");
    //}

}
