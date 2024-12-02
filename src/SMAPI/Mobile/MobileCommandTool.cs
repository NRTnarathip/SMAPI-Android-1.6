using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework;

namespace StardewModdingAPI.Mobile;

public static class MobileCommandTool
{
    public static void SendCommand(string command)
    {
        MobileConsoleTool.WriteLine(command);
    }
}
