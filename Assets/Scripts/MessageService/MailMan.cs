using UnityEngine;
using System.Collections.Generic;
using Net;

// CLASSE RESPONSÁVEL POR RECEBER TODAS OS COMANDOS DO JOGO POR MEIO DE "MAILs", E ENVIA-LOS PARA O SERVER ONLINE OU OFFLINE
public class MailMan : NetBehaviour {
	public static MailMan Post;	//SINGLETON
	public bool DebugMode;
	
	//PUBLIC ATTRIBUTES 
	public List<string> SupportedSubjects;

    //PRIVATE ATTRIBUTES
    private static List<Mail> _mailsRecorded; //Mensagens gravadas para serem executadas ao final do turno

	void Awake () {
		if (Post != null)
			Destroy(Post.gameObject);
        else
		    Post = this;

        _mailsRecorded = new List<Mail>();
    }

    public void Reset() {
        _mailsRecorded.Clear();
    }
	
	public void NewMail (Mail mail) {
        if (mail != null)
        {
            if (DebugMode) Debug.Log("Enviando: " + mail.Subject);
            if (NetManager.isConnected)
            {
                Package.Send(1, Target.All, mail.CommandID, mail.OwnerID, mail.TargetID, mail.TargetPosition, mail.DestinyPosition, mail.Value1, mail.Value2);
            }
            else
            {
                CommandCenter.Manager.AddCommands(mail);
            }
        }
        else if (DebugMode) Debug.Log("Mensagem invalida");	
	}

	//Processa e encaminha Mails para serem executados
	static public void ProcessRecordedMails() {
        foreach (Mail m in _mailsRecorded)
        {
            CommandCenter.Manager.AddCommands(m);
        }
        _mailsRecorded.Clear();
	}

    [RFC(1)] // ID 1 refere-se a chamada de action pelo sistema de rfcs
    //Processa os mails recebidos dos outros jogadores
    private void ProcessMailReceived(int commandId, int ownerId, int targetId, Vector3 targetPosition, Vector3 destinyPosition, int value1, int value2)
    {
        _mailsRecorded.Add(new Mail(SupportedSubjects[commandId], ownerId, targetId, targetPosition, destinyPosition, value1, value2));
    }

}