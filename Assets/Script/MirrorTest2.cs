using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Mirror;
using UnityEngine.Networking;

public class MirrorTest2 : NetworkBehaviour
{
    //private const int numOfVector4List = 4000;

    private const int numOfVector3List = 300;
    private const int numOfVector4List = 300;
    private int maxNumOfStrings = 50;
    public string v;
    //public SyncListStruct<string> m_strs = new SyncListStruct<string>();
    

    public struct KeyVector3
    {
        public string key; public Vector3 value;

        public override string ToString() { return string.Format("key = {0}, value = {1}", key, value); }
    }
    public struct KeyVector4
    {
        public string key;
        public Vector4 value;

        public override string ToString() { return string.Format("key = {0}, value = {1}", key, value); }
    }
    public class SyncListVector3 : SyncListStruct<KeyVector3> { }
    public class SyncListVector4 : SyncListStruct<KeyVector4> { }
    //public SyncListVector3 m_Vector3 = new SyncListVector3();
    public SyncListVector4 m_Vector4 = new SyncListVector4();

    public void OnVector4Changed(SyncListVector4.Operation op, int index)
    {
        Debug.LogFormat("list changed: {0}, index: {1}, item: {2}", op, index, m_Vector4[index]);
    }

    public void OnStringChanged(SyncListStruct<string>.Operation op, int index)
    {
        Debug.LogFormat("list changed: {0}, index: {1}, item: {2}", op, index, m_Vector4[index]);
    }
    
    
    void Start()
    {
        //ConnectionConfig myConfig = new ConnectionConfig();
        //myConfig.AddChannel(QosType.Unreliable);
        //myConfig.AddChannel(QosType.UnreliableFragmented);
        //myConfig.PacketSize = 1800;
    }

    public override void OnStartServer()
    {
        
        // Generate random vector4
        for(int i = 0; i < numOfVector4List; i++)
        {
            //m_Vector3.Add(RandomKeyVector3(i));
            m_Vector4.Add(RandomKeyVector4(i));
        }
    }
    
    public KeyVector4 RandomKeyVector4(int seed)
    {
        return new KeyVector4()
        {
            key = StringUtils.GeneratePassword(maxNumOfStrings, seed),
            value = new Vector4(0,0,0,0)
        };
    }

    public KeyVector3 RandomKeyVector3(int seed)
    {
        return new KeyVector3() {
            key = StringUtils.GeneratePassword(maxNumOfStrings, seed),
            value = new Vector3(0,0,0)
        };
    }

    public override void OnStartClient()
    {
        //m_strs.Callback += OnStringChanged;
        m_Vector4.Callback += OnVector4Changed;
    }
    
    public void Update()
    {
        if (Time.frameCount % 60 == 0)
            Debug.LogFormat("Update on MirrorTest2 isLocalPlayer={0}, isClient={1}, isServer={2}", this.isLocalPlayer, this.isClient, this.isServer);

        if (Time.frameCount % (60 * 5)  == 0 && ( NetworkServer.active ) )
            m_Vector4[2] = this.RandomKeyVector4(Time.frameCount);
        //    m_strs[10000-1] = v;
        //    m_strs[10000-1] = v;
    }
}


public static class StringUtils
{
    private const string PASSWORD_CHARS = 
        "0123456789abcdefghijklmnopqrstuvwxyz";

    public static string GeneratePassword( int length, int seed )
    {
        var sb  = new System.Text.StringBuilder( length );
        var r   = new System.Random(seed);

        for ( int i = 0; i < length; i++ )
        {
            int     pos = r.Next( PASSWORD_CHARS.Length );
            char    c   = PASSWORD_CHARS[ pos ];
            sb.Append( c );
        }

        return sb.ToString();
    }
}
