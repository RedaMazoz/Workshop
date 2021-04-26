//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorshopElements", menuName = "Workshop/New Save", order = 1)]

public class WorkshopElementsScriptableObj : ScriptableObject
{

    public string saveName;

    [SerializeField] public List<string> stContainer = new List<string>();
    [SerializeField] public Dictionary<string, GameObject> sInactiveElements = new Dictionary<string, GameObject>();

}
