//------------------------------------------
//            Network
//------------------------------------------

using UnityEngine;
using Net;
using System.IO;
using System.Collections;

[ExecuteInEditMode]
public class MenuManager : MonoBehaviour
{
	const float buttonWidth = 150f;
	const float buttonHeight = 40f;

	public GUIStyle button;
	public GUIStyle text;
	public GUIStyle input;

	string mAddress = "127.0.0.1";
	string mMessage = "";
	float mAlpha = 0f;

	/// <summary>
	/// Faz o deslizamento da tela de lista de serers
	/// </summary>

	void Update ()
	{
		if (Application.isPlaying)
		{
			float target = (NetManager.serverList.size == 0) ? 0f : 1f;
			mAlpha = UnityTools.SpringLerp(mAlpha, target, 8f, Time.deltaTime);
		}
	}

	/// <summary>
	/// GUI
	/// </summary>

	void OnGUI ()
	{
		if (!NetManager.isConnected)
		{
            DrawConnectMenu();
		}
		else
        {
            if (NetManager.isInChannel)
            {
                DrawExampleMenu();
            }
			DrawStopGameButton();
			DrawDebugInfo();

		}
	}

	/// <summary>
	/// Este menu é mostrado se o cliente nao estiver conectado ao servidor
	/// </summary>

	void DrawConnectMenu ()
    {
		Rect rect = new Rect(Screen.width * 0.5f - 200f * 0.5f - mAlpha * 120f,
			Screen.height * 0.5f - 100f, 200f, 220f);

		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.Box(UnityTools.PadRect(rect, 8f), "");
		GUI.color = Color.white;

		GUILayout.BeginArea(rect);
		{
			GUILayout.Label("Game IP", text);
			mAddress = GUILayout.TextField(mAddress, input, GUILayout.Width(200f));

			if (GUILayout.Button("Join", button))
			{
				// Conecta no destino especificado ao clicar no botao
				// "OnNetworkConnect" será chamado mais tarde se for bem sucedido
				NetManager.Connect(mAddress);
				mMessage = "Joinning...";
            }

			if (NetManager.isHosting)
			{
				GUI.backgroundColor = Color.red;

				if (GUILayout.Button("Close Game", button))
				{
					// Fecha o servidor
                    NetManager.Disconnect();
					mMessage = "Game Closed";
				}
			}
			else
			{
				GUI.backgroundColor = Color.green;

				if (GUILayout.Button("Host Game", button))
				{

                    NetUdpLobbyClient lan = GetComponent<NetUdpLobbyClient>();
                    int lobbyPort = (lan != null) ? lan.remotePort : 0;
                    NetManager.HostGame(lobbyPort, "New room", "123", 5, true);
					mMessage = "Game Hosted";
				}
			}
			GUI.backgroundColor = Color.white;

			if (!string.IsNullOrEmpty(mMessage)) GUILayout.Label(mMessage, text);
		}
		GUILayout.EndArea();

		if (mAlpha > 0.01f)
		{
			rect.x = rect.x + (Screen.width - rect.xMin - rect.xMax) * mAlpha;
			DrawServerList(rect);
		}
	}

    /// <summary>
    /// Esta função é chamada quando uma conexão foi ou não estabelecida.
    /// Conectando no servidor não significa que os jogadores conectados estarão imediatamente 
    /// capazes de comunicar um com os outros, porque ele ainda nao estao em um canal. Apenas os
    /// jogadores que estão em um canal são capazes de verem e interagirem uns com os outros.
    /// Da pra chamar o NetManager.JoinChannel aqui, mas este exemplo é só um exemplo.
	/// </summary>

    void OnNetworkConnect(bool success, string message) { mMessage = message; }

	/// <summary>
    /// Este menu é mostrado quando um jogador entrou em um canal.
	/// </summary>

	void DrawExampleMenu ()
	{
		Rect rect = new Rect(0f, Screen.height - buttonHeight, buttonWidth, buttonHeight);

		if (GUI.Button(rect, "Start Game", button))
		{
            NetManager.StartGame();
		}
	}

	/// <summary>
    /// Retorna ao menu principal se o jogador deixar o canal.
    /// Esta mensagem é enviada quando o jogador deixa um canal.
	/// </summary>

	void OnNetworkLeaveChannel ()
	{
        //Application.LoadLevel(NetManager.menuScene);
	}

	/// <summary>
    /// O botao Disconnect só é mostrado se o jogador estiver conectado.
	/// </summary>

	void DrawStopGameButton ()
	{
		Rect rect = new Rect(Screen.width - buttonWidth, Screen.height - buttonHeight, buttonWidth, buttonHeight);

		if (GUI.Button(rect, "Disconnect", button))
		{
            NetManager.Disconnect();
		}
	}

	/// <summary>
	/// Informacoes adicionais
	/// </summary>

	void DrawDebugInfo ()
    {
        GUILayout.Label("Ping: " + NetManager.ping + " (" + (NetManager.canUseUDP ? "TCP+UDP" : "TCP") + ")", text);
        GUILayout.Label("Connected: " + NetManager.isConnected, text);
        GUILayout.Label("IsHost: " + NetManager.isHosting, text);
        GUILayout.Label("ID: " + NetManager.playerID, text);
        GUILayout.Label("Name: " + NetManager.playerName, text);
        GUILayout.Label("Team: " + NetManager.playerTeamID, text);
        GUILayout.Label("House: " + NetManager.playerHouse, text);
        GUILayout.Label("Role: " + NetManager.playerRole, text);
        GUILayout.Label("Sync: " + NetManager.playerSynchronized, text);
        GUILayout.Label("Turn: " + NetManager.playerTurn, text);

	}

	/// <summary>
	/// Desenha a lista de servidores
	/// </summary>

	void DrawServerList (Rect rect)
	{
		GUI.color = new Color(1f, 1f, 1f, mAlpha * mAlpha * 0.5f);
		GUI.Box(UnityTools.PadRect(rect, 8f), "");
		GUI.color = new Color(1f, 1f, 1f, mAlpha * mAlpha);

		GUILayout.BeginArea(rect);
		{
			GUILayout.Label("Network Games", text);

			for (int i = 0; i < NetManager.serverList.size; ++i)
			{
                ServerList.Entry ent = NetManager.serverList[i];
				if (GUILayout.Button(ent.externalAddress.ToString(), button))
				{
					if(NetManager.ConnectGame(ent, "123"))
					    mMessage = "Connecting...";
                    else
                        mMessage = "Wrong password...";
				}
			}
		}
		GUILayout.EndArea();
		GUI.color = Color.white;
	}
}
