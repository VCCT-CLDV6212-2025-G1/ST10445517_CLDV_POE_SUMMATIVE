using System.Text.Json;

namespace Microsoft.AspNetCore.Http
{
    // Helper extension class to manage the objects in Session
    public static class SessionExtensions
    {
        //---------------------------------------------------------------------------------------------------------------------
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        //---------------------------------------------------------------------------------------------------------------------
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            // Deserialize if the value exists
            // if not return the default outcome
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//-----------------------------------------------------------END OF FILE-------------------------------------------------------