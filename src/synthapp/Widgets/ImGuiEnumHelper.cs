using System;
using System.Collections.Generic;

namespace SynthApp.Widgets
{
    public class ImGuiEnumHelper<T>
    {
        public string[] Names { get; }
        public T[] Values { get; }

        public ImGuiEnumHelper()
        {
            Type t = typeof(T);
            Names = Enum.GetNames(t);
            Values = (T[])Enum.GetValues(t);
        }
    }

    public class ImGuiEnumHelper
    {
        private static Dictionary<Type, object> s_cachedHelpers = new Dictionary<Type, object>();

        public static ImGuiEnumHelper<T> GetHelper<T>()
        {
            if (!s_cachedHelpers.TryGetValue(typeof(T), out object helper))
            {
                helper = new ImGuiEnumHelper<T>();
                s_cachedHelpers.Add(typeof(T), helper);
            }

            return (ImGuiEnumHelper<T>)helper;
        }
    }
}
