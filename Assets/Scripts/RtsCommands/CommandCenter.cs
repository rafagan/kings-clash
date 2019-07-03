using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandCenter : MonoBehaviour {
	public static CommandCenter Manager;	//SINGLETON
	
	private List<Mail> CommandsToExecute;
	
	void Awake () {
		if (Manager != null)
			Destroy(Manager.gameObject);
		Manager = this;
	}
	
	void Start () {
		CommandsToExecute = new List<Mail>();
	}
	
	void FixedUpdate () {
		ExecuteCommands();
	}
	
	public void AddCommands (Mail mail) {
		if (mail != null) {
			CommandsToExecute.Add(mail);
		}
	}   

    private void ExecuteCommands()
    {
		if (CommandsToExecute.Count > 0) {
            BaseUnit _ownerUnit = PoolsManager.Manager.GetUnitByID(CommandsToExecute[0].OwnerID);
            if (_ownerUnit != null) {
                int _commandID = CommandsToExecute[0].CommandID;
                if (_ownerUnit.GetUnitComponent<CommandsComponent>() == null)
                    Debug.Log("CommandList Nulo");
                var _command = _ownerUnit.GetUnitComponent<CommandsComponent>().GetCommand(_commandID);
                if (_command != null) {
                    _command.Execute(CommandsToExecute[0]);
                    CommandsToExecute.RemoveAt(0);
                } else
                    Debug.Log("Mail subject nao existe " + CommandsToExecute[0].Subject +  " ID: " + CommandsToExecute[0].CommandID);
            }
            else
                Debug.Log("Unidade não encontrada!");
		}
	}
}