using UnityEngine;

public class GridMarker : MonoBehaviour
{
    public int rows = 6;
    public int cols = 6;
    public Color gridColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        
        Vector3 origin = transform.position;
        float cellSize = 1f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 center = origin + new Vector3(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2, 0);
                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0));
            }
        }
    }
}