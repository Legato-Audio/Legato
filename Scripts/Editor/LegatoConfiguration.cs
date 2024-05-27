using extOSC;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Legato
{
    public class LegatoConfiguration : EditorWindow
    {
        const int INSTRUMENTS = 0, TEMPOS = 1;
        const string motifMatrixPath = "Assets/Legato/.motifMatrix.txt";
        string[] folders = { "instruments", "tempos", "fragments" };
        const string tempFolder = "legatoMetadata";

        GameObject go;
        OSCTransmitter transmitter;

        int numInstruments, numMotifs, numTempos;
        string[] instrumentsID, motifsID;
        int[] tempos;
        bool[,] instrumentMatrix, temposMatrix;
        bool saved = true;

        Vector2 scrollWindow, scrollMatrixV, scrollMatrixH;
        bool showInstruments = false, showMotifs = false, showTempos = false, showAdvanced = false;
        int selectedTab = 0;

        bool rendering = false;
        int renderProgress, totalRenders;
        string progressText = "", dots;

        int auxInt;
        string auxString;
        bool auxBool;

        [MenuItem("Window/Legato")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(LegatoConfiguration));
        }

        private void Awake()
        {
            go = new GameObject();
            go.name = "Transmitter";
            go.hideFlags = HideFlags.HideInHierarchy;

            transmitter = go.AddComponent<OSCTransmitter>();
            transmitter.RemotePort = 8000;
            transmitter.RemoteHost = "127.0.0.1";
            transmitter.Connect();

            ReadData();
        }

        private void OnDestroy()
        {
            if (!saved && EditorUtility.DisplayDialog("The project has been changed", "Would you like to save the changes?", "Save", "No"))
                WriteData();
            DestroyImmediate(go);
        }

        private void Update()
        {
            if (rendering)
            {
                switch(((int)EditorApplication.timeSinceStartup) % 4)
                {
                    case 0:
                        dots = "";
                        break;
                    case 1:
                        dots = ".";
                        break;
                    case 2:
                        dots = "..";
                        break;
                    case 3:
                        dots = "...";
                        break;
                }

                DirectoryInfo d = new DirectoryInfo(Application.dataPath + "/Resources/Legato/RenderedSamples");
                renderProgress = d.GetFiles("*.wav").Length;

                if (renderProgress > 0)
                    progressText = "Rendering... " + renderProgress + "/" + totalRenders;
                else
                    progressText = dots + "Queuing renders" + dots;

                if (renderProgress == totalRenders)
                {
                    rendering = false;
                    CreateFragments();
                }
            }
        }

        void OnGUI()
        {
            scrollWindow = EditorGUILayout.BeginScrollView(scrollWindow);

            GUI.enabled = !rendering;

            #region GUIStyles
            GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            
            GUIStyle semiTransparentLabelStyle = new GUIStyle(EditorStyles.label);
            semiTransparentLabelStyle.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
            
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Normal;
            #endregion

            #region INSTRUMENTS SECTION
            EditorGUILayout.LabelField("Instruments", titleStyle);

            // Define number of instruments
            EditorGUILayout.BeginHorizontal();
            auxInt = EditorGUILayout.DelayedIntField("Number of intstruments", numInstruments, GUILayout.ExpandWidth(false));
            if (auxInt != numInstruments)
            {
                numInstruments = auxInt;
                if (numInstruments < 1) numInstruments = 1;
                else if (numInstruments > 99) numInstruments = 99;

                // Update ID array
                string[] iID = new string[numInstruments];
                for (int i = 0; i < numInstruments; i++)
                {
                    if (i < instrumentsID.Length) iID[i] = instrumentsID[i];
                    else iID[i] = "instrument" + (i + 1);
                }
                instrumentsID = iID;

                UpdateInstrumentsMatrix();
                saved = false;
            }
            EditorGUILayout.LabelField("  [1, " + (100 - numMotifs) + "]", semiTransparentLabelStyle);
            EditorGUILayout.EndHorizontal();

            // Foldout with instruments IDs
            showInstruments = EditorGUILayout.Foldout(showInstruments, "Instruments ID", foldoutStyle);
            if (showInstruments)
            {
                for (int i = 0; i < numInstruments; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Instrument " + (i + 1), GUILayout.Width(100), GUILayout.ExpandWidth(false));
                    auxString = EditorGUILayout.DelayedTextField(instrumentsID[i]);
                    EditorGUILayout.EndHorizontal();

                    if (auxString != instrumentsID[i])
                    {
                        instrumentsID[i] = auxString;
                        saved = false;
                    }
                }
            }
            GUILayout.Space(10);
            #endregion

            #region MOTIFS SECTION
            EditorGUILayout.LabelField("Motifs", titleStyle);

            // numInstruments + numMotfis must be less than 100
            if (numMotifs > 100 - numInstruments)
            {
                numMotifs = 100 - numInstruments;

                // Update ID array
                string[] mID = new string[numMotifs];
                for (int m = 0; m < numMotifs; m++) mID[m] = motifsID[m];
                motifsID = mID;

                UpdateInstrumentsMatrix();
                UpdateTemposMatrix(-1);
                saved = false;
            }

            // Define number of motifs
            EditorGUILayout.BeginHorizontal();
            auxInt = EditorGUILayout.DelayedIntField("Number of motifs", numMotifs, GUILayout.ExpandWidth(false));
            if (auxInt != numMotifs)
            {
                numMotifs = auxInt;
                if (numMotifs < 1) numMotifs = 1;
                else if (numMotifs > 100 - numInstruments) numMotifs = 100 - numInstruments;

                // Update array
                string[] mID = new string[numMotifs];
                for (int m = 0; m < numMotifs; m++)
                {
                    if (m < motifsID.Length) mID[m] = motifsID[m];
                    else mID[m] = "motif" + (m + 1);
                }
                motifsID = mID;

                UpdateInstrumentsMatrix();
                UpdateTemposMatrix(-1);
                saved = false;
            }
            EditorGUILayout.LabelField("  [1, " + (100 - numInstruments) + "]", semiTransparentLabelStyle);
            EditorGUILayout.EndHorizontal();

            // Foldout with motifs IDs
            showMotifs = EditorGUILayout.Foldout(showMotifs, "Motifs ID", foldoutStyle);
            if (showMotifs)
            {
                for (int m = 0; m < numMotifs; m++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Motif " + (m + 1), GUILayout.Width(100), GUILayout.ExpandWidth(false));
                    auxString = EditorGUILayout.DelayedTextField(motifsID[m]);
                    EditorGUILayout.EndHorizontal();

                    if (auxString != motifsID[m])
                    {
                        motifsID[m] = auxString;
                        saved = false;
                    }
                }
            }
            GUILayout.Space(10);
            #endregion

            #region TEMPOS SECTION
            EditorGUILayout.LabelField("Tempos", titleStyle);

            showTempos = EditorGUILayout.Foldout(showTempos, "Number of tempos: " + numTempos, foldoutStyle);
            if (showTempos)
            {
                if (numTempos == 1) EditorGUILayout.HelpBox("The number of tempos can't be less than 1", MessageType.Info);

                for (int t = 0; t < numTempos; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(tempos[t] + "", GUILayout.Width(100));
                    GUI.enabled = numTempos > 1;
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        numTempos--;
                        int[] aux = new int[numTempos];
                        for (int a = 0; a < numTempos; a++)
                        {
                            if (a < t) aux[a] = tempos[a];
                            else aux[a] = tempos[a + 1];
                        }
                        tempos = aux;

                        UpdateTemposMatrix(t);

                        saved = false;
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                auxInt = EditorGUILayout.DelayedIntField(0, GUILayout.Width(100));
                if (auxInt > 0 && auxInt < 961)
                {
                    numTempos++;
                    int[] aux = new int[numTempos];
                    int a = 0;
                    while (a < tempos.Length && tempos[a] < auxInt)
                    {
                        aux[a] = tempos[a];
                        a++;
                    }
                    aux[a] = auxInt;
                    int t = a; // Save position of new tempo
                    a++;
                    while (a < numTempos)
                    {
                        aux[a] = tempos[a - 1];
                        a++;
                    }
                    tempos = aux;

                    UpdateTemposMatrix(t);

                    saved = false;
                }
                EditorGUILayout.LabelField("  [1, 960]", semiTransparentLabelStyle);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            #endregion

            // MATRIX SECTION
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Instruments", "Tempos" });
            RenderMatrix(selectedTab);

            #region MATRIX BUTTONS
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(100)); // To align buttons with matrix
            if (GUILayout.Button("Disable All", GUILayout.ExpandWidth(false)))
            {
                SetMatrix(selectedTab, false);
                saved = false;
            }
            if (GUILayout.Button("Enable All", GUILayout.ExpandWidth(false)))
            {
                SetMatrix(selectedTab, true);
                saved = false;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            #endregion

            #region REGENERATE ASSETS
            // Foldout with Regenerate Assets button
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", foldoutStyle);
            if (showAdvanced)
            {
                GUIContent regenerateButton = new GUIContent("Regenerate assets", "  ");
                if (GUILayout.Button(regenerateButton, GUILayout.ExpandWidth(false)))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Fragments");
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Instruments");
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Tempos");
                    CreateFragments();
                }
            }
            #endregion

            #region SAVE & RENDER BUTTONS
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                WriteData();
                saved = true;
            }
            if (GUILayout.Button("Render", GUILayout.ExpandWidth(false)))
            {
                WriteData();
                saved = true;

                totalRenders = CountRenders();
                if (totalRenders > 0)
                {
                    rendering = true;
                    renderProgress = 0;

                    DeleteOldRenders();
                    transmitter.Send(new OSCMessage("/action/_LegatoRender"));
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/RenderedSamples");
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Fragments");
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Instruments");
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Legato/Tempos");
                }
                else Debug.LogWarning("No clips to render. The instrument or tempo matrices may be empty or there may be no valid combinations.");
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.EndScrollView();

            GUI.enabled = true;

            if (rendering)
            {
                Rect progressBarPos = EditorGUILayout.GetControlRect(GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
                EditorGUI.ProgressBar(progressBarPos, renderProgress * 1.0f / totalRenders, progressText);
            }
        }

        private int CountRenders()
        {
            int numRenders = 0;
            for(int m = 0; m < numMotifs; m++)
            {
                int instruments = 0, tempos = 0;
                for(int i = 0; i < numInstruments; i++) if (instrumentMatrix[m, i]) instruments++;
                for(int t = 0; t < numTempos; t++) if (temposMatrix[m, t]) tempos++;
                numRenders += instruments * tempos;
            }
            return numRenders;
        }

        private void DefaultValues()
        {
            numInstruments = 1;
            instrumentsID = new string[numInstruments];
            for (int i = 0; i < numInstruments; i++) instrumentsID[i] = "instrument" + (i + 1);

            numMotifs = 1;
            motifsID = new string[numMotifs];
            for (int m = 0; m < numMotifs; m++) motifsID[m] = "motif" + (m + 1);

            numTempos = 1;
            tempos = new int[numTempos];
            tempos[0] = 120;

            instrumentMatrix = new bool[numMotifs, numInstruments];
            SetMatrix(INSTRUMENTS, true);

            temposMatrix = new bool[numMotifs, numTempos];
            SetMatrix(TEMPOS, true);
        }

        private void WriteData()
        {
            StreamWriter writer = new StreamWriter(motifMatrixPath, false);

            writer.WriteLine(numInstruments);
            for (int i = 0; i < numInstruments; i++) writer.WriteLine(instrumentsID[i]);
            writer.WriteLine();

            writer.WriteLine(numMotifs);
            for (int m = 0; m < numMotifs; m++) writer.WriteLine(motifsID[m]);
            writer.WriteLine();

            writer.WriteLine(numTempos);
            for (int t = 0; t < numTempos; t++) writer.WriteLine(tempos[t]);
            writer.WriteLine();

            for (int m = 0; m < numMotifs; m++)
            {
                for (int i = 0; i < numInstruments; i++) writer.Write(instrumentMatrix[m, i] ? 1 : 0);
                writer.WriteLine();
                for (int t = 0; t < numTempos; t++) writer.Write(temposMatrix[m, t] ? 1 : 0);
                writer.WriteLine();
                writer.WriteLine();
            }

            writer.Close();
        }

        private void CreateFragments()
        {
            //SaveMetadata();
            AssetDatabase.Refresh();

            foreach(string assetGUID in AssetDatabase.FindAssets("t:Instrument", new string[] { "Assets/Resources/Legato/Instruments" }))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetGUID));
            }
            for (int i = 0; i < numInstruments; ++i)
            {
                Instrument asset = ScriptableObject.CreateInstance<Instrument>();

                AssetDatabase.CreateAsset(asset, "Assets/Resources/Legato/Instruments/" + instrumentsID[i] + ".asset");
            }

            foreach (string assetGUID in AssetDatabase.FindAssets("t:Tempo", new string[] { "Assets/Resources/Legato/Tempos" }))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetGUID));
            }
            for (int t = 0; t < numTempos; ++t)
            {
                Tempo asset = ScriptableObject.CreateInstance<Tempo>();

                AssetDatabase.CreateAsset(asset, "Assets/Resources/Legato/Tempos/" + tempos[t] + ".asset");
            }

            foreach (string assetGUID in AssetDatabase.FindAssets("t:Fragment", new string[] { "Assets/Resources/Legato/Fragments" }))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assetGUID));
            }
            for (int m = 0; m < numMotifs; ++m)
            {
                FragmentBuilder.CreateFragment(motifsID[m]);
            }

            //RestoreMetadata();
            AssetDatabase.SaveAssets();

            if (EditorSceneManager.GetActiveScene().isDirty && EditorUtility.DisplayDialog("Save scene?", "Unsaved changes in scene will be lost", "Save", "Don't save"))
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
        }

        private void ReadData()
        {
            try
            {
                StreamReader reader = new StreamReader(motifMatrixPath);

                numInstruments = int.Parse(reader.ReadLine());
                instrumentsID = new string[numInstruments];
                for (int i = 0; i < numInstruments; i++)
                {
                    instrumentsID[i] = reader.ReadLine();
                }
                reader.ReadLine();

                numMotifs = int.Parse(reader.ReadLine());
                motifsID = new string[numMotifs];
                for (int m = 0; m < numMotifs; m++)
                {
                    motifsID[m] = reader.ReadLine();
                }
                reader.ReadLine();

                numTempos = int.Parse(reader.ReadLine()); // Convertir en variable de la clase
                tempos = new int[numTempos];
                for (int t = 0; t < numTempos; t++) tempos[t] = int.Parse(reader.ReadLine()); // Guardar en variables de tempos
                reader.ReadLine();

                instrumentMatrix = new bool[numMotifs, numInstruments];
                temposMatrix = new bool[numMotifs, numTempos];
                for (int m = 0; m < numMotifs; m++)
                {
                    string line = reader.ReadLine();
                    for (int i = 0; i < numInstruments; i++) instrumentMatrix[m, i] = line[i] == '1';
                    line = reader.ReadLine();
                    for (int t = 0; t < numTempos; t++) temposMatrix[m, t] = line[t] == '1';
                    reader.ReadLine();
                }

                reader.Close();
            }
            catch
            {
                Debug.LogWarning("Can't read from file. File doesn't exist or has been corrupted.");
                DefaultValues();
            }
        }

        private void SetMatrix(int matrix, bool value)
        {
            if (matrix == INSTRUMENTS)
            {
                for (int m = 0; m < numMotifs; m++)
                {
                    for (int i = 0; i < numInstruments; i++)
                    {
                        instrumentMatrix[m, i] = value;
                    }
                }
            }
            else // matrix == TEMPOS
            {
                for (int m = 0; m < numMotifs; m++)
                {
                    for (int t = 0; t < numTempos; t++)
                    {
                        temposMatrix[m, t] = value;
                    }
                }
            }
        }

        private void UpdateInstrumentsMatrix()
        {
            bool[,] aux = new bool[numMotifs, numInstruments];
            for (int m = 0; m < numMotifs; m++)
            {
                for (int i = 0; i < numInstruments; i++)
                {
                    if (m < instrumentMatrix.GetLength(0) && i < instrumentMatrix.GetLength(1)) aux[m, i] = instrumentMatrix[m, i];
                    else aux[m, i] = true;
                }
            }
            instrumentMatrix = aux;
        }

        private void UpdateTemposMatrix(int newTempo)
        {
            bool[,] aux = new bool[numMotifs, numTempos];

            if (newTempo < 0) // numMotifs changed
            {
                for (int m = 0; m < numMotifs; m++)
                {
                    for (int t = 0; t < numTempos; t++)
                    {
                        if (m < temposMatrix.GetLength(0)) aux[m, t] = temposMatrix[m, t];
                        else aux[m, t] = true;
                    }
                }
            }
            else // numTempos changed
            {
                if (numTempos > temposMatrix.GetLength(1)) // tempo added
                {
                    for (int m = 0; m < numMotifs; m++)
                    {
                        for (int t = 0; t < numTempos; t++)
                        {
                            if (t < newTempo) aux[m, t] = temposMatrix[m, t];
                            else if (t == newTempo) aux[m, t] = true;
                            else aux[m, t] = temposMatrix[m, t - 1];
                        }
                    }
                }
                else // tempo removed
                {
                    for (int m = 0; m < numMotifs; m++)
                    {
                        for (int t = 0; t < numTempos; t++)
                        {
                            if (t < newTempo) aux[m, t] = temposMatrix[m, t];
                            else aux[m, t] = temposMatrix[m, t + 1];
                        }
                    }
                }
            }

            temposMatrix = aux;
        }

        private void RenderMatrix(int matrix)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            scrollMatrixH = EditorGUILayout.BeginScrollView(scrollMatrixH, GUIStyle.none, GUIStyle.none);
            EditorGUILayout.BeginHorizontal();
            if (matrix == 0)
            {
                for (int i = 0; i < numInstruments; i++)
                {
                    Rect position = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUI.LabelField(position, new GUIContent(instrumentsID[i], " " + instrumentsID[i] + " "));
                }
            }
            else
            {
                for (int t = 0; t < numTempos; t++)
                {
                    Rect position = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUI.LabelField(position, new GUIContent(tempos[t] + "", " " + tempos[t] + " "));
                }
            }
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();


            GUIStyle rightAlignment = new GUIStyle(GUI.skin.label);
            rightAlignment.alignment = TextAnchor.MiddleRight;

            scrollMatrixV = EditorGUILayout.BeginScrollView(scrollMatrixV); // General scroll view containing the matrix
            EditorGUILayout.BeginHorizontal(); // Horizontal consisting of: vertical with motifs, vertical with matrix

            // Row labels
            EditorGUILayout.BeginVertical(); // Vertical with motifs
            for (int m = 0; m < numMotifs; m++)
            {
                EditorGUILayout.LabelField(motifsID[m], rightAlignment, GUILayout.MaxWidth(100));
            }
            EditorGUILayout.LabelField("", GUILayout.MaxWidth(100)); // Add an empty row to pair with the horizontal scrollbar
            EditorGUILayout.EndVertical();

            // Toggles
            EditorGUILayout.BeginVertical();// Vertical with matrix
            scrollMatrixH = EditorGUILayout.BeginScrollView(scrollMatrixH); // Scroll view horizontal for the matrix
            for (int m = 0; m < numMotifs; m++)
            {
                EditorGUILayout.BeginHorizontal(); // Each row
                if (matrix == INSTRUMENTS)
                {
                    for (int i = 0; i < numInstruments; i++)
                    {
                        Rect position = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight));
                        auxBool = EditorGUI.Toggle(position, instrumentMatrix[m, i]);
                        if (auxBool != instrumentMatrix[m, i])
                        {
                            instrumentMatrix[m, i] = auxBool;
                            saved = false;
                        }
                    }
                }
                else // matrix == TEMPOS 
                {
                    for (int t = 0; t < numTempos; t++)
                    {
                        Rect position = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight));
                        auxBool = EditorGUI.Toggle(position, temposMatrix[m, t]);
                        if (auxBool != temposMatrix[m, t])
                        {
                            temposMatrix[m, t] = auxBool;
                            saved = false;
                        }
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        // Delete all existing sound files (but not the associated .meta files)
        private void DeleteOldRenders()
        {
            string filepath = Application.dataPath + "/Resources/Legato/RenderedSamples";
            
            if (!Directory.Exists(filepath)) return;

            DirectoryInfo d = new DirectoryInfo(filepath);
            foreach (var file in d.GetFiles())
            {
                AssetDatabase.DeleteAsset("Assets/Resources/Legato/RenderedSamples/" + file.Name);
            }
        }

        private void SaveMetadata()
        {
            DirectoryInfo d;
            if (Directory.Exists(Path.GetTempPath() + tempFolder)) // If metadata is already saved, overwrite it
            {
                d = new DirectoryInfo(Path.GetTempPath() + tempFolder);

                d.Delete(true);
            }


            // Copy each folder of metadata to temp folder
            foreach (var folder in folders)
            {
                string projectPath = Application.dataPath + "/Resources/Legato/" + folder;
                string tempPath = Path.GetTempPath() + tempFolder + "\\" + folder;
                d = new DirectoryInfo(projectPath);

                if (!Directory.Exists(projectPath)) continue;

                Directory.CreateDirectory(tempPath);

                foreach (var file in d.GetFiles("*.meta"))
                {
                    File.Copy(file.FullName, tempPath + "\\" + file.Name);
                }
                Debug.Log("Saved " + folder + " metadata in " + tempPath);
            }
        }

        // Restore previous .meta files to new audio files with old names, to preserve references within project
        private void RestoreMetadata()
        {
            if (!Directory.Exists(Path.GetTempPath() + tempFolder)) return;

            
            // Restore each class's of metadata from temp folder
            foreach (var folder in folders)
            {
                // Delete meta files created by Unity since last render
                string projectPath = Application.dataPath + "/Resources/Legato/" + folder;
                DirectoryInfo d = new DirectoryInfo(projectPath), tempD = new DirectoryInfo(Path.GetTempPath() + tempFolder + "\\" + folder);
                foreach (var file in d.GetFiles("*.meta"))
                {
                    File.Delete(file.FullName);
                }

                // Bring back previously saved metadata
                foreach (var file in tempD.GetFiles("*.meta"))
                {
                    Debug.Log(file.Name);
                    File.Copy(file.FullName, projectPath + "/" + file.Name);
                }
            }

            // Delete meta files from temp folder
            DirectoryInfo tempDir = new DirectoryInfo(Path.GetTempPath() + tempFolder);
            tempDir.Delete(true);
        }
    }
}