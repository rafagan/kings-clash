using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandsComponent : AbstractUnitComponent {
	private Transform commandsContainer;

	public void Start () {
		commandsContainer = transform.Find("Commands");
	}

	public AbstractCommand GetCommand(int commandID) {
		if (commandsContainer != null) {
			var _commands = commandsContainer.GetComponents<AbstractCommand>();
			if (_commands.Length > 0) {
				foreach (AbstractCommand _command in _commands) {
					if (_command.CommandID == commandID)
						return _command;
				}
			}
		}
		return null;
	}
	
	 public override void GUIPriority() {
	 
	 }
	 
	 public override void UserInputPriority() {
	
	}
	
	public override void Reset() {}
}
