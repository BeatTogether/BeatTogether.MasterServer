using System;
using BeatTogether.MasterServer.Messaging.Models.LegacyModels;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.JsonConverters
{
    public class LegacySongPackMaskConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LegacySongPackMask);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return LegacySongPackMask.Parse(serializer.Deserialize<string>(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((LegacySongPackMask) value).ToShortString());
        }
    }
}