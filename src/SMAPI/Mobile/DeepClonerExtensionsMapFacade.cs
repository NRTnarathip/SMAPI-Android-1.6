using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley.Objects;

namespace StardewModdingAPI.Mobile;

public class DeepClonerExtensionsRewriter
{
    static void Test()
    {
        DeepClonerExtensions.DeepClone("");
    }
}
