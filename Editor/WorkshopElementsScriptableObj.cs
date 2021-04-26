//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorshopElements", menuName = "Workshop/New Save", order = 1)]

public class WorkshopElementsScriptableObj : ScriptableObject
{

    public string saveName;

    public List<string> stContainer = new List<string>();
    public Dictionary<string, GameObject> sInactiveElements = new Dictionary<string, GameObject>();

}
