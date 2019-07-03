using System.Collections.Generic;
using Net;
using UnityEngine;
using System.Collections;

public class HUDQueueRoot : MonoBehaviour {

    public GameObject QueuePrefab;

    private UITable _table;
    private int trainingCount = 0;
    private ProducerComponent selectedProducerUnit;
    private List<GameObject> queueButtonsList;
    //private List<Pool> lastProducerList;

	void Start () {
        _table = GetComponent<UITable>();
        if (_table == null || QueuePrefab == null) {
            Debug.LogError("Não foi encontrado o componente UITable ou não foi referenciado o prefab no game object HUDQueueButtons");
            Destroy(this);
            return;
        }
        queueButtonsList = new List<GameObject>();
        //lastProducerList = new List<Pool>();
	}
	
    void Update() {
        //Seleciona o producer;
        SelectProducer();

        //Cria os botões
        CreateQueueButtons();
    }

    private void CreateQueueButtons()
    {
        if ((selectedProducerUnit != null && selectedProducerUnit.ListHasBeenChanged) || 
            (selectedProducerUnit != null && queueButtonsList.Count != selectedProducerUnit.TrainingQueue.Count))
        {
            //Reseta o flag
            selectedProducerUnit.ListHasBeenChanged = false;

            //Apaga os botões atuais
            ClearButtons();

            //Cria os novos botões
            for (int i = 0; i < trainingCount; i++)
            {
                var queueButton = NGUITools.AddChild(gameObject, QueuePrefab);
                var buttonScript = queueButton.GetComponent<GUIButtonQueue>();
                buttonScript.index = i;
                buttonScript.producer = selectedProducerUnit;
                buttonScript.Pool = selectedProducerUnit.unitTrainQueue[i];
                if (i == 0)
                    buttonScript.DontShowProgress = false;
                queueButtonsList.Add(queueButton);
                _table.Reposition();
                _table.repositionNow = true;
            }
        }
        else if (selectedProducerUnit == null && queueButtonsList.Count > 0)
        {
            ClearButtons();
        }
    }

    private void ClearButtons()
    {
        if (queueButtonsList.Count > 0)
        {
            for (int i = queueButtonsList.Count - 1; i >= 0; i--)
            {
                var _buttonToDestroy = queueButtonsList[i];
                queueButtonsList.RemoveAt(i);
                Destroy(_buttonToDestroy);
            }
        }
    }

    private void SelectProducer()
    {
        BaseUnit _selectedUnit = InterfaceManager.GetSelectedBaseUnit();
        if (_selectedUnit == null)
        {
            trainingCount = 0;
            selectedProducerUnit = null;
            return;
        }
        else if (selectedProducerUnit == null || selectedProducerUnit != _selectedUnit)
        {

            var _producerComponent = _selectedUnit.GetUnitComponent<ProducerComponent>();
            if (_producerComponent != null) {
                selectedProducerUnit = _producerComponent;
                trainingCount = selectedProducerUnit.TrainingQueue.Count;
            }
        }
    }
}
