using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Workshop : EditorWindow
{
    [MenuItem("My Custom Widgets/Workshop")]
    public static void ShowWindow() { GetWindow<Workshop>("Workshop");}

    //std::ofstream
   

//public WorkshopElementsScriptableObj defaultSave;

    public Font myfont;
    public GUIStyle addButtonGUIStyle;
    public GUIStyle onButtonGUIStyle;
    public GUIStyle offButtonGUIStyle;
    public GUIStyle clearButtonGUIStyle;

    private static List<string> tContainer = new List<string>();
    private Dictionary <string,GameObject> inactiveElements = new Dictionary<string, GameObject>();

    private bool isAdding = false;
    private int tempInt = 0;

    public string[] saveList;
    public WorkshopElementsScriptableObj defaultSave;

    private List<string> savesNameList = new List<string>();
    private int LastIndex = 0;
    private int saveIndex = 0;

    public void Awake()
    {
        addButtonGUIStyle = new GUIStyle();
        addButtonGUIStyle.font = myfont;
        addButtonGUIStyle.normal.textColor = Color.white;
        addButtonGUIStyle.fontSize = 30;
        onButtonGUIStyle = new GUIStyle();
        onButtonGUIStyle.font = myfont;
        onButtonGUIStyle.normal.textColor = Color.white;
        onButtonGUIStyle.fontSize = 20;
        onButtonGUIStyle.fontStyle = FontStyle.Bold;
        onButtonGUIStyle.alignment = TextAnchor.MiddleCenter;
        //onButtonGUIStyle.border = new RectOffset(3, 3, 2, 2);
        offButtonGUIStyle = new GUIStyle();
        offButtonGUIStyle.font = myfont;
        offButtonGUIStyle.normal.textColor = Color.grey;
        offButtonGUIStyle.fontSize = 20;
        offButtonGUIStyle.fontStyle = FontStyle.Bold;
        offButtonGUIStyle.alignment = TextAnchor.MiddleCenter;
        //offButtonGUIStyle.border = new RectOffset(3, 3, 2, 2);
        clearButtonGUIStyle = new GUIStyle();
        clearButtonGUIStyle.font = myfont;
        clearButtonGUIStyle.normal.textColor = Color.red;
        clearButtonGUIStyle.fontSize = 16;
        //clearButtonGUIStyle.alignment = TextAnchor.MiddleCenter;

        /*if (saveList.Length == 0) { }
        else
        {
            tContainer = defaultSave.stContainer;
            inactiveElements = defaultSave.sInactiveElements;
        }
        //tContainer =*/
        saveList = AssetDatabase.FindAssets("t: WorkshopElementsScriptableObj");
        defaultSave = AssetDatabase.LoadAssetAtPath<WorkshopElementsScriptableObj>(AssetDatabase.GUIDToAssetPath(saveList[0]));
        tContainer = defaultSave.stContainer;
        inactiveElements = defaultSave.sInactiveElements;
        for(int i = 0; i < saveList.Length; i++)
        {
            savesNameList.Add(System.Text.RegularExpressions.Regex.Replace(AssetDatabase.GUIDToAssetPath(saveList[i]), "([0-9A-z ]*/)*", "").Replace(".asset", ""));
        }
        
        //for (int i = 0; i < saveList.Length ; i++) { savesNameList.Add(AssetDatabase.GUIDToAssetPath()); }

    }
    void OnDestroy()
    {   
        defaultSave.stContainer = tContainer;
        defaultSave.sInactiveElements = inactiveElements;
    }
    private void OnGUI()
    {
        if (Screen.height / 12 <= 50 && Screen.height / 12 >= 20) { addButtonGUIStyle.fontSize = Screen.height / 12; }
        
        saveIndex = EditorGUI.Popup(new Rect(45, 2, 100, 25), saveIndex, savesNameList.ToArray());
        if(saveIndex != LastIndex)
        {
            defaultSave.stContainer = tContainer;
            defaultSave.sInactiveElements = inactiveElements;
            defaultSave = AssetDatabase.LoadAssetAtPath<WorkshopElementsScriptableObj>(AssetDatabase.GUIDToAssetPath(saveList[saveIndex]));
            tContainer = defaultSave.stContainer;
            inactiveElements = defaultSave.sInactiveElements;
            LastIndex = saveIndex;
        }

        for (int i = 0; i < tContainer.Count; i++)
        {
            GUI.SetNextControlName("b" + i.ToString());
            addElement(tContainer[i], i);
        }
        if (GUI.Button(new Rect(Screen.width - 45, 2, 75, 25), "Clear", clearButtonGUIStyle))
        {
            inactiveElements.Clear();
            tContainer.Clear();
            isAdding = false;
            tempInt = 0;
        }
        if (GUI.Button(new Rect(5, 0, 10, 25), "+", addButtonGUIStyle))
        {
            tContainer.Add("");
            isAdding = true;
        }
        if (!isAdding) { dragg();}
        tempInt = (tContainer.Count == 0) ? tContainer.Count : tContainer.Count - 1;
        if (isAdding) {
            GUI.SetNextControlName("t" + (tempInt).ToString());
            tContainer[tempInt] = GUI.TextField(new Rect(10, 25 * tempInt + 30, Screen.width - 20, 20), tContainer[tempInt], 40);
            GUI.FocusControl("t" + (tempInt).ToString());
            
        }
        
        if (isAdding && Event.current.keyCode == KeyCode.Return && !(Event.current.type == EventType.KeyUp))
        {
            if (GameObject.Find(tContainer[tempInt]) == null) { tContainer.RemoveAt(tempInt);}
            else
            {
                GUI.SetNextControlName("b" + (tempInt).ToString());
                addElement(tContainer[tempInt], tempInt);
                //tContainer.RemoveAt(int.Parse(GUI.GetNameOfFocusedControl().Replace("t",""))) ;
                isAdding = false;
                Repaint();
            }
        }

    }
    void addElement(string elementName, int i) {

        if (GameObject.Find(elementName))
        {

            if (GameObject.Find(elementName).activeSelf)
            {
                if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), elementName, onButtonGUIStyle)) { Selection.activeGameObject = GameObject.Find(elementName); }
                if (GUI.Button(new Rect(Screen.width - 25, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/My Editor/Editor/x.png",typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i); }
            }
        }
        else if(inactiveElements.ContainsKey(elementName))
        {
            if (!((inactiveElements[elementName]).activeSelf))
            {
                //Debug.Log("btata");
                if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), elementName, offButtonGUIStyle)) { Selection.activeGameObject = inactiveElements[elementName] as GameObject; }
                if (GUI.Button(new Rect(Screen.width - 25, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/My Editor/Editor/x.png", typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i); }


            }
        }
        /*        if 
                {
                    if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 20, 20), elementName, offButtonGUIStyle)) { Selection.activeGameObject = inactiveElements; }
                    //inactiveDraggedElement = false;
                }
        */
    }
    //{ Selection.activeGameObject = GameObject.Find(elementName); }

    private void dragg()
    {
        EventType eventType = Event.current.type;
        if (eventType == EventType.DragUpdated ||
            eventType == EventType.DragPerform)
        {
            // Show a copy icon on the drag
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            //Debug.Log((DragAndDrop.objectReferences[0]).name);
            if (eventType == EventType.DragPerform && (DragAndDrop.objectReferences[0]).name != "")
            {
                GUI.SetNextControlName("t" + (tempInt).ToString());
                tContainer.Add((DragAndDrop.objectReferences[0]).name);
                inactiveElements[DragAndDrop.objectReferences[0].name] = (GameObject)(DragAndDrop.objectReferences[0]);
                //if (!((DragAndDrop.objectReferences[0] as GameObject).activeSelf)) { inactiveDraggedElement = true;}
                //else { inactiveDraggedElement = false;}
                //GUI.SetNextControlName("b" + (tempInt).ToString());
                //addElement(tContainer[tempInt], tempInt);
                //Repaint();
                DragAndDrop.AcceptDrag();
            }
            Event.current.Use();
        }
    }
    private void checkDragable()
    {
        GameObject ele = new GameObject();
        Event e= Event.current;
        if ((e.type == EventType.MouseDrag))
        {
            //StartDrag();

            // Use the event, else the drag won't start

            e.Use();
        }
        if ( (Event.current.type == EventType.DragUpdated) && (new Rect(10, 25 * tContainer.Count + 30, Screen.width - 20, Screen.height - (25 * tContainer.Count + 30))).Contains(Event.current.mousePosition) )
        {
            

            if (DragAndDrop.GetGenericData("GameObject") == null)
            {
                DragAndDrop.SetGenericData("GameObject", ele);
            }

            Event.current.Use();
        }

        if (Event.current.type == EventType.DragPerform
            && (Event.current.type == EventType.DragUpdated) && (new Rect(10, 25 * tContainer.Count + 30, Screen.width - 20, Screen.height - (25 * tContainer.Count + 30))).Contains(Event.current.mousePosition) )
        {
DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            tContainer[tempInt] = GUI.TextField(new Rect(10, 25 * tempInt + 30, Screen.width - 20, 20), tContainer[tempInt], 40);
            GUI.SetNextControlName("b" + (tempInt).ToString());
            addElement(tContainer[tempInt], tempInt);
            Repaint();
        }

    }
}
