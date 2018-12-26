using UnityEngine;

public class Frame : MonoBehaviour
{
    // Draws a line from "startVertex" var to the curent mouse position.
    public Material mat;
    Vector3 begVex;
    Vector3 endVex;

    void Start()
    {
        begVex = Vector3.zero;
        endVex = new Vector3(10f,10f,10f);
    }

    void Update()
    {
        //endVex = Input.mousePosition;
        // Press space to update startVertex
        if (Input.GetKeyDown(KeyCode.Space))
        {
           // begVex = new Vector3(endVex.x / Screen.width, endVex.y / Screen.height, 0);
        }
    }

    void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex(begVex);
       // GL.Vertex(new Vector3(endVex.x / Screen.width, endVex.y / Screen.height, 0));
        GL.Vertex(endVex);
        GL.End();

        GL.PopMatrix();
    }
}
