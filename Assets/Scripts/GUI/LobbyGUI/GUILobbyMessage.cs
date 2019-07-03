using UnityEngine;
using System.Collections;

public class GUILobbyMessage : MonoBehaviour {

    private static GUILobbyMessage mInstance;
    public UILabel MessageLabel;

    void Awake() {
        if (mInstance != null) {
            Destroy(gameObject);
        }
        else {
            mInstance = this;
        }
    }

    public static void SetMessage(string message) {
        if (mInstance == null)
            return;
        if (mInstance.MessageLabel != null) {
            mInstance.MessageLabel.text = message;
        }
    }


    /// <summary>
    /// Esta função é chamada quando uma conexão foi ou não estabelecida.
    /// Conectando no servidor não significa que os jogadores conectados estarão imediatamente 
    /// capazes de comunicar um com os outros, porque ele ainda nao estao em um canal. Apenas os
    /// jogadores que estão em um canal são capazes de verem e interagirem uns com os outros.
    /// Da pra chamar o NetManager.JoinChannel aqui, mas este exemplo é só um exemplo.
    /// </summary>

    void OnNetworkConnect(bool success, string message) { SetMessage(message); }

}
