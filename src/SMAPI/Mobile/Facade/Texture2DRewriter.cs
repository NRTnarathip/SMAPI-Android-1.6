using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Rewriters;

namespace StardewModdingAPI.Mobile.Facade;

public static class Texture2DRewriter
{
    public static void GetData(this Texture2D texture, Color[] data)
    {
        Console.WriteLine("Hook GetData()");
        Console.WriteLine("texture: " + texture.Name);
        Console.WriteLine("texture size width: " + texture.Width);
        Console.WriteLine("texture size height: " + texture.Height);
        Console.WriteLine("data length: " + data.Length);

        //remap data size
        Color[] dataWithActualSize = new Color[texture.ActualWidth * texture.ActualHeight];

        texture.GetData(dataWithActualSize);

        //clone data
        Array.Copy(dataWithActualSize, data, data.Length);
    }

    public static MethodInfo GetData_Color_MethodInfo_New => AccessTools.Method(typeof(Texture2DRewriter), nameof(GetData));
    public const string GetData_FullNameStartWith = "System.Void Microsoft.Xna.Framework.Graphics.Texture2D::GetData";
    public static bool GetData_Callback(
        MapMethodWithCallback mapMethod,
        MethodReference methodReference,
        ModuleDefinition module, ILProcessor cil, Instruction instruction)
    {
        if (methodReference.FullName == "System.Void Microsoft.Xna.Framework.Graphics.Texture2D::GetData<Microsoft.Xna.Framework.Color>(!!0[])")
        {
            instruction.Operand = module.ImportReference(GetData_Color_MethodInfo_New);
            return true;
        }

        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Texture2D), nameof(Texture2D.CopyFromTexture))]
    static void FixCopyFromTexture(Texture2D other)
    {

    }
}
