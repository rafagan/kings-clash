using UnityEngine;
using System.Collections;

public interface IState<in T> {
    string Name { get; set; }

    void Enter();
	IState<T> Process();
    IState<T> FixedProcess(); 
	void Leave();
    void ReceiveMessage(IA_Messages iaMessage);
}
