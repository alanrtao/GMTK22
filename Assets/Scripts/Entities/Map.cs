using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map : MonoBehaviour
{
    public int WIDTH, HEIGHT;
    public int Count => WIDTH * HEIGHT;

    private float[,] m_Grid;
    private BaseRollable[,] m_Grid_Obstacles;


    public bool done = false;
    public float seedX, seedY;

    private void Awake()
    {
        done = false;
        seedX = Random.value * 10f;
        seedY = Random.value * 10f;
    }


    private void Start()
    {
        done = false;
        m_Grid = new float[WIDTH, HEIGHT];
        m_Grid_Obstacles = new BaseRollable[WIDTH, HEIGHT];

        Height();

        Render();
        
        done = true;
    }

    public List<(int, int)> GetEmptyAtDistance(int sz, int sx, int d)
    {
        var temp = new List<(int, int)>();
        for(int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                if (Mathf.Abs(sz - i) + Mathf.Abs(sx - j) == d && Obstacle(i, j) == null)
                {
                    //Debug.Log((i, j));
                    temp.Add((i, j));
                }
            }
        }
        // Debug.Log(string.Join(", ", temp));
        return temp;
    }

    public int Weight(int sz, int sx, int ez, int ex) => Mathf.RoundToInt(Mathf.Abs(Grid(sz, sx) - Grid(ez, ex)));

    public (int, int) Fold(float z, float x) => (Mathf.RoundToInt(z), Mathf.RoundToInt(x));
    public (int, int) Fold((float, float) p) => (Mathf.RoundToInt(p.Item1), Mathf.RoundToInt(p.Item2));

    /// <returns>z, x</returns>
    public (int, int) Expand(int f) => (f / WIDTH, f % WIDTH);

    public float Grid((float, float) p) => Grid(p.Item1, p.Item2);
    public float Grid(float z, float x) => Grid(
        Mathf.RoundToInt(z), Mathf.RoundToInt(x)
        );

    public float Grid(int z, int x) => m_Grid[z, x];

    public void SetObstacle(BaseRollable o)
    {
        var p = ((Orientation)o).position_GRD;
        SetObstacle(
            Mathf.RoundToInt(p.Item1),
            Mathf.RoundToInt(p.Item2),
            o
            );
    }
    public void SetObstacle(int z, int x, BaseRollable o) {
        if (o != null && Obstacle(z, x) != null)
        {
            throw new UnityException($"{o.gameObject.name} clashing with {Obstacle(z, x).gameObject.name}");
        }
        m_Grid_Obstacles[z, x] = o;
    }
    public BaseRollable Obstacle(int z, int x) => m_Grid_Obstacles[z, x];

    //public bool Legal((float, float) p, (float, float) q) => Legal(p.Item1, p.Item2, q.Item1, q.Item2);
    //public bool Legal(float sz, float sx, float z, float x) => Legal(Mathf.RoundToInt(sz), Mathf.RoundToInt(sx), Mathf.RoundToInt(z), Mathf.RoundToInt(x));
    //public bool Legal((int, int) p, (int, int) q) => Legal(p.Item1, p.Item2, q.Item1, q.Item2);
    //public bool Legal(int sz, int sx, int z, int x)
    //    => z >= 0 && z < WIDTH && x >= 0 && x < HEIGHT &&
    //    Obstacle(z, x) == null; // || Obstacle(z, x) == Player.Instance); // &&
    //    // (Grid(z, x) - Grid(sz, sx)) < 1f;
    
    public bool Legal<T> (int z, int x) where T:BaseRollable
    {
        if (z < 0 || z >= WIDTH || x < 0 || x >= HEIGHT) return false;

        var o = Obstacle(z, x);
        if (o != null)
        {
            if (o is T) return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        for(int i = 0; i < WIDTH; i++) {
            for(int j = 0; j < HEIGHT; j++) {
                Gizmos.color = Obstacle(i, j) ? Color.red : Color.green;
                Gizmos.DrawCube(new Vector3(j, 0.5f, i), Vector3.one * 0.2f);
            }
        }
    }

    [SerializeField] float kElevateThreshold;
    [SerializeField] float kElevateResolution;
    private void Height()
    {
        var kHeightLoop = floorTiles.Count - 1;
        for(int l = 0; l < kHeightLoop; l++)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    float h = Mathf.PerlinNoise(seedX + (i + l * WIDTH) * kElevateResolution / WIDTH, seedY + (j + l * HEIGHT) * kElevateResolution / WIDTH);
                    if (h > kElevateThreshold) m_Grid[i, j] += 1;
                    else if (l == kHeightLoop - 1) m_Grid[i, j] += h;
                }
            }
        }
    }

    [SerializeField] GameObject floorTilePrototype;
    [SerializeField] List<Sprite> floorTiles;
    [SerializeField] float kAltitude = 0.3f;
    public float AltModifier => kAltitude;
    private void Render()
    {
        for (int i = 0; i < WIDTH; i++) {
            for(int j  =0; j < HEIGHT; j++) {
                var ftp = Instantiate(floorTilePrototype, transform);
                var altitude = Grid(i, j);
                ftp.transform.position = new Vector3(j, altitude * kAltitude, i);
                // ftp.transform.localScale = new Vector3(1, altitude * kAltitude, 1);
                ftp.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = floorTiles[Mathf.FloorToInt(altitude)];
            }
        }
    }

    public IEnumerable<(int, int)> Adjacent(float z, float x)
    {
        var raw = new List<(int, int)>
        {
            Fold(z-1, x),
            Fold(z+1, x),
            Fold(z, x-1),
            Fold(z, x+1),
        }.Where(p => p.Item1 >= 0 && p.Item1 < WIDTH && p.Item2 >= 0 && p.Item2 < HEIGHT);

        Debug.Log(string.Join(", ", raw));

        return raw;
    }
}
