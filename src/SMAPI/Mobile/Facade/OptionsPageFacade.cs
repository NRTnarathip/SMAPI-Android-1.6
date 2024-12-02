using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewValley.Menus;

namespace StardewModdingAPI.Mobile.Facade;

public class OptionsPageFacade : OptionsPage, IRewriteFacade
{

    public OptionsPageFacade(int x, int y, int width, int height, float widthMod = 1, float heightMod = 1) : base(x, y, width, height, widthMod, heightMod)
    {
    }

    static FieldInfo _optionsField = typeof(OptionsPage).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic);
    public List<OptionsElement> options
    {
        get => _optionsField.GetValue(this) as List<OptionsElement>;
        set => _optionsField.SetValue(this, value);
    }
}
