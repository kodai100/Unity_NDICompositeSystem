﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

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


        public void Awake()
        {
            _typeToSyncList = new Dictionary<Type, ISyncListKeyObj>()
            {
                { typeof(bool),    _syncListKeyBool    },
                { typeof(int),     _syncListKeyInt     },
                { typeof(uint),    _syncListKeyUInt    },
                { typeof(float),   _syncListKeyFloat   },
                { typeof(string),  _syncListKeyString  },
                { typeof(Vector2), _syncListKeyVector2 },
                { typeof(Vector3), _syncListKeyVector3 },
                { typeof(Vector4), _syncListKeyVector4 },
                { typeof(Vector2Int), _syncListKeyVector2Int },
                { typeof(Vector3Int), _syncListKeyVector3Int },
            };
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.SpawnObjects();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ReadPrefs();
        }


        public void Start()
        {
            SendPrefs();
        }

        public void Update()
        {
            SendPrefs();
            ReadPrefs();
        }


        [ServerCallback]
        void SendPrefs()
        {
            PrefsParam.all.Values.ToList().ForEach(prefs =>
            {
                var key = prefs.key;
                if (false == _ignoreKeys.Contains(key))
                {
                    var obj = prefs.GetObject();
                    var type = prefs.GetInnerType();
                    if (type.IsEnum)
                    {
                        type = typeof(int);
                        obj = Convert.ToInt32(obj);
                    }

                    TypeAndIdx ti;
                    if (_keyToTypeIdx.TryGetValue(key, out ti))
                    {
                        var iSynList = _typeToSyncList[type];
                        iSynList.Set(ti.idx, obj);
                    }
                    else
                    {
                        Assert.IsTrue(_typeToSyncList.ContainsKey(type), string.Format("type [{0}] is not supported.", type));
                        var iSynList = _typeToSyncList[type];
                        var idx = iSynList.Count;
                        iSynList.Add(key, obj);
                        _keyToTypeIdx[key] = new TypeAndIdx() { type = type, idx = idx };
                    }
                }
            });

            _materialPropertyDebugMenuUpdate = MaterialPropertyDebugMenu.update;
        }

        [ClientCallback]
        void ReadPrefs()
        {
            // ignore at "Host"
            if (!NetworkServer.active)
            {
                var all = PrefsParam.all;
                _typeToSyncList.Values.ToList().ForEach(sl =>
                {
                    for (var i = 0; i < sl.Count; ++i)
                    {
                        var keyObj = sl.Get(i);
                        PrefsParam prefs;
                        if (all.TryGetValue(keyObj.key, out prefs))
                        {
                            prefs.SetObject(keyObj._value, true);
                        }
                    }
                });
            }

            MaterialPropertyDebugMenu.update = _materialPropertyDebugMenuUpdate;
        }
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
