using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley.Audio;

namespace StardewModdingAPI.Mobile.Facade;

internal interface IAudioEngineFacade : IAudioEngine, IRewriteFacade
{
    int GetCategoryIndex(string name);
}
