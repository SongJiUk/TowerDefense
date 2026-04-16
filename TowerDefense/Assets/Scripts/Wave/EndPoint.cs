using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    void OnEnable() => Managers.EndPoint = this;
    void OnDisable() => Managers.EndPoint = null;
}
