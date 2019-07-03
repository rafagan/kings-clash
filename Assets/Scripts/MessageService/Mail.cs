using UnityEngine;
using System.Collections;

// CLASSE RESPONSÁVEL POR CONTER AS INFORMAÇÕES ÚTEIS DE UMA MENSAGEM. O CAMPO SUBJECT É O ASSUNTO DA MENSAGEM, E DEVE SER IGUAL À LISTA DE MENSAGENS
// DO MAILMAN. O ATRIBUTO GENERICVALUE1 E GENERICVALUE2, É UM OPCIONAL, PARA MENSAGENS QUE CARREGUEM VALORES, POR EXEMPLO A MENSAGEM DE 
// ADICIONAR PLASMO AO BANCO DO JOGADOR, CONTÉM UM VALUE1 = 1 QUE CORRESPONDE AO ID DO PLASMO, E O VALUE2 = 100 QUE CORRESPONDE A QUANTIA
// TOTAL A SER ADICIONADA.
public class Mail {
	//Mail Attributes
	public string Subject;
	public int Value1;
	public int Value2;
	//Command Attributes
	public int CommandID;
	//Owner Unit Attributes
	public int OwnerID;
	//Target Unit Attributes
	public int TargetID;
	//Position Attributes
    public Vector3 TargetPosition;
    public Vector3 DestinyPosition = Vector3.zero;
	
	public bool IsValid { get { if (this.CommandID >= 9999) return false; else return true; } }
	
	//Construtor para mensagem que envolve unidade alvo
	public Mail (string subject, int ownerID, int targetID, int value1 = 0, int value2 = 0) {
		this.Subject = subject;
		this.Value1 = value1;
		this.Value2 = value2;
		
		this.OwnerID = ownerID;
		this.TargetID = targetID;
		this.TargetPosition = Vector3.zero;
		
		SetCommandID();
	}
	
	//Construtor para mensagem que envolve posição
	public Mail (string subject, int ownerID, Vector3 targetPosition, int value1 = 0, int value2 = 0) {
		this.Subject = subject;
		this.Value1 = value1;
		this.Value2 = value2;
		
		this.OwnerID = ownerID;
		this.TargetID = 9999;
		this.TargetPosition = targetPosition;
		
		SetCommandID();
	}

    //Construtor para mensagens que envolvem posição inicial e posição final para interpolação
    public Mail(string subject, int ownerID, int targetID, Vector3 targetPosition, Vector3 destinyPosition, int value1 = 0, int value2 = 0) {
        this.Subject = subject;
        this.Value1 = value1;
        this.Value2 = value2;

        this.OwnerID = ownerID;
        this.TargetID = targetID;
        this.TargetPosition = targetPosition;
        this.DestinyPosition = destinyPosition;

        SetCommandID();
    }
	
	//Construtor para mensagem que envolve target e posição
	public Mail (string subject, int ownerID, int targetID, Vector3 targetPosition, int value1 = 0, int value2 = 0) {
		this.Subject = subject;
		this.Value1 = value1;
		this.Value2 = value2;
		
		this.OwnerID = ownerID;
		this.TargetID = targetID;
		this.TargetPosition = targetPosition;
		
		SetCommandID();
	}
		
	//Método responsável por encontrar o ID do Command
	private void SetCommandID () {

		if (MailMan.Post.SupportedSubjects.Count > 0) {
			foreach (string _subject in MailMan.Post.SupportedSubjects) {
				if (this.Subject == _subject) {
					this.CommandID = MailMan.Post.SupportedSubjects.IndexOf(_subject);
					return;
				}
			}
		} 
		Debug.Log("Mail subject nao existe " +Subject);
		this.CommandID = 9999;
	}
}

