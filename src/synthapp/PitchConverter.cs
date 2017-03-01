using Newtonsoft.Json;
using System;

namespace SynthApp
{
    public class PitchConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Pitch) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new Pitch(byte.Parse(reader.Value.ToString()));
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Pitch p = (Pitch)value;
            writer.WriteValue((int)p.Value);
        }
    }
}
