using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class PolygonColliderVisualizer : MonoBehaviour
{



    private PolygonCollider2D polygonCollider;
    private LineRenderer lineRenderer;


    public Color lineColor = Color.red;

    public float lineWidth = 0.05f;

    void Awake()
    {

        polygonCollider = GetComponent<PolygonCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();


        ConfigureLineRenderer();

        DrawPolygon();
    }


    private void ConfigureLineRenderer()
    {

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true; 
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); 
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }


    private void DrawPolygon()
    {

        Vector2[] points2D = polygonCollider.points;


        Vector3[] points3D = points2D.Select(p => (Vector3)p).ToArray();


        lineRenderer.positionCount = points3D.Length;

        lineRenderer.SetPositions(points3D);

    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        if (lineRenderer != null)
        {
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            if (polygonCollider != null)
            {
                DrawPolygon();
            }
        }
    }
#endif
}
