using PrefsGUI.Wrapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace PrefsGUI
{
    #region static class
    public class Prefs
    {
		// used for Json version of data.
		// PrefsWrapperNative uses Unity's player prefs and the save location is fixed
		public enum FileLocation
		{
			PersistantData,		// Windows Application's Data folder.  Inside of c:/users/<user>/AppData/...
			StreamingAssets,	// Unity streaming assets folder
			HardCodedPath,		// a user defined path to anywhere on the computer's hard drive

			NumLocations
		}

        public static void Save() { PrefsGlobal.Save(); }
		public static void SaveInAllLocations()
		{
			FileLocation backup = GetFileLocation();
			for(int i = 0; i < (int)FileLocation.NumLocations; ++i)
			{
				SetFileLocation((FileLocation)i);
				Save();
			}

			// restore origional location
			SetFileLocation(backup);
		}
        public static void Load() { PrefsGlobal.Load(); }
        public static void DeleteAll() { PrefsGlobal.DeleteAll(); }
		public static void SetFileLocation( FileLocation fileLocation) { PrefsGlobal.SetFileLocation(fileLocation); }
		public static void SetFileLocationHardCodedPath( string hardCodedPath) { PrefsGlobal.SetFileLocationHardCodedPath(hardCodedPath); }
		public static FileLocation GetFileLocation() { return PrefsGlobal.GetFileLocation(); }
		public static void SetFilePathPrefix(Prefs.FileLocation fileLocation, string prefix) { PrefsGlobal.SetFilePathPrefix(fileLocation, prefix); }
		public static DateTime GetFileTimeStamp() { return PrefsGlobal.GetFileTimeStamp(); }
		public static string GetFileNameAndPath() { return PrefsGlobal.GetFileNameAndPath(); }
    }
    #endregion


    [Serializable]
    public class PrefsString : PrefsParam<string>
    {
        public PrefsString(string key, string defaultValue = "") : base(key, defaultValue) { }
    }

    [Serializable]
    public class PrefsInt : PrefsParam<int>
    {
        public PrefsInt(string key, int defaultValue = default(int)) : base(key, defaultValue) { }
    }

    [Serializable]
    public class PrefsFloat : PrefsParam<float>
    {
        public PrefsFloat(string key, float defaultValue = default(float)) : base(key, defaultValue) { }

        public bool OnGUISlider(string label = null) { return OnGUISlider(0f, 1f, label); }
        public bool OnGUISlider(float min, float max, string label = null)
        {
            return OnGUIStrandardStyle((float v, ref string unparsedStr) =>
            {
                GUIUtil.PrefixLabel(label ?? key);
                return GUIUtil.Slider(v, min, max, ref unparsedStr);
            });
        }
    }

    [Serializable]
    public class PrefsBool : PrefsParam<bool>
    {
        public PrefsBool(string key, bool defaultValue = default(bool)) : base(key, defaultValue) { }
        public bool OnGUIToggle(string label = null)
        {
            return OnGUIStrandardStyle((bool v, ref string unparsedStr) =>
            {
                return GUILayout.Toggle(v, label);
            });
        }
    }

    [Serializable]
    public class PrefsVector2 : PrefsVector<Vector2>
    {
        public PrefsVector2(string key, Vector2 defaultValue = default(Vector2)) : base(key, defaultValue) { }

        public static implicit operator Vector3(PrefsVector2 v) { return v.Get(); }
        public static implicit operator Vector4(PrefsVector2 v) { return v.Get(); }
    }

    [Serializable]
    public class PrefsVector3 : PrefsVector<Vector3>
    {
        public PrefsVector3(string key, Vector3 defaultValue = default(Vector3)) : base(key, defaultValue) { }

        public static implicit operator Vector2(PrefsVector3 v) { return v.Get(); }
        public static implicit operator Vector4(PrefsVector3 v) { return v.Get(); }
    }

    [Serializable]
    public class PrefsVector4 : PrefsVector<Vector4>
    {
        public PrefsVector4(string key, Vector4 defaultValue = default(Vector4)) : base(key, defaultValue) { }

        public static implicit operator Vector2(PrefsVector4 v) { return v.Get(); }
        public static implicit operator Vector3(PrefsVector4 v) { return v.Get(); }
        public static implicit operator Color(PrefsVector4 v) { return v.Get(); }
    }

    [Serializable]
    public class PrefsVector2Int : PrefsVector<Vector2Int>
    {
        public PrefsVector2Int(string key, Vector2Int defaultValue = default(Vector2Int)) : base(key, defaultValue) { }

        protected override Vector2Int defaultMax { get { return base.defaultMax * 100; } }

        public static implicit operator Vector2Int(PrefsVector2Int v) { return v.Get(); }
    }

    [Serializable]
    public class PrefsVector3Int : PrefsVector<Vector3Int>
    {
        public PrefsVector3Int(string key, Vector3Int defaultValue = default(Vector3Int)) : base(key, defaultValue) { }

        protected override Vector3Int defaultMax { get { return base.defaultMax * 100; } }

        public static implicit operator Vector3Int(PrefsVector3Int v) { return v.Get(); }
    }


    [Serializable]
    public class PrefsColor : PrefsTuple<Color, Vector4>
    {
        public PrefsColor(string key, Color defaultValue = default(Color)) : base(key, defaultValue) { }

        protected override Vector4 defaultMin { get { return Vector4.zero; } }
        protected override Vector4 defaultMax { get { return Vector4.one; } }

        static readonly string[] _defaultElementLabels = new[] { "H", "S", "V", "A" };
        protected override string[] defaultEelementLabels
        {
            get
            {
                return _defaultElementLabels;
            }
        }

        protected override bool Compare(Vector4 lhs, Vector4 rhs)
        {
            return lhs == rhs;
        }

        protected override void OnGUISliderRight(Vector4 v)
        {
            var c = ToOuter(v);
            using (var cs = new GUIUtil.ColorScope(c))
            {
                GUILayout.Label("■■■");
            }
        }

        protected override Color ToOuter(Vector4 v4)
        {
            var c = Color.HSVToRGB(v4.x, v4.y, v4.z);
            c.a = v4.w;
            return c;
        }

        protected override Vector4 ToInner(Color c)
        {
            Vector4 v4 = default(Vector4);
            Color.RGBToHSV(c, out v4.x, out v4.y, out v4.z);
            v4.w = c.a;
            return v4;
        }

        public static implicit operator Vector4(PrefsColor c)
        {
            return c.Get();
        }
    }


    [Serializable]
    public class PrefsRect : PrefsTuple<Rect, Vector4>
    {
        public PrefsRect(string key, Rect defaultValue = default(Rect)) : base(key, defaultValue) { }

        protected override string[] defaultEelementLabels { get { return null; } }
        protected override Vector4 defaultMax { get { return Vector4.one; } }
        protected override Vector4 defaultMin { get { return Vector4.zero; } }

        protected override Vector4 ToInner(Rect outerV)
        {
            return new Vector4(outerV.x, outerV.y, outerV.width, outerV.height);
        }

        protected override Rect ToOuter(Vector4 innerV)
        {
            return new Rect(innerV.x, innerV.y, innerV.z, innerV.w);
        }
    }

    /// <summary>
    /// OnGUI() is depreciated. NOT user friendly.
    /// you can set customGUI or write runtime GUI.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PrefsList<T> : PrefsParam<List<T>, string>, IList<T>
    {
        Func<List<T>, List<T>> _customOnGUIFunc;

        public PrefsList(string key, List<T> defaultValue = default(List<T>)) : this(key, null, defaultValue) { }
        public PrefsList(string key, Func<List<T>, List<T>> customOnGUIFunc, List<T> defaultValue = default(List<T>)) : base(key, defaultValue)
        {
            _customOnGUIFunc = customOnGUIFunc;
        }


        public override bool OnGUI(string label = null)
        {
            return OnGUIStrandardStyle((string v, ref string unparsedStr) =>
            {
                string ret = null;
                string l = label ?? key;
                if (_customOnGUIFunc != null)
                {
                    GUILayout.Label(l);
                    GUIUtil.Indent(() => ret = ToInner(_customOnGUIFunc(ToOuter(v))));
                }
                else
                {
                    ret = GUIUtil.Field<string>(v, ref unparsedStr, l);
                }

                return ret;
            });
        }

        static List<T> _empty = new List<T>();
        XmlSerializer serializer_;
        XmlSerializer serializer => serializer_ ?? (serializer_ = new XmlSerializer(typeof(List<T>)));


        protected override string ToInner(List<T> outerV)
        {
            if (outerV == null) return "";

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, outerV);
                return writer.ToString();
            }
        }

        protected override List<T> ToOuter(string innerV)
        {
            if (!string.IsNullOrEmpty(innerV))
            {
                using (StringReader reader = new StringReader(innerV))
                {
                    try
                    {
                        return (List<T>)serializer.Deserialize(reader);
                    }
                    catch { }
                }
            }
            return _empty;
        }

        #region IList<T>
        protected void UpdateValue(Action<List<T>> action) { var v = Get(); action(v); Set(v); }

        public int Count { get { return Get().Count; } }
        public bool IsReadOnly { get { return false; } }
        public T this[int index] { get { return Get()[index]; } set { UpdateValue((v) => v[index] = value); } }
        public int IndexOf(T item) { return Get().IndexOf(item); }
        public void Insert(int index, T item) { UpdateValue((v) => v.Insert(index, item)); }
        public void RemoveAt(int index) { UpdateValue((v) => v.RemoveAt(index)); }
        public void Add(T item) { UpdateValue((v) => v.Add(item)); }
        public void Clear() { UpdateValue((v) => v.Clear()); }
        public bool Contains(T item) { return Get().Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { Get().CopyTo(array, arrayIndex); }
        public bool Remove(T item)
        {
            var v = Get();
            var ret = v.Remove(item);
            Set(v);
            return ret;
        }
        public IEnumerator<T> GetEnumerator() { return Get().GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Get().GetEnumerator(); }
        #endregion
    }


    #region abstract classes
    public abstract class PrefsVector<T> : PrefsTuple<T, T>
    {
        public PrefsVector(string key, T defaultValue = default(T)) : base(key, defaultValue) { }

        protected override T defaultMin { get { return (T)typeof(T).GetProperty("zero").GetValue(null); } }
        protected override T defaultMax { get { return (T)typeof(T).GetProperty("one").GetValue(null); } }

        static readonly string[] _defaultElementLabels = new[] { "x", "y", "z", "w" };
        protected override string[] defaultEelementLabels
        {
            get
            {
                return _defaultElementLabels;
            }
        }
        protected override T ToOuter(T innerV) { return innerV; }
        protected override T ToInner(T TouterV) { return TouterV; }
    }

    public abstract class PrefsTuple<OuterT, InnerT> : PrefsParam<OuterT, InnerT>
    {
        bool foldOpen;

        #region abstract
        protected abstract string[] defaultEelementLabels { get; }
        protected abstract InnerT defaultMin { get; }
        protected abstract InnerT defaultMax { get; }
        #endregion abstract

        public PrefsTuple(string key, OuterT defaultValue = default(OuterT)) : base(key, defaultValue) { }

        public bool OnGUISlider(string label = null)
        {
            return OnGUISlider(defaultMin, defaultMax, label);
        }

        public bool OnGUISlider(OuterT min, OuterT max, string label = null, string[] elementLabels = null)
        {
            return OnGUISlider(ToInner(min), ToInner(max), label, elementLabels);
        }

        public bool OnGUISliderHSV(InnerT min, InnerT max, string label = null, string[] elementLabels = null)
        {
            return OnGUISlider(min, max, label, elementLabels);
        }

        protected bool OnGUISlider(InnerT min, InnerT max, string label = null, string[] elementLabels = null)
        {
            return OnGUIStrandardStyle((InnerT v, ref string unparsedStr) =>
            {
                elementLabels = elementLabels ?? defaultEelementLabels;

                using (var h = new GUILayout.HorizontalScope())
                {
                    var foldStr = foldOpen ? "▼" : "▶";

                    foldOpen ^= GUIUtil.Prefix((width) => GUILayout.Button(foldStr + (label ?? key), GUIUtil.Style.FoldoutPanelStyle, GUILayout.Width(width)));

                    v = foldOpen
                        ? GUIUtil.Slider(v, min, max, ref unparsedStr, "", elementLabels)
                        : GUIUtil.Field(v, ref unparsedStr, null);

                    OnGUISliderRight(v);
                    //GUILayout.FlexibleSpace();
                }

                return v;
            });
        }

        protected virtual void OnGUISliderRight(InnerT v) { }
    }


    public class PrefsParam<T> : PrefsParam<T, T>
    {
        public PrefsParam(string key, T defaultValue = default(T)) : base(key, defaultValue) { }
        protected override T ToOuter(T innerV) { return innerV; }
        protected override T ToInner(T TouterV) { return TouterV; }
    }


    /// <summary>
    /// Basic implementation of OuterT and InnnerT
    /// </summary>
    /// <typeparam name="OuterT"></typeparam>
    /// <typeparam name="InnerT"></typeparam>
    public abstract class PrefsParam<OuterT, InnerT> : PrefsParamOuter<OuterT>
    {
        protected bool isCachedOuter = false;
        protected OuterT cachedOuter;

        protected bool isCachedObj = false;
        protected object cachedObj;

        protected bool synced;

        protected bool hasDefaultInner;
        protected InnerT defaultInner;

        public PrefsParam(string key, OuterT defaultValue = default(OuterT)) : base(key, defaultValue)
        {
        }

        protected InnerT _Get()
        {
            if (!hasDefaultInner)
            {
                defaultInner = ToInner(defaultValue);
                hasDefaultInner = true;
            }
            return PlayerPrefs<InnerT>.Get(key, defaultInner);
        }

        protected void _Set(InnerT v, bool synced = false)
        {
            if (false == Compare(v, _Get()))
            {
                PlayerPrefs<InnerT>.Set(key, v);
                isCachedOuter = false;
                isCachedObj = false;
            }
            this.synced = synced;
        }


        #region override

        public override OuterT Get()
        {
            if (!isCachedOuter)
            {
                cachedOuter = ToOuter(_Get());
                isCachedOuter = true;
            }
            return cachedOuter;
        }

        public override void Set(OuterT v) { _Set(ToInner(v)); }

        public override Type GetInnerType()
        {
            return typeof(InnerT);
        }
        public override object GetObject()
        {
            if (!isCachedObj)
            {
                cachedObj = _Get();
                isCachedObj = true;
            }

            return cachedObj;
        }
        public override void SetObject(object obj, bool synced)
        {
            _Set((InnerT)obj, synced);
        }

        public override bool OnGUI(string label = null)
        {
            return OnGUIStrandardStyle((InnerT v, ref string unparsedStr) =>
            {
                GUIUtil.PrefixLabel(label ?? key);
                return GUIUtil.Field(v, ref unparsedStr, null);
            });
        }

        public override bool IsDefault { get { return Compare(ToInner(defaultValue), _Get()); } }
        public override void SetCurrentToDefault() { defaultValue = Get(); }
        #endregion


        #region abstract
        protected abstract OuterT ToOuter(InnerT innerV);
        protected abstract InnerT ToInner(OuterT outerV);
        #endregion


        #region GUI Implement
        protected bool OnGUIStrandardStyle(GUIFunc guiFunc)
        {
            Color? prevColor = null;
            if (synced)
            {
                prevColor = GUI.color;
                GUI.color = syncedColor;
            }
            var ret = OnGUIwithButton(() => OnGUIWithUnparsedStr(key, guiFunc));

            if (prevColor.HasValue) GUI.color = prevColor.Value;

            return ret;
        }

        protected virtual bool Compare(InnerT lhs, InnerT rhs) { return lhs.Equals(rhs); }

        protected bool OnGUIwithButton(Func<bool> onGUIFunc)
        {
            var changed = false;
            using (var h = new GUILayout.HorizontalScope())
            {
                changed = onGUIFunc();
                changed |= OnGUIDefaultButton();
            }

            return changed;
        }

        // public for Custom GUI
        public bool OnGUIDefaultButton()
        {
            var label = Compare(_Get(), ToInner(defaultValue)) ? "default" : "<color=red>default</color>";

            var ret = GUILayout.Button(label, GUILayout.ExpandWidth(false));
            if (ret)
            {
                Set(defaultValue);
            }

            return ret;
        }

        static Dictionary<string, string> _unparsedStrTable = new Dictionary<string, string>();
        protected delegate InnerT GUIFunc(InnerT v, ref string unparsedStr);

        protected bool OnGUIWithUnparsedStr(string key, GUIFunc guiFunc)
        {
            var changed = false;
            if (!PlayerPrefs<InnerT>.HasKey(key))
            {
                Set(defaultValue);
                changed = true;
            }

            var hasUnparsedStr = _unparsedStrTable.ContainsKey(key);
            var unparsedStr = hasUnparsedStr ? _unparsedStrTable[key] : null;

            var prev = _Get();
            var next = guiFunc(prev, ref unparsedStr);
            if (!Compare(prev, next))
            {
                _Set(next);
                changed = true;
            }

            if (unparsedStr != null) _unparsedStrTable[key] = unparsedStr;
            else if (hasUnparsedStr) _unparsedStrTable.Remove(key);

            return changed;
        }
        #endregion
    }


    /// <summary>
    /// Define Outer Interface
    /// </summary>
    /// <typeparam name="OuterT"></typeparam>
    public abstract class PrefsParamOuter<OuterT> : PrefsParam
    {
        [SerializeField]
        protected OuterT defaultValue;

        public PrefsParamOuter(string key, OuterT defaultValue = default(OuterT)) : base(key)
        {
            this.defaultValue = defaultValue;
        }

        public static implicit operator OuterT(PrefsParamOuter<OuterT> me)
        {
            return me.Get();
        }

        #region abstract

        public abstract OuterT Get();

        public abstract void Set(OuterT v);

        #endregion


        #region override

        public override void SetCurrentToDefault() { defaultValue = Get(); }

        #endregion
    }

    public abstract class PrefsParam : ISerializationCallbackReceiver
    {
        #region RegistAllInstance
        public static Dictionary<string, PrefsParam> all = new Dictionary<string, PrefsParam>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { Regist(); } // To Regist Array/List In Inspector. Constructor not called.

        void Regist() { all[key] = this; }
        #endregion

        public string key;
        public static Color syncedColor = new Color32(255, 143, 63, 255);

        public PrefsParam(string key)
        {
            this.key = key;
            Regist();
        }
        public virtual void Delete() { PlayerPrefs.DeleteKey(key); }


        #region abstract

        public abstract Type GetInnerType();
        public abstract object GetObject();
        public abstract void SetObject(object obj, bool synced);

        public abstract bool OnGUI(string label = null);
        public abstract bool IsDefault { get; }
        public abstract void SetCurrentToDefault();

        #endregion
    }
    #endregion
}
