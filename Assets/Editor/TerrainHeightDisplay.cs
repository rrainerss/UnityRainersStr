using UnityEngine;
using UnityEditor;

//This script was needed to create terrains with SetHeight brush since i cant pick the height manually, have to type it in

[InitializeOnLoad]
public class TerrainHeightDisplay
{
    static TerrainHeightDisplay()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        //Get current mouse position in Scene View
        Event e = Event.current;
        if (e == null) return;

        //Create ray from mouse position
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;

        //Find the terrain under the mouse ray
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider == null) return;

        if (terrainCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            float height = terrain.SampleHeight(hit.point);

            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 200, 40));
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 14;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(0, 0, 200, 30), $"Terrain Height: {height:F2}", style);
            GUILayout.EndArea();

            Handles.EndGUI();

            //Repaint to update every mouse move
            sceneView.Repaint();
        }
    }
}
