using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Workshop : EditorWindow
{
    [MenuItem("My Custom Widgets/Workshop")]
    public static void ShowWindow() { GetWindow<Workshop>("Workshop");}

    public Font myfont;
    public GUIStyle addButtonGUIStyle;
    public GUIStyle onButtonGUIStyle;
    public GUIStyle offButtonGUIStyle;
    public GUIStyle clearButtonGUIStyle;

    [SerializeField] private static List<string> tContainer = new List<string>();
    [SerializeField] private Dictionary <string,GameObject> inactiveElements = new Dictionary<string, GameObject>();

    private bool isAdding = false;
    private int tempInt = 0;

    private string[] saveList;
    [SerializeField] private WorkshopElementsScriptableObj defaultSave;

    private List<string> savesNameList = new List<string>();
    private int LastIndex = 0;
    private int saveIndex = 0;

    Dictionary<string,GameObject> sceneInactiveGO = new Dictionary<string, GameObject>();

    private Vector2 scrollPosition;

    private string lastSaveGUID;

    // Bug Fix: Empty First Save after Build
    // Putting On Window Reset Loading in OnEnable instead of Awake
    private void OnEnable()
    {
        saveList = AssetDatabase.FindAssets("t: WorkshopElementsScriptableObj");
        if(saveList.Length == 0) { CreateSaveIfNot(); } //BugFix: Buttons Disappear when All Saves Deleted
        defaultSave = AssetDatabase.LoadAssetAtPath<WorkshopElementsScriptableObj>(AssetDatabase.GUIDToAssetPath(saveList[saveIndex]));

        for (int i = 0; i < saveList.Length; i++)
        {
            savesNameList.Add(System.Text.RegularExpressions.Regex.Replace(AssetDatabase.GUIDToAssetPath(saveList[i]), "([0-9A-z ]*/)*", "").Replace(".asset", ""));
        }
        tContainer = defaultSave.stContainer;

        GetSceneInactiveGameObjs(sceneInactiveGO);
        foreach(string goName in tContainer)
        {
            if(sceneInactiveGO.ContainsKey(goName) && !inactiveElements.ContainsKey(goName)) { inactiveElements[goName] = sceneInactiveGO[goName];}
        }
    }

    private void OnProjectChange()
    {
        //BugFix: No Update of SaveList when New Save Created
        if (saveList.Length != 0) { lastSaveGUID = saveList[saveIndex];}
        saveList = AssetDatabase.FindAssets("t: WorkshopElementsScriptableObj");

        saveIndex = ArrayUtility.IndexOf<string>(saveList, lastSaveGUID);
        LastIndex = saveIndex;

        savesNameList.Clear();
        for (int i = 0; i < saveList.Length; i++)
        {
            savesNameList.Add(System.Text.RegularExpressions.Regex.Replace(AssetDatabase.GUIDToAssetPath(saveList[i]), "([0-9A-z ]*/)*", "").Replace(".asset", ""));
        }
    }
    private void OnInspectorUpdate()
    {
        //BugFix: Active GameObject disappear when desactivated in Inspector after the Worshop was already Open
        //BugFix: Instant Buttons Color Change if Activated or Desactivated in Inspector
        GetSceneInactiveGameObjs(sceneInactiveGO);
        foreach (string goName in tContainer)
        {
            if (sceneInactiveGO.ContainsKey(goName) && !inactiveElements.ContainsKey(goName)) { inactiveElements[goName] = sceneInactiveGO[goName];}
        }
        Repaint();
    }

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

        offButtonGUIStyle = new GUIStyle();
        offButtonGUIStyle.font = myfont;
        offButtonGUIStyle.normal.textColor = Color.grey;
        offButtonGUIStyle.fontSize = 20;
        offButtonGUIStyle.fontStyle = FontStyle.Bold;
        offButtonGUIStyle.alignment = TextAnchor.MiddleCenter;

        clearButtonGUIStyle = new GUIStyle();
        clearButtonGUIStyle.font = myfont;
        clearButtonGUIStyle.normal.textColor = Color.red;
        clearButtonGUIStyle.fontSize = 16;

    }
    void OnDestroy()
    {   
        defaultSave.stContainer = tContainer;
        defaultSave.sInactiveElements = inactiveElements;

        //BugFix: Saving for Editor restart
        //BugFix: Saving On Compile Solution
        EditorUtility.SetDirty(defaultSave);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private void OnGUI()
    {

        
        saveIndex = EditorGUI.Popup(new Rect(45, 2, 100, 25), saveIndex, savesNameList.ToArray());
        //BugFix: Buttons Disappear when a currently open Save is Deleted
        if (defaultSave == null)
        {
            tContainer.Clear();
            saveIndex = 0;
            LastIndex = 0;
            if (savesNameList.Count != 0)
            {
                //Reset To First Element in List
                defaultSave = AssetDatabase.LoadAssetAtPath<WorkshopElementsScriptableObj>(AssetDatabase.GUIDToAssetPath(saveList[saveIndex]));
                tContainer = defaultSave.stContainer;
                inactiveElements = defaultSave.sInactiveElements;
            }
            else { CreateSaveIfNot(); } //BugFix: Buttons Disappear when All Saves Deleted
        }
        if(saveIndex != LastIndex)
        {
            defaultSave.stContainer = tContainer;
            defaultSave.sInactiveElements = inactiveElements;
            EditorUtility.SetDirty(defaultSave);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            defaultSave = AssetDatabase.LoadAssetAtPath<WorkshopElementsScriptableObj>(AssetDatabase.GUIDToAssetPath(saveList[saveIndex]));
            tContainer = defaultSave.stContainer;
            inactiveElements = defaultSave.sInactiveElements;
            LastIndex = saveIndex;
        }
        scrollPosition = GUI.BeginScrollView(new Rect(0, 20, Screen.width, Screen.height - 60), scrollPosition, new Rect(0, 0, Screen.width, tContainer.Count * 25 + 30), false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
        for (int i = 0; i < tContainer.Count; i++)
        {
            GUI.SetNextControlName("b" + i.ToString());
            addElement(tContainer[i], i);
        }
        GUI.EndScrollView(true);

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
            scrollPosition = GUI.BeginScrollView(new Rect(0, 20, Screen.width, Screen.height - 60), scrollPosition, new Rect(0, 0, Screen.width, tContainer.Count * 25 + 30), false, false, GUIStyle.none, GUI.skin.verticalScrollbar);  //Bug Fix: Scroll doesn't include the TextField
            GUI.SetNextControlName("t" + (tempInt).ToString());
            tContainer[tempInt] = GUI.TextField(new Rect(10, 25 * tempInt + 30, Screen.width - 50, 20), tContainer[tempInt], 40);
            GUI.EndScrollView(true);
            GUI.FocusControl("t" + (tempInt).ToString());
        }

        if (isAdding && Event.current.keyCode == KeyCode.Return && !(Event.current.type == EventType.KeyUp))
        {
            if (GameObject.Find(tContainer[tempInt]) == null && !inactiveElements.ContainsKey(tContainer[tempInt])) { tContainer.RemoveAt(tempInt);} //BugFix: Inactive buttons Removed after Enter Hit
            else
            {
                GUI.SetNextControlName("b" + (tempInt).ToString());
                addElement(tContainer[tempInt], tempInt);
                isAdding = false;
                Repaint();
            }
        }
        
        if (GUI.Button(new Rect(10, Screen.height - 42, 85, 20), "By TensaDev", EditorStyles.whiteBoldLabel)) { Help.BrowseURL("https://github.com/redamazoz");}
        
    }
    void addElement(string elementName, int i) {

        if (GameObject.Find(elementName))
        {

            if (GameObject.Find(elementName).activeSelf)
            {
                if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), elementName, onButtonGUIStyle)) { Selection.activeGameObject = GameObject.Find(elementName); }
                if (GUI.Button(new Rect(Screen.width - 35, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/Workshop/Editor/x.png",typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i); }
            }
        }
        else if(inactiveElements.ContainsKey(elementName))
        {
            //BugFix: Buttons Disappear when Inactive Element in List Deleted in Hierarchy
            if (inactiveElements[elementName] == null)
            {
                if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), "", offButtonGUIStyle)) { Selection.activeGameObject = null; }
                if (GUI.Button(new Rect(Screen.width - 35, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/Workshop/Editor/x.png", typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i); }
            }
            else if (!((inactiveElements[elementName]).activeSelf))
            {
                if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), elementName, offButtonGUIStyle)) { Selection.activeGameObject = inactiveElements[elementName];}
                if (GUI.Button(new Rect(Screen.width - 35, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/Workshop/Editor/x.png", typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i);}
            }
        }
        //BugFix: No Delete Button if GameObjs isn't in LoadedScenes or is Deleted from Hierarchy
        else if (elementName != "")
        {
            if (GUI.Button(new Rect(10, 25 * i + 30, Screen.width - 35, 20), "", onButtonGUIStyle)) { Selection.activeGameObject = null; }
            if (GUI.Button(new Rect(Screen.width - 35, 25 * i + 30, 20, 20), (Texture)AssetDatabase.LoadAssetAtPath("Assets/Workshop/Editor/x.png", typeof(Texture)), EditorStyles.miniButton)) { tContainer.RemoveAt(i); }
        }

    }

    private void dragg()
    {
        EventType eventType = Event.current.type;
        if (eventType == EventType.DragUpdated ||
            eventType == EventType.DragPerform)
        {
            // Show a copy icon on the drag
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (eventType == EventType.DragPerform && (DragAndDrop.objectReferences[0]).name != "")
            {
                GUI.SetNextControlName("t" + (tempInt).ToString());
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    tContainer.Add((DragAndDrop.objectReferences[i]).name);
                }

                DragAndDrop.AcceptDrag();
                //Making Stack of Elements Draggable at Once
                for(int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    inactiveElements[DragAndDrop.objectReferences[i].name] = (GameObject)(DragAndDrop.objectReferences[i]);
                    if (inactiveElements[DragAndDrop.objectReferences[i].name].activeSelf) { inactiveElements.Remove(DragAndDrop.objectReferences[i].name);}
                }
            }
            Event.current.Use();
        }
    }

    //BugFix: Buttons Disappear when All Saves Deleted
    private void CreateSaveIfNot()
    {
        defaultSave = CreateInstance<WorkshopElementsScriptableObj>();
        AssetDatabase.CreateAsset(defaultSave, "Assets/Workshop/Editor/Saves/defaultSave.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void GetSceneInactiveGameObjs(Dictionary<string, GameObject> sceneInactiveGO)
    {
        List<GameObject> inactiveGO = new List<GameObject>();

        //for Every Scene in Hierarchy
        for(int i =0; i < SceneManager.sceneCount; i++)
        {
            SceneManager.GetSceneAt(i).GetRootGameObjects(inactiveGO);

            foreach (GameObject go in inactiveGO)
            {
                foreach (Transform child in go.GetComponentsInChildren<Transform>(true)) //Include Inactive Childs
                {
                    if (!(child.gameObject.activeSelf)) { sceneInactiveGO[child.gameObject.name] = child.gameObject; }
                }
            }
        }
        
    }

}
