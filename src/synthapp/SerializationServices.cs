using Newtonsoft.Json;
using System.IO;
using System;

namespace SynthApp
{
    public class SerializationServices
    {
        private JsonSerializer _serializer;

        public SerializationServices()
        {
            JsonConverter[] converters = new JsonConverter[]
            {
                new PatternTimeConverter(),
                new PitchConverter()
            };
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                Converters = converters
            });
        }

        public void SaveTo<T>(T item, string path)
        {
            using (var fs = File.OpenWrite(path))
            using (var sw = new StreamWriter(fs))
            {
                _serializer.Serialize(sw, item);
            }
        }

        public T Load<T>(string path)
        {
            using (var fs = File.OpenRead(path))
            using (var sr = new StreamReader(fs))
            using (var jtr = new JsonTextReader(sr))
            {
                return _serializer.Deserialize<T>(jtr);
            }
        }
    }
}
