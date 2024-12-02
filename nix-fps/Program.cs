using System.Diagnostics;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

using ( var game = new nixfps.NixFPS())
{
    try
    {
        game.Run();
    }
    catch (Exception e)
    {
        File.WriteAllText("exception.txt", $"MSG: {e.Message}\nFUNC: {e.TargetSite}\nTRACE: {e.StackTrace}\n" );
        throw;
    }
}

