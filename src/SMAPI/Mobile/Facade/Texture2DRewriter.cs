using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.Hardware.Lights;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using static Android.Icu.Text.ListFormatter;

namespace StardewModdingAPI.Mobile.Facade;
[HarmonyPatch]
public static class Texture2DRewriter
{
    public static void GetData(this Texture2D texture, Color[] data)
    {
        //Console.WriteLine("Hook GetData()");
        //Console.WriteLine("texture: " + texture.Name);
        //Console.WriteLine("texture size width: " + texture.Width);
        //Console.WriteLine("texture size height: " + texture.Height);
        //Console.WriteLine("data length: " + data.Length);

        //remap data size
        Color[] dataWithActualSize = new Color[texture.ActualWidth * texture.ActualHeight];

        texture.GetData(dataWithActualSize);

        //TODO: Work in progress
        //not sure

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

    static List<Texture2D> ListAllTextureFrom_FixCopyFromTexture = new();
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Texture2D), nameof(Texture2D.CopyFromTexture))]
    static bool FixCopyFromTexture(Texture2D __instance, Texture2D other)
    {
        var srcTexture = other;
        var dstTexture = __instance;
        //Console.WriteLine("try fix copy from texture");
        //Console.WriteLine($"other width: {srcTexture.Width}, height: {srcTexture.Height}");
        //Console.WriteLine($"other actual width: {srcTexture.ActualWidth},actual height: {srcTexture.ActualHeight}");
        //Console.WriteLine($"try copy to: width {dstTexture.Width}, height: {dstTexture.Height}");
        //Console.WriteLine($"try copy to: actual width {dstTexture.ActualWidth},actual height: {dstTexture.ActualHeight}");

        //get data, android use size power two
        Color[] dataInActualSize = new Color[other.ActualWidth * other.ActualHeight];
        other.GetData(dataInActualSize);

        //set width, height
        width_Field.SetValue(dstTexture, srcTexture.Width);
        height_Field.SetValue(dstTexture, srcTexture.Height);

        //set actual width, height
        ActualWidth_Prop.SetValue(dstTexture, srcTexture.ActualWidth);
        ActualHeight_Prop.SetValue(dstTexture, srcTexture.ActualHeight);

        //set texel width, height
        float texelWidth = (float)TexelWidth_Prop.GetValue(other);
        TexelWidth_Prop.SetValue(dstTexture, texelWidth);

        float texelHeight = (float)TexelHeight_Prop.GetValue(other);
        TexelHeight_Prop.SetValue(dstTexture, texelHeight);

        dstTexture.SetData(dataInActualSize);

        ListAllTextureFrom_FixCopyFromTexture.Add(dstTexture);
        Console.WriteLine("done fix clone texture: " + dstTexture.Name);

        return false;
    }
    static PropertyInfo TexelWidth_Prop = AccessTools.Property(typeof(Texture2D), "TexelWidth");
    static PropertyInfo TexelHeight_Prop = AccessTools.Property(typeof(Texture2D), "TexelHeight");
    static PropertyInfo ActualWidth_Prop = AccessTools.Property(typeof(Texture2D), "ActualWidth");
    static PropertyInfo ActualHeight_Prop = AccessTools.Property(typeof(Texture2D), "ActualHeight");
    static FieldInfo width_Field = AccessTools.Field(typeof(Texture2D), "width");
    static FieldInfo height_Field = AccessTools.Field(typeof(Texture2D), "height");
}
