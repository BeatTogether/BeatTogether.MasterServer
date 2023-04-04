using System;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.JsonConverters
{
    public class SongPackMaskConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SongPackMask);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return SongPackMask.Parse(serializer.Deserialize<string>(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((SongPackMask) value).ToShortString());
        }
    }
}