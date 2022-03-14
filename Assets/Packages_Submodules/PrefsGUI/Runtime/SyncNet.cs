﻿using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace PrefsGUI
{
    public class SyncNet
    {
        public static bool isServer { get { return NetworkServer.active; } }
        public static bool isClient { get { return NetworkClient.active; } }
        public static bool isHost { get { return isServer && isClient; } }
        public static bool isStandAlone { get { return !isServer && !isClient; } }

        public static bool isServerOrStandAlone { get { return isServer || !isClient; } }

        // Not Server but Client.
        // warn Host: isServer == isClient == true
        public static bool isSlave { get { return !isServerOrStandAlone; } }

        public static bool isActive { get { var nm = NetworkManager.singleton; return ( nm != null ) && nm.isNetworkActive; } }

        public static NetworkClient client { get { var nm = NetworkManager.singleton; return ( nm == null ) ? null : nm.client; } }


        public static void Spawn( GameObject go )
        {
            if ( isServer ) NetworkServer.Spawn( go );
        }

        public static void Destroy( GameObject go )
        {
            if ( isServer ) NetworkServer.Destroy( go );
            if ( isServerOrStandAlone ) Object.Destroy( go );
        }
    } // end class
} // end namespace