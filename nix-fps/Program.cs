using Microsoft.Extensions.Configuration;


var fileCfg = "app-settings.json";
var cfgApp = new ConfigurationBuilder().AddJsonFile(fileCfg, false, true).Build();

using var game = new nixfps.NixFPS(cfgApp);
game.Run();
