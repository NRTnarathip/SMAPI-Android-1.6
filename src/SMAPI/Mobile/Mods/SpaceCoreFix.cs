using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI.Framework;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using Mono.Cecil;

namespace StardewModdingAPI.Mobile.Mods;

internal static class SpaceCoreFix
{
    static Assembly modAssembly;
    const string SpaceCoreAssemblyName = "SpaceCore";
    public static void Init(AndroidModFixManager androidModFix)
    {
        androidModFix.RegisterOnModLoaded(SpaceCoreAssemblyName, OnModLoaded);
        androidModFix.RegisterOnRewriterModAssemblyDef(SpaceCoreAssemblyName, OnRewriterAssembly);
    }
    static void OnRewriterAssembly(Mono.Cecil.AssemblyDefinition assemblyDef)
    {
        Console.WriteLine("try rewrite SpaceCore.dll");
        var mainModule = assemblyDef.MainModule;

        //Fix FarmerAlwaysAcceptVirtualCurrencyPatch1
        {
            var FarmerAlwaysAcceptVirtualCurrencyPatch1
                = mainModule.GetType("SpaceCore.VanillaAssetExpansion.FarmerAlwaysAcceptVirtualCurrencyPatch1");

            var harmonyPatchAttribute = FarmerAlwaysAcceptVirtualCurrencyPatch1.CustomAttributes[0];
            var harmonyPatch_Types = harmonyPatchAttribute.ConstructorArguments[2];
            var argTypeArray = harmonyPatch_Types.Value as CustomAttributeArgument[];

            var boolType = mainModule.ImportReference(typeof(bool));
            var newArgBoolType = new CustomAttributeArgument(boolType, boolType);
            var argTypeList = new List<CustomAttributeArgument>();
            argTypeList.AddRange(argTypeArray);
            argTypeList.Add(newArgBoolType);
            harmonyPatchAttribute.ConstructorArguments[2] = new CustomAttributeArgument(
                harmonyPatch_Types.Type,
                argTypeList.ToArray()
            );
        }


    }

    static void OnModLoaded(Assembly asm)
    {
        modAssembly = asm;
        var monitor = SCore.Instance.GetMonitorForGame();
        monitor.Log("Start SpaceCoreFix");
        try
        {
            monitor.Log("Start SpaceCoreFix.ApplyFix()");
            ApplyFix();
        }
        catch (Exception ex)
        {
            monitor.Log(ex.ToString());
        }

        monitor.Log("Done SpaceCoreFix");
    }
    static void ApplyFix()
    {
        var harmony = new Harmony(nameof(SpaceCoreFix));
        DisableQuickSave.TryInit(harmony);
        //fix private void GatherLocals()
        var SpaceCoreModEntry = modAssembly.GetType("SpaceCore.SpaceCore");
        harmony.Patch(
            original: AccessTools.Method(SpaceCoreModEntry, "GatherLocals"),
            prefix: AccessTools.Method(typeof(SpaceCoreFix), nameof(Prefix_GatherLocals))
        );

    }
    static bool Prefix_GatherLocals()
    {
        var monitor = SCore.Instance.GetMonitorForGame();
        monitor.Log("call GatherLocals() for android");
        try
        {

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return false;
    }

    static class DisableQuickSave
    {
        public static void TryInit(Harmony harmony)
        {
            var monitor = SCore.Instance.GetMonitorForGame();
            try
            {
                var OptionPageCtor = AccessTools.Constructor(OptionsPageType, [
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                    typeof(float),
                ]);
                harmony.Patch(
                    original: OptionPageCtor,
                    postfix: new(typeof(DisableQuickSave), nameof(Postfix_OptionsPage_Ctor)));

                var saveWholeBackup = typeof(Game1).GetMethod(nameof(Game1.saveWholeBackup));
                harmony.Patch(saveWholeBackup,
                    prefix: new(typeof(DisableQuickSave), nameof(Prefix_saveWholeBackup)));
            }
            catch (Exception ex)
            {
                monitor.Log(ex.ToString(), LogLevel.Error);
            }
        }
        static Type OptionsPageType = typeof(OptionsPage);
        static FieldInfo optionsFieldInfo = OptionsPageType.GetField("options",
            BindingFlags.Instance | BindingFlags.NonPublic);

        static Type OptionsButtonType = typeof(OptionsButton);
        static FieldInfo btnLabelFieldInfo = OptionsButtonType.GetField("_label",
            BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo btnPaddingYField = OptionsButtonType.GetField("paddingY",
            BindingFlags.Instance | BindingFlags.NonPublic);

        const string ButtonBlockLabelText = "Disable QuickSave By SMAPI Android";
        static void Postfix_OptionsPage_Ctor(OptionsPage __instance,
            int x, int y, int width, int height, float widthMod = 1f, float heightMod = 1f)
        {
            var options = optionsFieldInfo.GetValue(__instance) as List<OptionsElement>;
            var btn = options[2] as OptionsButton;
            btnLabelFieldInfo.SetValue(btn, ButtonBlockLabelText);
            int paddingY = (int)btnPaddingYField.GetValue(btn);
            btn.enabled = false;

            int num = (int)Game1.dialogueFont.MeasureString(ButtonBlockLabelText).X + 64;
            int num2 = (int)Game1.dialogueFont.MeasureString(ButtonBlockLabelText).Y + paddingY * 2;
            btn.bounds = new Rectangle(btn.bounds.X, btn.bounds.Y, num, num2);
            btn.button = new ClickableComponent(btn.bounds, "OptionsButton_" + ButtonBlockLabelText);
        }
        static bool Prefix_saveWholeBackup()
        {
            var monitor = SCore.Instance.GetMonitorForGame();
            monitor.Log("bypass saveWholeBackup()");
            return false;
        }
    }
}
