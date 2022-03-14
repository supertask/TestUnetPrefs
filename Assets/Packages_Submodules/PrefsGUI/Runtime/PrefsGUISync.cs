using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

#pragma warning disable 0618  

namespace PrefsGUI
{
    /// <summary>
    /// Sync PrefsGUI parameter over UNET
    /// </summary>
    public partial class PrefsGUISync : NetworkBehaviour
    {
        #region Type Define

        public class TypeAndIdx
        {
            public Type type;
            public int idx;
        }

        #endregion


        #region Sync

        SyncListKeyBool _syncListKeyBool = new SyncListKeyBool();
        SyncListKeyInt _syncListKeyInt = new SyncListKeyInt();
        SyncListKeyUInt _syncListKeyUInt = new SyncListKeyUInt();
        SyncListKeyFloat _syncListKeyFloat = new SyncListKeyFloat();
        SyncListKeyDouble _syncListKeyDouble = new SyncListKeyDouble();
        SyncListKeyString _syncListKeyString = new SyncListKeyString();
        SyncListKeyVector2 _syncListKeyVector2 = new SyncListKeyVector2();
        SyncListKeyVector3 _syncListKeyVector3 = new SyncListKeyVector3();
        SyncListKeyVector4 _syncListKeyVector4 = new SyncListKeyVector4();
        SyncListKeyVector2Int _syncListKeyVector2Int = new SyncListKeyVector2Int();
        SyncListKeyVector3Int _syncListKeyVector3Int = new SyncListKeyVector3Int();

        [SyncVar]
        bool _materialPropertyDebugMenuUpdate;

        #endregion

        Dictionary<Type, ISyncListKeyObj> _typeToSyncList;
        Dictionary<string, TypeAndIdx> _keyToTypeIdx = new Dictionary<string, TypeAndIdx>();

        public List<string> _ignoreKeys = new List<string>(); // want use HashSet but use List so it will be serialized on Inspector

        static int IDMaster = -1;
        int ID = -1;
        public void Awake()
        {
            ID = ++IDMaster;
            Debug.LogFormat("PrefsGUISync.Awake() ID: {0}", ID);
            GameObject.DontDestroyOnLoad(this.gameObject);

            _typeToSyncList = new Dictionary<Type, ISyncListKeyObj>()
            {
                { typeof(bool),    _syncListKeyBool    },
                { typeof(int),     _syncListKeyInt     },
                { typeof(uint),    _syncListKeyUInt    },
                { typeof(float),   _syncListKeyFloat   },
                { typeof(double),  _syncListKeyDouble  },
                { typeof(string),  _syncListKeyString  },
                { typeof(Vector2), _syncListKeyVector2 },
                { typeof(Vector3), _syncListKeyVector3 },
                { typeof(Vector4), _syncListKeyVector4 },
                { typeof(Vector2Int), _syncListKeyVector2Int },
                { typeof(Vector3Int), _syncListKeyVector3Int },
            };
        }

		public void OnDestroy()
		{
            Debug.LogFormat( "PrefsGUISync.OnDestroy() ID: {0}", ID );
        }

		public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.SpawnObjects();
            SendPrefs();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ReadPrefs();
        }


        public void Start()
        {
        }

        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();        
        public void Update()
        {
            if (Time.frameCount % 60 == 0)
                Debug.LogFormat("Update on PrefsGUISync isLocalPlayer={0}, isClient={1}, isServer={2}", this.isLocalPlayer, this.isClient, this.isServer);
            //stopwatch.Reset();
            //stopwatch.Start();
            SendPrefs();
            if(this.enumeratorReadPrefs == null || enumeratorReadPrefs.Current == 1)
			{
                enumeratorReadPrefs = ReadPrefs();
            }
            else
			{
                enumeratorReadPrefs.MoveNext();
            }
            //stopwatch.Stop();
            //if( stopwatch.ElapsedMilliseconds > 5 )
            //    Debug.LogFormat("PrefsGUISync.Update() (class ID: {1}) took {0} ms", stopwatch.ElapsedMilliseconds, this.ID );
            
        }


        [ServerCallback]
        void SendPrefs()
        {
            var list = PrefsParam.all.Values.ToList();
            list.ForEach( SendPref );

            _materialPropertyDebugMenuUpdate = MaterialPropertyDebugMenu.update;
        }

        [ServerCallback]
        void SendPref( PrefsParam pref )
        {
            var key = pref.key;
            if ( false == _ignoreKeys.Contains( key ) )
            {
                var obj = pref.GetObject();
                if ( obj != null )
                {
                    var type = pref.GetInnerType();
                    if ( type.IsEnum )
                    {
                        type = typeof( int );
                        obj = Convert.ToInt32( obj );
                    }

                    TypeAndIdx ti;
                    if ( _keyToTypeIdx.TryGetValue( key, out ti ) )
                    {
                        //NOTE:マイフレームセットされ呼ばれる
                        var iSynList = _typeToSyncList[type];
                        ///Debug.Log("Set Prefs: " + key);
                        iSynList.Set( ti.idx, obj );
                    }
                    else
                    {
                        //NOTE:追加されているときだけ呼ばれる
                        Assert.IsTrue( _typeToSyncList.ContainsKey( type ), string.Format( "type [{0}] is not supported.", type ) );
                        var iSynList = _typeToSyncList[type];
                        var idx = iSynList.Count;
                        //Debug.Log("Add Prefs: " + key);
                        iSynList.Add( key, obj );
                        _keyToTypeIdx[ key ] = new TypeAndIdx() { type = type, idx = idx };
                    }
                }
            }
        }

        System.Diagnostics.Stopwatch stopwatchReadPrefs = new System.Diagnostics.Stopwatch();
        IEnumerator<int> enumeratorReadPrefs = null;
        [ClientCallback]
        IEnumerator<int> ReadPrefs()
        {
            // ignore at "Host"
            if( !NetworkServer.active )
            {
                int numYields = 0;
                stopwatchReadPrefs.Reset();
                stopwatchReadPrefs.Start();
                var all = PrefsParam.all;
                var arrayValues = _typeToSyncList.Values.ToArray();
                for(int iV  = 0; iV < arrayValues.Length; ++iV)
                {
                    var sl = arrayValues[iV];
                    for( var i = 0; i < sl.Count; ++i )
                    {
                        var keyObj = sl.Get(i);
                        PrefsParam prefs;
                        if( all.TryGetValue( keyObj.key, out prefs ) )
                        {
                            prefs.SetObject( keyObj._value, true );
                        }
                        stopwatchReadPrefs.Stop();
                        if(stopwatchReadPrefs.ElapsedMilliseconds > 3)
                        { 
                            ++numYields;
                            yield return 0;
                            stopwatchReadPrefs.Reset();
                            stopwatchReadPrefs.Start();
                        }
                        else
						{
                            stopwatchReadPrefs.Start();
                        }
                    }
                }
                //Debug.LogFormat("ReadPrefs completed after {0} yields", numYields);
            }

            MaterialPropertyDebugMenu.update = _materialPropertyDebugMenuUpdate;
            yield return 1;
        }

        //[ClientCallback]
        //void ReadPrefs()
        //{
        //    // ignore at "Host"
        //    if (!NetworkServer.active)
        //    {
        //        var all = PrefsParam.all;
        //        _typeToSyncList.Values.ToList().ForEach(sl =>
        //        {
        //            for (var i = 0; i < sl.Count; ++i)
        //            {
        //                var keyObj = sl.Get(i);
        //                PrefsParam prefs;
        //                if (all.TryGetValue(keyObj.key, out prefs))
        //                {
        //                    prefs.SetObject(keyObj._value, true);
        //                }
        //            }
        //        });
        //    }

        //    MaterialPropertyDebugMenu.update = _materialPropertyDebugMenuUpdate;
        //}
    }


    public static class SyncListStructExtenion
    {
        public class KVField
        {
            public FieldInfo keyField;
            public FieldInfo valueField;
        }

        static Dictionary<Type, KVField> _typeToField = new Dictionary<Type, KVField>();
        static KVField GetField(Type type)
        {
            KVField kvField;
            if (!_typeToField.TryGetValue(type, out kvField))
            {
                _typeToField[type] = kvField = new KVField()
                {
                    keyField = type.GetField("key"),
                    valueField = type.GetField("_value")
                };
            }
            return kvField;
        }


        static T CreateInstance<T>(string key, object obj)
        {
            var ret = Activator.CreateInstance(typeof(T));
            var kvField = GetField(typeof(T));
            kvField.keyField.SetValue(ret, key);
            kvField.valueField.SetValue(ret, obj);
            return (T)ret;
        }

        public static void _Add<T>(this SyncListStruct<T> sl, string key, object obj)
            where T : struct
        {
            sl.Add(CreateInstance<T>(key, obj));
        }

        public static void _Set<T>(this SyncListStruct<T> sl, int idx, object obj)
            where T : struct
        {
            var kvField = GetField(typeof(T));
            if (false == kvField.valueField.GetValue(sl[idx]).Equals(obj))
            {
                var key = (string)kvField.keyField.GetValue(sl[idx]);
                sl[idx] = CreateInstance<T>(key, obj);
            }
        }

        public static PrefsGUISync.KeyObj _Get<T>(this SyncListStruct<T> sl, int idx)
            where T : struct
        {
            var kvField = GetField(typeof(T));
            return new PrefsGUISync.KeyObj()
            {
                key = (string)kvField.keyField.GetValue(sl[idx]),
                _value = kvField.valueField.GetValue(sl[idx])
            };
        }
    }
}
