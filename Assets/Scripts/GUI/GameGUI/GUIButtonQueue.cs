using UnityEngine;

public class GUIButtonQueue : MonoBehaviour {

    private UILabel Percent;
    private UILabel Unit;

    public Pool Pool;
    public ProducerComponent producer;
    public bool DontShowProgress = true;
    public int index;

    void Start() {
        Percent = transform.Find("Percent").GetComponent<UILabel>();
        Unit = transform.Find("Unit").GetComponent<UILabel>();
        if (Percent == null || Unit == null)
            Destroy(gameObject);
    }


    void Update() {
        if (InterfaceManager.GetSelectedBaseUnit() == null || !producer.unitTrainQueue.Contains(Pool)) {
            Destroy(gameObject);
            return;
        }
        Unit.text = Pool.Prototype.name;
        if (DontShowProgress == false && producer.unitTrainQueue[0] == Pool) {
            Percent.text = producer.CalculateProgress().ToString() + "%";
        }
        else
        {
            Percent.text = "In Queue";
        }
    }


    void OnClick() {
        producer.CancelTraining(index);
        /*
        if (_ability != null) {
            InterfaceManager.Manager.SetAbilityClicked(_ability);
        }
        if (_pool != null) {
            if (_pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.CHARACTER)
                InterfaceManager.GetSelectedBaseUnit().GetUnitComponent<ProducerComponent>().TrainUnit(_pool.PoolUniqueID);
            else if (_pool.Prototype.GetComponent<AttributesComponent>().UnitType == ObjectType.STRUCTURE)
                _pool.PreviewStructure.Spawn(InterfaceManager.GetSelectedBaseUnit());
        }*/
    }

}
