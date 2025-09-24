using StardewModdingAPI;

namespace InstantBuildingConstructionAndUpgrade
{
	public partial class ModEntry
	{
		internal void RegisterConsoleCommands()
		{
			SHelper.ConsoleCommands.Add("ibcu_reload", "Reapply modifications to building data and farmhouse renovation data.", IBCU_reload);
		}

		private void IBCU_reload(string command, string[] args)
		{
			SHelper.GameContent.InvalidateCache(asset => asset.Name.IsEquivalentTo("Data/Buildings"));
			SHelper.GameContent.InvalidateCache(asset => asset.Name.IsEquivalentTo("Data/HomeRenovations"));
			SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Strings/Locations"));
			SMonitor.Log("Modifications to building data and farmhouse renovation data reapplied.", LogLevel.Info);
		}
	}
}
