using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public void EnableCollision(GameObject SlicingPlane, GameObject body_model)
    {
        SlicingPlane.GetComponent<BoxCollider>().enabled = true;
        body_model.GetComponent<MeshCollider>().enabled = true;
    }

    public void DisableCollision(GameObject SlicingPlane, GameObject body_model)
    {
        SlicingPlane.GetComponent<BoxCollider>().enabled = false;
        body_model.GetComponent<MeshCollider>().enabled = false;
    }
}
