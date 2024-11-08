using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley.Menus;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

/// <summary>Maps Stardew Valley 1.5.6's <see cref="DiscreteColorPicker"/> methods to their newer form to avoid breaking older mods.</summary>
/// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
public class DiscreteColorPickerFacade : DiscreteColorPicker, IRewriteFacade
{
    /*********
    ** Public methods
    *********/


#if SMAPI_FOR_ANDROID
    //not sure
    //public new int getSelectionFromColor(Color c)
    //{
    //    const int totalColors = 21; //
    //    for (int i = 0; i < totalColors; i++)
    //    {
    //        if (this.getColorFromSelection(i).Equals(c))
    //        {
    //            return i;
    //        }
    //    }
    //    return -1;

    //}

    //public new Color getColorFromSelection(int selection)
    //{
    //    return selection switch
    //    {
    //        2 => new Color(119, 191, 255),
    //        1 => new Color(85, 85, 255),
    //        3 => new Color(0, 170, 170),
    //        4 => new Color(0, 234, 175),
    //        5 => new Color(0, 170, 0),
    //        6 => new Color(159, 236, 0),
    //        7 => new Color(255, 234, 18),
    //        8 => new Color(255, 167, 18),
    //        9 => new Color(255, 105, 18),
    //        10 => new Color(255, 0, 0),
    //        11 => new Color(135, 0, 35),
    //        12 => new Color(255, 173, 199),
    //        13 => new Color(255, 117, 195),
    //        14 => new Color(172, 0, 198),
    //        15 => new Color(143, 0, 255),
    //        16 => new Color(89, 11, 142),
    //        17 => new Color(64, 64, 64),
    //        18 => new Color(100, 100, 100),
    //        19 => new Color(200, 200, 200),
    //        20 => new Color(254, 254, 254),
    //        _ => Color.Black,
    //    };
    //}

#else
    public new int getSelectionFromColor(Color c)
    {
        return DiscreteColorPicker.getSelectionFromColor(c);
    }

    public new Color getColorFromSelection(int selection)
    {
        return DiscreteColorPicker.getColorFromSelection(selection);
    }
#endif


    /*********
    ** Private methods
    *********/
    public DiscreteColorPickerFacade()
        : base(0, 0)
    {
        RewriteHelper.ThrowFakeConstructorCalled();
    }
}
