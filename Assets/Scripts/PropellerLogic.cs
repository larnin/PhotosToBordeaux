using UnityEngine;
using System.Collections;

public class PropellerLogic : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1;
    
    void Update()
    {
        var rot = transform.localRotation.eulerAngles;
        rot.z += rotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(rot);
    }
}
