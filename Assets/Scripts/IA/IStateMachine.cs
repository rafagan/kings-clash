using UnityEngine;
using System.Collections;

public enum IA_Messages {
    MOVING, DYING,
    CRUDE_GATHERING_PLASMO, NODE_GATHERING_PLASMO, CRUDE_GATHERING_TREE, GATHERING_LOGGED_TREE, STOCK,
    MOVE_TO_ATTACK, BUILD, REPAIR, IDLE, OFFENSIVE, DEFENSIVE, PASSIVE
}

public interface IStateMachine {
    void SendMessageToState(IA_Messages iaMessage);
    MonoBehaviour GetMeAsAComponent();
    IState<BaseUnit> CurrentState();
}
