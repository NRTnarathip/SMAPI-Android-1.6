using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewModdingAPI.Mobile;

[HarmonyPatch]
internal static class MonoModFix
{
    public class Hello
    {
        protected int version = 777;
        public void Draw()
        {
            try
            {
                int current = this.version;
                this.DrawInternal();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        protected void DrawInternal()
        {
            Console.WriteLine("Draw internal");
        }
    }
    public static Hello hello = new();
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hello), nameof(Hello.Draw))]
    public static void PrefixDrawPublic()
    {
        Console.WriteLine("prefix draw");
    }
}
