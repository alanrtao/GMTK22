using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int WIDTH, HEIGHT;
    public int Count => WIDTH * HEIGHT;

    private int[] m_Grid;
    private BaseRollable[] m_Grid_Obstacles;

    private void Start()
    {
        m_Grid = new int[Count];
        m_Grid_Obstacles = new BaseRollable[Count];
    }

    public int Fold(int z, int x)
    {
        return z * WIDTH + x;
    }

    public int Weight(int c1, int c2) => Mathf.Abs(Grid(c1) - Grid(c2));

    public int Fold((float, float) p) => Fold(Mathf.FloorToInt(p.Item1), Mathf.FloorToInt(p.Item2));
    public int Fold((int, int) p) => Fold(p.Item1, p.Item2);

    /// <returns>z, x</returns>
    public (int, int) Expand(int f) => (f / WIDTH, f % WIDTH);

    public int Grid(float z, float x) => Grid(
        Mathf.FloorToInt(z), Mathf.FloorToInt(x)
        );
    public int Grid(int z, int x) => m_Grid[Fold(z, x)];
    public int Grid(int c) => m_Grid[c];

    public void SetObstacle(int z, int x, BaseRollable o) => m_Grid_Obstacles[Fold(z, x)] = o;
    public BaseRollable Obstacle(int z, int x) => m_Grid_Obstacles[Fold(z, x)];

    public void Flush()
    {
        for (int i = 0; i < Count; i++)
        {
            if (m_Grid[i] != int.MaxValue)
                m_Grid[i] = 0;
        }
    }

    public bool Legal((float, float) p) => Legal(p.Item1, p.Item2);
    public bool Legal(float z, float x) => Legal(Mathf.FloorToInt(z), Mathf.FloorToInt(x));
    public bool Legal((int, int) p) => Legal(p.Item1, p.Item2);
    public bool Legal(int z, int x) => (z >= 0 && z < WIDTH) && (x >= 0 && x < HEIGHT) &&
        (Obstacle(z, x) == null || Obstacle(z, x) == Player.Instance);

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        for(int i = 0; i < WIDTH; i++) {
            for(int j = 0; j < HEIGHT; j++) {
                Gizmos.color = Obstacle(i, j) ? Color.red : Color.green;
                Gizmos.DrawCube(new Vector3(j, 0.5f, i), Vector3.one * 0.5f);
            }
        }
    }
}
