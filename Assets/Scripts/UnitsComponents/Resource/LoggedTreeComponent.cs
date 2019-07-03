using UnityEngine;
using System.Collections;

public class LoggedTreeComponent : AbstractResource {
    void Awake() {
        ResourceName = ResourceType.Tree;
    }
}
