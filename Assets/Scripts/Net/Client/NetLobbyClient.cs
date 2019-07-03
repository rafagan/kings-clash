//---------------------------------------------
//             Network
//---------------------------------------------

using UnityEngine;
using Net;

/// <summary>
/// Server Lobby Client é uma classe abstrada desenvolvida para se comunicar com o Lobby Server
/// Voce deve instanciar NetUdpLobbyClient que é especifico para protocolo UDP
/// </summary>

public abstract class NetLobbyClient : MonoBehaviour
{
    public delegate void OnListChange();

    /// <summary>
    /// Lista dos servidores conhecidos.
    /// </summary>

    static public ServerList knownServers = new ServerList();

    /// <summary>
    /// Callback that will be triggered every time the server list changes.
    /// </summary>

    static public OnListChange onChange;

    /// <summary>
    /// Whether some lobby client is currently active.
    /// </summary>

    static public bool isActive = false;

    /// <summary>
    /// Clear the list of known servers when the component is disabled.
    /// </summary>

    protected virtual void OnDisable() { knownServers.Clear(); }
}
