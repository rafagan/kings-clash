using UnityEngine;
using System.Collections;

//NO COMANDO DESPAWN, O ID DO POOL DESEJADO ESTÁ NO VALUE1, E A UNIDADE A SER DESPAWNADA ESTÁ NO TARGETUNITID
public class DespawnCommand : AbstractCommand {

	void Start () {
		CommandID = 4; //ATENÇÃO: DEVE-SE DEFINIR MANUALMENTE A ID DO COMMAND! AS IDs SÃO OS ÍNDICES LOCALIZADOS NO MAILMAN!!!
	}

	#region implemented abstract members of AbstractCommand

	public override void Execute (Mail mail)
	{
		var pool = PoolsManager.Manager.GetPoolByID(mail.Value1);

	    if (pool == null) return;
        var item = PoolsManager.Manager.GetItemByID(mail.TargetID);
        pool.Despawn(item);
	}

	public override IEnumerator CommandRoutine () { throw new System.NotImplementedException (); }

	#endregion


}
