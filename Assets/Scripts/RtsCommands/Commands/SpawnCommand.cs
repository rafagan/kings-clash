using UnityEngine;
using System.Collections;

//NO COMANDO SPAWN, A ID DO POOLMANAGER REQUIRIDO ESTÁ NO TARGETID DO MAIL E O TEAMID NO VALUE1
public class SpawnCommand : AbstractCommand {	
	private PoolsManager poolsManager;

	void Start () {
		CommandID = 3; //ATENÇÃO: DEVE-SE DEFINIR MANUALMENTE A ID DO COMMAND! AS IDs SÃO OS ÍNDICES LOCALIZADOS NO MAILMAN!!!

		poolsManager = GameObject.Find("POOLMANAGERS").GetComponent<PoolsManager>();
		if (poolsManager == null) {
			Debug.Log("PoolsManager não encontrado: " + transform.name);
			enabled = false;
		}
	}

	#region implemented abstract members of AbstractCommand

	public override void Execute (Mail mail)
	{
		Pool _pool = poolsManager.GetPoolByID(mail.TargetID);
		
		if (_pool != null) {
			bool _isEnemy = false;
			var _teamID = mail.Value1;
			if (_teamID < 99 && PlayerManager.Player.PlayerTeam != _teamID)
				_isEnemy = true;
			
			var spawnedUnit = PoolsManager.Manager.LastSpawnedUnit = _pool.Spawn(mail.TargetPosition, _teamID, _isEnemy).OwnerUnit;

            //Acopla o geyser caso tenha em um barracks
		    if (spawnedUnit.UnitClass == Unit.Barrack && mail.Value2 >= 0)
		    {
                Debug.Log("Acoplando geyser");
		        var _geyserPool = poolsManager.GetPoolByID(poolsManager.GetPoolIDByName("rGeyserPool"));
		        if (_geyserPool != null)
		        {
                    Debug.Log("Pool do geyser encontrado");
                    Debug.Log(mail.Value2);
		            var _geyser = _geyserPool.GetUnitByID(mail.Value2);
                    if (_geyser != null)
                    {
                        Debug.Log("Geyser encontrado");
                        var _geyserComponent = _geyser.GetUnitComponent<SteamComponent>();
                        if (_geyserComponent != null)
                            spawnedUnit.GetUnitComponent<ExtractorComponent>().AttachGeyser(_geyserComponent);
                    }
                    else
                    {
                        Debug.Log("Geyser não encontrado");
                    }
		        }
		    }
			
            if (spawnedUnit.GetUnitComponent<MobileComponent>() != null) {
				#region XUNXADA AQUI, NAO OLHE!
            	spawnedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            	spawnedUnit.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
            	#endregion
                MailMan.Post.NewMail(new Mail("Move", spawnedUnit.UniqueID, mail.DestinyPosition));
            }
		}
	}

	public override IEnumerator CommandRoutine () { throw new System.NotImplementedException (); }

	#endregion
}
