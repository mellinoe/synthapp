using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SynthApp
{
    public class PatternTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PatternTime) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return new PatternTime((uint)array[0], (uint)array[1]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            PatternTime pt = (PatternTime)value;
            writer.WriteStartArray();
            writer.WriteValue(pt.Step);
            writer.WriteValue(pt.Tick);
            writer.WriteEndArray();
        }
    }
}