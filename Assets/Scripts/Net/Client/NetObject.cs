//---------------------------------------------
//            Network
//---------------------------------------------

using System.IO;
using System.Reflection;
using UnityEngine;
using Net;

/// <summary>
/// Esta classe tem na prática a mesma função do "Network View" da unity.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("Net/Network Object")]
public sealed class NetObject : MonoBehaviour
{
    /// <summary>
    /// Uma remote function call não pode ser executada assim que ela é guardada,
    /// só será executada quando o object ID apropriado for adicionado.
    /// </summary>

    class DelayedCall
    {
        public uint objID;
        public byte funcID;
        public string funcName;
        public object[] parameters;
    }

    /// <summary>
    /// Remote function call consiste em chamar um metodo em algum objeto (Como o MonoBehavior)
    /// Este metodo pode ou não ter um RFC ID especificado (INT). Se houver um ID especifico, vai
    /// enviar menos dados através da rede, em vez de enviar o nome da função.
    /// </summary>

    struct CachedRFC
    {
        public byte rfcID;
        public object obj;
        public MethodInfo func;
    }

    // Lista dos network objs para percorrer
    static NetList<NetObject> mList = new NetList<NetObject>();

    // Lista dos network objs para uma rápida verificação
    static System.Collections.Generic.Dictionary<uint, NetObject> mDictionary =
        new System.Collections.Generic.Dictionary<uint, NetObject>();

    // Lista das chamadas com delay - chamadas que nao pode ser executadas no mesmo instante da chamada
    static NetList<DelayedCall> mDelayed = new NetList<DelayedCall>();

    /// <summary>
    /// Identificador unico de network. É responsavel pelas mensagens chegarem no destino correto
    /// O ID era pra sesr um 'uint', mas a Unity não é capaz de serializar 'uint' types.
    /// </summary>

    [SerializeField]
    int id = 0;

    /// <summary>
    /// Identificado único do objeto.
    /// </summary>

    public uint uid
    {
        get
        {
            return (mParent != null) ? mParent.uid : (uint)id;
        }
        set
        {
            if (mParent != null) mParent.uid = value;
            else id = (int)(value & 0xFFFFFF);
        }
    }

    /// <summary>
    /// Quando setado para 'true', isso vai fazer com que a lista de RFC ser reconstruída na proxima vez que for necessário.
    /// </summary>

    [HideInInspector]
    public bool rebuildMethodList = true;

    // Cached RFC functions
    NetList<CachedRFC> mRFCs = new NetList<CachedRFC>();

    // Se o objeto foi registrado nas listas
    bool mIsRegistered = false;

    // Child objects não tem seu próprio ID, então se tem um parent NetObject este vai ser quem vai receber os eventos.
    NetObject mParent = null;


    /// <summary>
    /// Vazio.
    /// </summary>

    void Awake() { }


    /// <summary>
    /// Retorna o Network Object pelo ID.
    /// </summary>

    static public NetObject Find(uint tnID)
    {
        if (mDictionary == null) return null;
        NetObject tno = null;
        mDictionary.TryGetValue(tnID, out tno);
        return tno;
    }

    /// <summary>
    /// Retorna o hierarchy do game object em um formato de leitura.
    /// </summary>

    static public string GetHierarchy(GameObject obj)
    {
        string path = obj.name;

        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return "\"" + path + "\"";
    }

#if UNITY_EDITOR
    // Ultimo ID usado
    static uint mLastID = 0;

    /// <summary>
    /// Obtem um novo ID único.
    /// </summary>

    static uint GetUniqueID()
    {
        NetObject[] tns = (NetObject[])FindObjectsOfType(typeof(NetObject));

        for (int i = 0, imax = tns.Length; i < imax; ++i)
        {
            NetObject ts = tns[i];
            if (ts != null && ts.uid > mLastID && ts.uid < 32768) mLastID = ts.uid;
        }
        return ++mLastID;
    }

    /// <summary>
    /// Confirma se o ID é realmete único.
    /// </summary>

    void UniqueCheck()
    {
        if (id < 0) id = -id;
        NetObject tobj = Find(uid);

        if (id == 0 || tobj != null)
        {
            if (Application.isPlaying && NetManager.isInChannel)
            {
                if (tobj != null)
                {
                    Debug.LogError("Network ID " + id + " ja está em uso por " +
                        GetHierarchy(tobj.gameObject) +
                        ".\nVerifique se os network IDs dos NetObjects são unicos.\nSe preferir use a opcao 'Component>Net>Normalizar IDs' para resolver o problema", this);
                }
                else
                {
                    Debug.LogError("Network ID 0 já está em uso por " + GetHierarchy(gameObject) +
                        "\nVerifique se um ID diferente de zero foi dado a todos os NetObjects.\nSe preferir use a opcao 'Component>Net>Normalizar IDs' para resolver o problema", this);
                }
            }
            uid = GetUniqueID();
        }
    }

    /// <summary>
    /// Geralmente acontece quando a script é recompilada.
    /// Quando isso acontece, as variaveis estaticas são apagadas, e a lista de objetos é reconstruída.
    /// </summary>

    void OnEnable()
    {
        if (!Application.isPlaying && id != 0)
        {
            Unregister();
            Register();
        }
    }
#endif // Fim do Unity_Editor

    /// <summary>
    /// Retorna o NetObject de um game object especifico ou algum dos seu parents.
    /// </summary>

    static NetObject FindParent(Transform t)
    {
        while (t != null)
        {
            NetObject tno = t.gameObject.GetComponent<NetObject>();
            if (tno != null) return tno;
            t = t.parent;
        }
        return null;
    }

    /// <summary>
    /// Registra o objeto na lista.
    /// </summary>

    void Start()
    {
        if (id == 0) mParent = FindParent(transform.parent);
        Register();

        // Se tem alguma chamada de função em delay executa agora.
        for (int i = 0; i < mDelayed.size; )
        {
            DelayedCall dc = mDelayed[i];

            if (dc.objID == uid)
            {
                if (!string.IsNullOrEmpty(dc.funcName)) Execute(dc.funcName, dc.parameters);
                else Execute(dc.funcID, dc.parameters);
                mDelayed.RemoveAt(i);
                continue;
            }
            ++i;
        }
    }

    /// <summary>
    /// Remove o objeto da lista.
    /// </summary>

    void OnDestroy() { Unregister(); }

    /// <summary>
    /// Registra o network object nas listas.
    /// </summary>

    public void Register()
    {
        if (!mIsRegistered && uid != 0 && mParent == null)
        {
#if UNITY_EDITOR
            UniqueCheck();
#endif
            mDictionary[uid] = this;
            mList.Add(this);
            mIsRegistered = true;
        }
    }

    /// <summary>
    /// Remove o registro do network object.
    /// </summary>

    void Unregister()
    {
        if (mIsRegistered)
        {
            if (mDictionary != null) mDictionary.Remove(uid);
            if (mList != null) mList.Remove(this);
            mIsRegistered = false;
        }
    }

    /// <summary>
    /// Chamada a funcao a partir do seu ID.
    /// </summary>

    public bool Execute(byte funcID, params object[] parameters)
    {
        if (mParent != null) return mParent.Execute(funcID, parameters);
        if (rebuildMethodList) RebuildMethodList();
        bool retVal = false;

        for (int i = 0; i < mRFCs.size; ++i)
        {
            CachedRFC ent = mRFCs[i];

            if (ent.rfcID == funcID)
            {
                retVal = true;
#if UNITY_EDITOR
                try
                {
                    ParameterInfo[] infos = ent.func.GetParameters();

                    if (infos.Length == 1 && infos[0].ParameterType == typeof(object[]))
                    {
                        ent.func.Invoke(ent.obj, new object[] { parameters });
                    }
                    else
                    {
                        ent.func.Invoke(ent.obj, parameters);
                    }
                }
                catch (System.Exception ex)
                {
                    string types = "";

                    if (parameters != null)
                    {
                        for (int b = 0; b < parameters.Length; ++b)
                        {
                            if (b != 0) types += ", ";
                            types += parameters[b].GetType().ToString();
                        }
                    }
                    Debug.LogError(ex.Message + "\n" + ent.obj.GetType() + "." + ent.func.Name + " (" + types + ")");
                }
#else
				ParameterInfo[] infos = ent.func.GetParameters();

				if (infos.Length == 1 && infos[0].ParameterType == typeof(object[]))
				{
					ent.func.Invoke(ent.obj, new object[] { parameters });
				}
				else
				{
					ent.func.Invoke(ent.obj, parameters);
				}
#endif
            }
        }
        return retVal;
    }

    /// <summary>
    /// Chama a função a partir do seu nome.
    /// </summary>

    public bool Execute(string funcName, params object[] parameters)
    {
        if (mParent != null) return mParent.Execute(funcName, parameters);
        //if (rebuildMethodList) 
            RebuildMethodList();

        bool retVal = false;

        for (int i = 0; i < mRFCs.size; ++i)
        {
            CachedRFC ent = mRFCs[i];

            if (ent.func.Name == funcName)
            {
                retVal = true;
#if UNITY_EDITOR
                try
                {
                    ent.func.Invoke(ent.obj, parameters);
                }
                catch (System.Exception ex)
                {
                    string types = "";

                    for (int b = 0; b < parameters.Length; ++b)
                    {
                        if (b != 0) types += ", ";
                        types += parameters[b].GetType().ToString();
                    }
                    Debug.LogError(ex.Message + "\n" + ent.obj.GetType() + "." + ent.func.Name + " (" + types + ")");
                }
#else
				ent.func.Invoke(ent.obj, parameters);
#endif
            }
        }
        return retVal;
    }

    /// <summary>
    /// Procura e executa a função a partir do ID. É improvável que irá precisar chamar essa função
    /// </summary>

    static public void FindAndExecute(uint objID, byte funcID, params object[] parameters)
    {
        NetObject obj = NetObject.Find(objID);

        if (obj != null)
        {
            if (!obj.Execute(funcID, parameters))
            {
#if UNITY_EDITOR
                Debug.LogError("Nao foi possivel executar a funcao com o ID '" + funcID + "'. Verifique se existe uma funcao que possa receber esta chamada.", obj.gameObject);
#endif
            }
        }
        else if (NetManager.isInChannel)
        {
            DelayedCall dc = new DelayedCall();
            dc.objID = objID;
            dc.funcID = funcID;
            dc.parameters = parameters;
            mDelayed.Add(dc);
        }
#if UNITY_EDITOR
        else Debug.LogError("Tentando executar a funcao " + funcID + " no NetObject #" + objID +
            " ante dele ser criado.\nVerifique se o componente NetObject esta adicionado ao game object.");
#endif
    }

    /// <summary>
    /// Procura e executa a função a partir do nome. É improvável que irá precisar chamar essa função
    /// </summary>

    static public void FindAndExecute(uint objID, string funcName, params object[] parameters)
    {
        NetObject obj = NetObject.Find(objID);

        if (obj != null)
        {
            if (!obj.Execute(funcName, parameters))
            {
#if UNITY_EDITOR
                Debug.LogError("Nao foi possivel executar a funcao '" + funcName + "'. Verifique se talvez voce nao esqueceu um prefixo [RFC]", obj.gameObject);
#endif
            }
        }
        else if (NetManager.isInChannel)
        {
            DelayedCall dc = new DelayedCall();
            dc.objID = objID;
            dc.funcName = funcName;
            dc.parameters = parameters;
            mDelayed.Add(dc);
        }
#if UNITY_EDITOR
        else Debug.LogError("Tentando executar a funcao '" + funcName + "' no NetObject #" + objID +
            " ante dele ser criado.\nVerifique se o componente NetObject esta adicionado ao game object.");
#endif
    }

    /// <summary>
    /// Reconstroi a lista dos RFC calls conhecidos.
    /// </summary>

    void RebuildMethodList()
    {
        rebuildMethodList = false;
        mRFCs.Clear();
        MonoBehaviour[] mbs = GetComponentsInChildren<MonoBehaviour>();

        for (int i = 0, imax = mbs.Length; i < imax; ++i)
        {
            MonoBehaviour mb = mbs[i];
            System.Type type = mb.GetType();

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            for (int b = 0; b < methods.Length; ++b)
            {
                if (methods[b].IsDefined(typeof(RFC), true))
                {
                    CachedRFC ent = new CachedRFC();
                    ent.obj = mb;
                    ent.func = methods[b];

                    RFC tnc = (RFC)ent.func.GetCustomAttributes(typeof(RFC), true)[0];
                    ent.rfcID = tnc.Id;
                    mRFCs.Add(ent);
                }
            }
        }
    }

    /// <summary>
    /// Envia uma remote funcion call a partir do ID.
    /// </summary>

    public void Send(byte rfcID, Target target, params object[] objs) { SendRFC(uid, rfcID, null, target, true, objs); }

    /// <summary>
    /// Envia uma remote funcion calla partir do nome da função.
    /// </summary>

    public void Send(string rfcName, Target target, params object[] objs) { SendRFC(uid, 0, rfcName, target, true, objs); }

    /// <summary>
    /// Envia uma remote funcion call para um Player especifico a partir do ID.
    /// </summary>

    public void Send(byte rfcID, Player target, params object[] objs) { SendRFC(uid, rfcID, null, target, true, objs); }

    /// <summary>
    /// Envia uma remote funcion call para um Player especifico a partir do nome da função.
    /// </summary>

    public void Send(string rfcName, Player target, params object[] objs) { SendRFC(uid, 0, rfcName, target, true, objs); }

    /// <summary>
    /// Envia uma remote function call a partir do ID via UDP (se possivel).
    /// </summary>

    public void SendQuickly(byte rfcID, Target target, params object[] objs) { SendRFC(uid, rfcID, null, target, false, objs); }

    /// <summary>
    /// Envia uma remote function call a partir do nome da função via UDP (se possivel).
    /// </summary>

    public void SendQuickly(string rfcName, Target target, params object[] objs) { SendRFC(uid, 0, rfcName, target, false, objs); }

    /// <summary>
    /// Envia uma remote function call a partir do ID para um Player especifico via UDP (se possivel).
    /// </summary>

    public void SendQuickly(byte rfcID, Player target, params object[] objs) { SendRFC(uid, rfcID, null, target, false, objs); }

    /// <summary>
    /// Envia uma remote function call a partir do nome da função para um Player especifico via UDP (se possivel).
    /// </summary>

    public void SendQuickly(string rfcName, Player target, params object[] objs) { SendRFC(uid, 0, rfcName, target, false, objs); }

    /// <summary>
    /// Envia um broadcast para toda a LAN a partir do ID. Não precisa de uma conexão ativa.
    /// </summary>

    public void BroadcastToLAN(int port, byte rfcID, params object[] objs) { BroadcastToLAN(port, uid, rfcID, null, objs); }

    /// <summary>
    /// Envia um broadcast para toda a LAN a partir do nome da função. Não precisa de uma conexão ativa.
    /// </summary>

    public void BroadcastToLAN(int port, string rfcName, params object[] objs) { BroadcastToLAN(port, uid, 0, rfcName, objs); }


    /// <summary>
    /// Converte um object e os seus RFC IDs para um UINT.
    /// </summary>

    static uint GetUID(uint objID, byte rfcID)
    {
        return (objID << 8) | rfcID;
    }

    /// <summary>
    /// Decodifica um object ID e seus RFC IDs para um UINT.
    /// </summary>

    static public void DecodeUID(uint uid, out uint objID, out byte rfcID)
    {
        rfcID = (byte)(uid & 0xFF);
        objID = (uid >> 8);
    }

    /// <summary>
    /// Envia uma chamada RFC para um target especifico.
    /// </summary>

    static void SendRFC(uint objID, byte rfcID, string rfcName, Target target, bool reliable, params object[] objs)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        bool executeLocally = false;
        if (target == Target.Host && NetManager.isHosting)
        {
            // Se é o host, e o pacote deve ir para o host, apenas executa no local
            executeLocally = true;
        }
        else if (NetManager.isInChannel)
        {
            // Executa os pacotes UDP-based localmente em vez de passar pelo servidor
            if (!reliable)
            {
                if (target == Target.All)
                {
                    target = Target.Others;
                    executeLocally = true;
                }
            }

            byte packetID = (byte)((int)Packet.ForwardToAll + (int)target);
            BinaryWriter writer = NetManager.BeginSend(packetID);
            writer.Write(GetUID(objID, rfcID));
            if (rfcID == 0) writer.Write(rfcName);
            UnityTools.Write(writer, objs);
            NetManager.EndSend(reliable);
        }
        else if (!NetManager.isConnected && (target == Target.All))
        {
            // Pacote offline
            executeLocally = true;
        }

        if (executeLocally)
        {
            if (rfcID != 0)
            {
                NetObject.FindAndExecute(objID, rfcID, objs);
            }
            else
            {
                NetObject.FindAndExecute(objID, rfcName, objs);
            }
        }
    }

    /// <summary>
    /// Envia uma chamada RFC para um Player especifico.
    /// </summary>

    static void SendRFC(uint objID, byte rfcID, string rfcName, Player target, bool reliable, params object[] objs)
    {
        if (NetManager.isInChannel)
        {
            BinaryWriter writer = NetManager.BeginSend(Packet.ForwardToPlayer);
            writer.Write(target.id);
            writer.Write(GetUID(objID, rfcID));
            if (rfcID == 0) writer.Write(rfcName);
            UnityTools.Write(writer, objs);
            NetManager.EndSend(reliable);
        }
    }

    /// <summary>
    /// Broadcast uma RFC para todos os jogadores na rede.
    /// </summary>

    static void BroadcastToLAN(int port, uint objID, byte rfcID, string rfcName, params object[] objs)
    {
        BinaryWriter writer = NetManager.BeginSend(Packet.ForwardToAll);
        writer.Write(GetUID(objID, rfcID));
        if (rfcID == 0) writer.Write(rfcName);
        UnityTools.Write(writer, objs);
        NetManager.EndSend(port);
    }
}
