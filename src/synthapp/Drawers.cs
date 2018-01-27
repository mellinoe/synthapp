﻿using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

// Taken from Game Engine Editor.

namespace SynthApp
{
    public abstract class Drawer
    {
        public Type TypeDrawn { get; }

        public bool Draw(string label, ref object obj, GraphicsDevice gd)
        {
            ImGui.PushID(label);

            bool result;
            if (obj == null)
            {
                result = DrawNewItemSelector(label, ref obj, gd);
            }
            else
            {
                result = DrawNonNull(label, ref obj, gd);
            }

            ImGui.PopID();

            return result;
        }

        protected abstract bool DrawNonNull(string label, ref object obj, GraphicsDevice gd);
        protected virtual bool DrawNewItemSelector(string label, ref object obj, GraphicsDevice gd)
        {
            ImGui.Text(label + ": NULL ");
            ImGui.SameLine();
            if (ImGui.Button($"Create New"))
            {
                obj = CreateNewObject();
                return true;
            }
            if (ImGui.IsItemHovered(HoveredFlags.Default))
            {
                ImGui.SetTooltip($"Create a new {TypeDrawn.Name}.");
            }
            return false;
        }

        public virtual object CreateNewObject()
        {
            try
            {
                return Activator.CreateInstance(TypeDrawn);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error creating instance of " + TypeDrawn, e);
            }
        }

        public Drawer(Type type)
        {
            TypeDrawn = type;
        }
    }

    public static class DrawerCache
    {
        private static Dictionary<Type, Drawer> s_drawers = new Dictionary<Type, Drawer>()
        {
            { typeof(int), new FuncDrawer<int>(GenericDrawFuncs.DrawInt) },
            { typeof(float), new FuncDrawer<float>(GenericDrawFuncs.DrawSingle) },
            { typeof(double), new FuncDrawer<double>(GenericDrawFuncs.DrawDouble) },
            { typeof(byte), new FuncDrawer<byte>(GenericDrawFuncs.DrawByte) },
            { typeof(string), new FuncDrawer<string>(GenericDrawFuncs.DrawString, GenericDrawFuncs.NewString) },
            { typeof(bool), new FuncDrawer<bool>(GenericDrawFuncs.DrawBool) },
            // { typeof(ImageSharpTexture), new TextureDrawer() },
            { typeof(RgbaFloat), new FuncDrawer<RgbaFloat>(GenericDrawFuncs.DrawRgbaFloat) },
            { typeof(Vector2), new FuncDrawer<Vector2>(GenericDrawFuncs.DrawVector2) },
            { typeof(Vector3), new FuncDrawer<Vector3>(GenericDrawFuncs.DrawVector3) },
            { typeof(Vector4), new FuncDrawer<Vector4>(GenericDrawFuncs.DrawVector4) },
            // { typeof(Quaternion), new FuncDrawer<Quaternion>(GenericDrawFuncs.DrawQuaternion) }
        };

        // Drawers which should be used when type doesn't identically match queried type.
        private static Dictionary<Type, Drawer> s_fallbackDrawers = new Dictionary<Type, Drawer>();

        public static void AddDrawer(Drawer drawer)
        {
            s_drawers.Add(drawer.TypeDrawn, drawer);
            s_fallbackDrawers.Clear();
        }

        public static Drawer GetDrawer(Type type)
        {
            Drawer d;
            if (!s_drawers.TryGetValue(type, out d))
            {
                d = GetFallbackDrawer(type);
                if (d == null)
                {
                    d = CreateDrawer(type);
                    s_drawers.Add(type, d);
                }
            }

            return d;
        }

        private static Drawer GetFallbackDrawer(Type type)
        {
            Drawer d = null;
            if (s_fallbackDrawers.TryGetValue(type, out d))
            {
                return d;
            }
            else
            {
                int hierarchyDistance = int.MaxValue;
                foreach (var kvp in s_drawers)
                {
                    if (kvp.Key.IsAssignableFrom(type))
                    {
                        int newHD = GetHierarchyDistance(kvp.Key.GetTypeInfo(), type.GetTypeInfo());
                        if (newHD < hierarchyDistance)
                        {
                            hierarchyDistance = newHD;
                            d = kvp.Value;
                        }
                    }
                }

                s_fallbackDrawers.Add(type, d);
                return d;
            }
        }

        private static int GetHierarchyDistance(TypeInfo baseType, TypeInfo derived)
        {
            int distance = 0;
            while ((derived = derived.BaseType?.GetTypeInfo()) != null)
            {
                distance++;
                if (derived == baseType)
                {
                    return distance;
                }
            }

            throw new InvalidOperationException($"{baseType.Name} is not a superclass of {derived.Name}");
        }

        private static Drawer CreateDrawer(Type type)
        {
            TypeInfo ti = type.GetTypeInfo();
            if (ti.IsEnum)
            {
                return new EnumDrawer(type);
            }
            else if (ti.IsArray)
            {
                return (Drawer)Activator.CreateInstance(typeof(ArrayDrawer<>).MakeGenericType(type.GetElementType()));
            }
            else if (ti.IsAbstract)
            {
                return new AbstractItemDrawer(type);
            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                if (typeof(List<>).GetTypeInfo().IsAssignableFrom(type.GetGenericTypeDefinition()))
                {
                    return (Drawer)Activator.CreateInstance(typeof(ListDrawer<>).MakeGenericType(type.GenericTypeArguments[0]));
                }
            }

            return new ComplexItemDrawer(type);
        }
    }

    public abstract class Drawer<T> : Drawer
    {
        public Drawer() : base(typeof(T)) { }

        protected sealed override bool DrawNonNull(string label, ref object obj, GraphicsDevice gd)
        {
            T tObj;
            try
            {
                tObj = (T)obj;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException($"Invalid type given to Drawer<{typeof(T).Name}>. {obj.GetType().Name} is not a compatible type.");
            }

            bool result = Draw(label, ref tObj, gd);
            obj = tObj;
            return result;
        }

        public abstract bool Draw(string label, ref T obj, GraphicsDevice gd);
    }

    public delegate bool DrawFunc<T>(string label, ref T obj, GraphicsDevice gd);
    public class FuncDrawer<T> : Drawer<T>
    {
        private readonly DrawFunc<T> _drawFunc;
        private readonly Func<object> _newFunc;

        public FuncDrawer(DrawFunc<T> drawFunc, Func<object> newFunc = null)
        {
            _drawFunc = drawFunc;
            _newFunc = newFunc;
        }

        public FuncDrawer(Action<T> drawFunc, Func<object> newFunc = null)
        {
            _drawFunc = (string label, ref T obj, GraphicsDevice gd) =>
            {
                drawFunc(obj);
                return false;
            };

            _newFunc = newFunc;
        }

        public override bool Draw(string label, ref T obj, GraphicsDevice gd)
        {
            return _drawFunc(label, ref obj, gd);
        }

        public override object CreateNewObject()
        {
            if (_newFunc != null)
            {
                return _newFunc();
            }
            else
            {
                return base.CreateNewObject();
            }
        }
    }

    public static class GenericDrawFuncs
    {
        public static unsafe bool DrawString(string label, ref string s, GraphicsDevice gd)
        {
            bool result = false;

            if (s == null)
            {
                s = "";
                result = true;
            }

            byte* stackBytes = stackalloc byte[200];
            IntPtr stringStorage = new IntPtr(stackBytes);
            for (int i = 0; i < 200; i++) { stackBytes[i] = 0; }
            IntPtr ansiStringPtr = Marshal.StringToHGlobalAnsi(s);
            SharpDX.Utilities.CopyMemory(stringStorage, ansiStringPtr, s.Length);
            float stringWidth = ImGui.GetTextSize(label).X;
            float drawWidth = Math.Max(200f, stringWidth + 10);
            ImGui.PushItemWidth(drawWidth);
            result |= ImGui.InputText(label, stringStorage, 200, InputTextFlags.Default, null);
            ImGui.PopItemWidth();
            if (result)
            {
                string newString = Marshal.PtrToStringAnsi(stringStorage);
                s = newString;
            }
            Marshal.FreeHGlobal(ansiStringPtr);

            return result;
        }

        public static object NewString()
        {
            return string.Empty;
        }

        public static bool DrawInt(string label, ref int i, GraphicsDevice gd)
        {
            ImGui.PushItemWidth(50f);
            bool result = ImGui.DragInt(label, ref i, 1f, int.MinValue, int.MaxValue, i.ToString());
            ImGui.PopItemWidth();
            return result;
        }

        public static bool DrawSingle(string label, ref float f, GraphicsDevice gd)
        {
            ImGui.PushItemWidth(50f);
            bool result = ImGui.DragFloat(label, ref f, -1000f, 1000f, 0.05f, f.ToString(), 1f);
            ImGui.PopItemWidth();
            return result;
        }

        public static bool DrawDouble(string label, ref double f, GraphicsDevice gd)
        {
            ImGui.PushItemWidth(50f);
            float value = (float)f;
            bool result = ImGui.DragFloat(label, ref value, -1000f, 1000f, 0.05f, f.ToString(), 1f);
            f = value;
            ImGui.PopItemWidth();
            return result;
        }

        public static bool DrawByte(string label, ref byte b, GraphicsDevice gd)
        {
            ImGui.PushItemWidth(50f);
            int val = b;
            if (ImGui.DragInt(label, ref val, 1f, byte.MinValue, byte.MaxValue, b.ToString()))
            {
                b = (byte)val;
                ImGui.PopItemWidth();
                return true;
            }
            ImGui.PopItemWidth();
            return false;
        }

        public static bool DrawBool(string label, ref bool b, GraphicsDevice gd)
        {
            return ImGui.Checkbox(label, ref b);
        }

        internal static bool DrawRgbaFloat(string label, ref RgbaFloat obj, GraphicsDevice gd)
        {
            var color = obj.ToVector4();
            bool result = ImGui.ColorEdit4(label, ref color, ColorEditFlags.Default);
            if (result)
            {
                obj = new RgbaFloat(color.X, color.Y, color.Z, color.W);
            }

            return result;
        }

        internal static bool DrawVector2(string label, ref Vector2 obj, GraphicsDevice gd)
        {
            return ImGui.DragVector2(label, ref obj, float.MinValue, float.MaxValue, .1f);
        }

        internal static bool DrawVector3(string label, ref Vector3 obj, GraphicsDevice gd)
        {
            return ImGui.DragVector3(label, ref obj, float.MinValue, float.MaxValue, .1f);
        }

        internal static bool DrawVector4(string label, ref Vector4 obj, GraphicsDevice gd)
        {
            return ImGui.DragVector4(label, ref obj, float.MinValue, float.MaxValue, .1f);
        }

        //internal static bool DrawQuaternion(string label, ref Quaternion obj, GraphicsDevice gd)
        //{
        //    Vector3 euler = MathUtil.RadiansToDegrees(MathUtil.GetEulerAngles(obj));
        //    if (DrawVector3(label, ref euler, gd))
        //    {
        //        var radians = MathUtil.DegreesToRadians(euler);
        //        obj = Quaternion.CreateFromYawPitchRoll(radians.Y, radians.X, radians.Z);
        //        return true;
        //    }

        //    return false;
        //}
    }

    public class EnumDrawer : Drawer
    {
        private readonly string[] _enumOptions;

        public EnumDrawer(Type enumType) : base(enumType)
        {
            _enumOptions = Enum.GetNames(enumType);
        }

        protected override bool DrawNonNull(string label, ref object obj, GraphicsDevice gd)
        {
            bool result = false;
            string menuLabel = $"{label}: {obj.ToString()}";
            if (ImGui.BeginMenu(menuLabel))
            {
                foreach (string item in _enumOptions)
                {
                    if (ImGui.MenuItem(item, ""))
                    {
                        result = true;
                        obj = Enum.Parse(TypeDrawn, item);
                    }
                }
                ImGui.EndMenu();
            }

            return result;
        }
    }

    public class ArrayDrawer<T> : Drawer
    {
        private readonly bool _isValueType;

        public ArrayDrawer() : base(typeof(T[]))
        {
            _isValueType = typeof(T).GetTypeInfo().IsValueType;
        }

        protected override bool DrawNonNull(string label, ref object obj, GraphicsDevice gd)
        {
            T[] arr = (T[])obj;
            int length = arr.Length;
            bool newArray = false;
            bool arrayModified = false;

            if (ImGui.TreeNode($"{label}[{length}]###{label}"))
            {
                if (ImGui.IsItemHovered(HoveredFlags.Default))
                {
                    ImGui.SetTooltip($"{TypeDrawn.GetElementType()}[{arr.Length}]");
                }

                if (!newArray)
                {
                    if (ImGui.SmallButton("-"))
                    {
                        int newLength = Math.Max(length - 1, 0);
                        Array.Resize(ref arr, newLength);
                        newArray = true;
                    }
                    ImGui.SameLine();
                    ImGui.Spacing();
                    ImGui.SameLine();
                    if (ImGui.SmallButton("+"))
                    {
                        Array.Resize(ref arr, length + 1);
                        newArray = true;
                    }
                }

                length = arr.Length;

                for (int i = 0; i < length; i++)
                {
                    ImGui.PushStyleColor(ColorTarget.Button, RgbaFloat.Red.ToVector4());
                    if (ImGui.Button($"X##{i}", new System.Numerics.Vector2(15, 15)))
                    {
                        ShiftArrayDown(arr, i);
                        Array.Resize(ref arr, length - 1);
                        newArray = true;
                        length -= 1;
                        ImGui.PopStyleColor();
                        i--;
                        continue;
                    }
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    object element = arr[i];
                    Drawer drawer;
                    if (element == null)
                    {
                        drawer = DrawerCache.GetDrawer(typeof(T));
                    }
                    else
                    {
                        Type realType = element.GetType();
                        drawer = DrawerCache.GetDrawer(realType);
                    }

                    bool changed = drawer.Draw($"{TypeDrawn.GetElementType().Name}[{i}]", ref element, gd);
                    if (changed || drawer.TypeDrawn.GetTypeInfo().IsValueType)
                    {
                        arr[i] = (T)element;
                        arrayModified = true;
                    }
                }

                ImGui.TreePop();
            }
            else if (ImGui.IsItemHovered(HoveredFlags.Default))
            {
                ImGui.SetTooltip($"{TypeDrawn.GetElementType()}[{arr.Length}]");
            }

            if (newArray)
            {
                obj = arr;
                return true;
            }

            return arrayModified;
        }

        private void ShiftArrayDown(T[] arr, int start)
        {
            for (int i = start; i < arr.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
        }

        public override object CreateNewObject()
        {
            return new T[0];
        }
    }

    public class ComplexItemDrawer : Drawer
    {
        private readonly PropertyInfo[] _properties;
        private readonly bool _drawRootNode;

        public ComplexItemDrawer(Type type) : this(type, true) { }
        public ComplexItemDrawer(Type type, bool drawRootNode)
            : base(type)
        {
            _drawRootNode = drawRootNode;
            _properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => !pi.IsDefined(typeof(JsonIgnoreAttribute))).ToArray();
        }

        protected override bool DrawNonNull(string label, ref object obj, GraphicsDevice gd)
        {
            ImGui.PushID(label);

            if (!_drawRootNode || ImGui.CollapsingHeader(label, label, true, true))
            {
                foreach (PropertyInfo pi in _properties)
                {
                    if (_drawRootNode)
                    {
                        const int levelMargin = 5;
                        ImGui.PushItemWidth(levelMargin);
                        ImGui.LabelText("", "");
                        ImGui.PopItemWidth();
                        ImGui.SameLine();
                    }

                    object originalValue = pi.GetValue(obj);
                    object value = originalValue;
                    Drawer drawer;
                    if (value != null)
                    {
                        drawer = DrawerCache.GetDrawer(value.GetType());
                    }
                    else
                    {
                        drawer = DrawerCache.GetDrawer(pi.PropertyType);
                    }
                    bool changed = drawer.Draw(pi.Name, ref value, gd);
                    if (changed && originalValue != value || pi.PropertyType.GetTypeInfo().IsValueType)
                    {
                        if (pi.SetMethod != null)
                        {
                            pi.SetValue(obj, value);
                        }
                    }
                }
            }

            ImGui.PopID();

            return false;
        }
    }

    public class AbstractItemDrawer : Drawer
    {
        private readonly Type[] _subTypes;

        public AbstractItemDrawer(Type type) : base(type)
        {
            _subTypes = type.GetTypeInfo().Assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract).ToArray();
        }

        protected override bool DrawNonNull(string label, ref object obj, GraphicsDevice gd)
        {
            throw new InvalidOperationException("AbstractItemDrawer shouldn't be used for non-null items.");
        }

        protected override bool DrawNewItemSelector(string label, ref object obj, GraphicsDevice gd)
        {
            bool result = false;

            ImGui.PushID(label);

            if (ImGui.BeginMenu(label))
            {
                foreach (Type t in _subTypes)
                {
                    if (ImGui.MenuItem(t.Name, ""))
                    {
                        obj = Activator.CreateInstance(t);
                        result = true;
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.PopID();

            return result;
        }
    }

    public class ListDrawer<T> : Drawer<List<T>>
    {
        public override bool Draw(string label, ref List<T> obj, GraphicsDevice gd)
        {
            var arrayDrawer = DrawerCache.GetDrawer(typeof(T[]));
            object arrayAsObj = obj.ToArray();
            if (arrayDrawer.Draw(label, ref arrayAsObj, gd))
            {
                T[] array = (T[])arrayAsObj;
                obj = new List<T>(array);
                return true;
            }

            return false;
        }
    }
}
