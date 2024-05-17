using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningPlatformManager : MonoBehaviour
{
    [SerializeField]
    private float _spinningSpeed;
    [SerializeField]
    private Vector3 _rotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // transform.Rotate(0f, _spinningSpeed * Time.deltaTime, 0f, Space.Self);
        this.transform.Rotate(_rotation * _spinningSpeed * Time.deltaTime);
    }
}
