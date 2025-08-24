using Newtonsoft.Json.Serialization;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace FurnitureDisplayFramework
{
    public partial class ModEntry
    {

        private static void HandleDeserializationError(object sender, ErrorEventArgs e)
        {
            //var currentError = e.ErrorContext.Error.Message;
            //SMonitor.Log(currentError);
            e.ErrorContext.Handled = true;
        }

        private static void HandleSerializationError(object sender, ErrorEventArgs e)
        {
            //var currentError = e.ErrorContext.Error.Message;
            //SMonitor.Log(currentError);
            e.ErrorContext.Handled = true;
        }

        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            return new Object(id, amount, false, -1, quality);
        }
    }
}