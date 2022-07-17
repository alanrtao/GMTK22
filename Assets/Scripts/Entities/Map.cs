using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int WIDTH, HEIGHT;
    public int Count => WIDTH * HEIGHT;

    private float[] m_Grid;
    private BaseRollable[] m_Grid_Obstacles;

    private void Start()
    {
        m_Grid = new float[Count];
        m_Grid_Obstacles = new BaseRollable[Count];

        Height();

        Render();
    }

    public int Fold(int z, int x)
    {
        return z * WIDTH + x;
    }

    public int Weight(int c1, int c2) => Mathf.FloorToInt(Mathf.Abs(Grid(c1) - Grid(c2)));

    public int Fold((float, float) p) => Fold(Mathf.FloorToInt(p.Item1), Mathf.FloorToInt(p.Item2));
    public int Fold((int, int) p) => Fold(p.Item1, p.Item2);

    /// <returns>z, x</returns>
    public (int, int) Expand(int f) => (f / WIDTH, f % WIDTH);

    public float Grid(float z, float x) => Grid(
        Mathf.FloorToInt(z), Mathf.FloorToInt(x)
        );
    public float Grid(int z, int x) => m_Grid[Fold(z, x)];
    public float Grid(int c) => m_Grid[c];

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

    public bool Legal((float, float) p, (float, float) q) => Legal(p.Item1, p.Item2, q.Item1, q.Item2);
    public bool Legal(float sz, float sx, float z, float x) => Legal(Mathf.FloorToInt(sz), Mathf.FloorToInt(sx), Mathf.FloorToInt(z), Mathf.FloorToInt(x));
    public bool Legal((int, int) p, (int, int) q) => Legal(p.Item1, p.Item2, q.Item1, q.Item2);
    public bool Legal(int sz, int sx, int z, int x)
        => z >= 0 && z < WIDTH && x >= 0 && x < HEIGHT &&
        (Obstacle(z, x) == null || Obstacle(z, x) == Player.Instance) &&
        (Grid(z, x) - Grid(sz, sx)) < 1f;

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

    [SerializeField] int kHeightLoop;
    private void Height()
    {
        for(int l = 0; l < kHeightLoop; l++)
        {

            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    float h = Mathf.PerlinNoise(i + l * WIDTH, j + l * HEIGHT);

                }
            }
        }
    }

    [SerializeField] GameObject floorTilePrototype;
    [SerializeField] List<Sprite> floorTiles;
    private void Render()
    {
        for (int i = 0; i < WIDTH; i++) {
            for(int j  =0; j < HEIGHT; j++) {
                var ftp = Instantiate(floorTilePrototype, transform);
                ftp.transform.position = new Vector3(j, 0, i);
                // ftp.GetComponent<SpriteRenderer>().sprite = floorTiles.Random();
                ftp.transform.rotation = Quaternion.AngleAxis(
                    Mathf.FloorToInt(Random.value * 4) * 90, Vector3.up
                    ) * ftp.transform.rotation;
            }
        }
    }
}
