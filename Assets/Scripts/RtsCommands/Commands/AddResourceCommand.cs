using UnityEngine;
using System.Collections;

//NO COMANDO ADDRESOURCE, O ID DO TIME A SER ADICIONADO O RECURSO ESTÁ NO TARGETID, O TIPO DE RECURSO NO VALUE1, 
//E A QUANTIDADE A SER ADICIONADA ESTÁ NO VALUE 2
public class AddResourceCommand : AbstractCommand {
	void Start () {
		CommandID = 5; //ATENÇÃO: DEVE-SE DEFINIR MANUALMENTE A ID DO COMMAND! AS IDs SÃO OS ÍNDICES LOCALIZADOS NO MAILMAN!!!
	}

	#region implemented abstract members of AbstractCommand
	public override void Execute (Mail mail)
	{
		if (PlayerManager.Player.PlayerTeam == mail.TargetID) { 
			var resourceType = (ResourceType)mail.Value1;
			var resourceAmount = mail.Value2;
			
			PlayerManager.Player.PlayerResources.AddResource(resourceType, resourceAmount);
		}
	}

	public override IEnumerator CommandRoutine () { return null; }
	#endregion
}
