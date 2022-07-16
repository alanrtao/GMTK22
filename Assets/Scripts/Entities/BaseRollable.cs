using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cinemachine;

public abstract class BaseRollable : MonoBehaviour
{
    public Transform RollableRoot => m_RollableRoot;
    [SerializeField] Transform m_RollableRoot;

    public CinemachineVirtualCamera vCam => m_vCam;
    [SerializeField] private CinemachineVirtualCamera m_vCam;

    protected (int, Vector3)[] m_Faces;

    protected virtual void Start()
    {
        m_Faces = new (int, Vector3)[Faces0.Length];
        for(int i = 0; i < Faces0.Length; i++)
        {
            m_Faces[i] = (Faces0[i].Item1, Faces0[i].Item2);
        }
    }

    protected void Place(Orientation o)
    {
        transform.position = new Vector3(
            o.position_GRD.Item2,
            transform.position.y,
            o.position_GRD.Item1);
        m_RollableRoot.localRotation = o.rotation_LS;
    }

    public virtual void Move(Direction d) => Move(new List<Direction>(){d});

    public virtual void Move(List<Direction> ds)
    {
        if (IsMoving) return;

        IsMoving = true;
        var p0 = GameManager.Map.Fold(Mathf.FloorToInt(transform.position.z), Mathf.FloorToInt(transform.position.x));
        GameManager.Map.RemoveEntityObstacle(p0);

        void terminate(Orientation end)
        {
            IsMoving = false;
            UpdateFaces(end);
        }

        if (ds.Count == 1)
        {
            StartCoroutine(RLerp(ds[0], terminate));
            return;
        }

        var lf = RLerp(ds[ds.Count - 1], terminate);
        for (var i = ds.Count - 2; i >= 0; i--) {
            var l = RLerp(ds[i], (_) => StartCoroutine(lf));
            if (i == 0) StartCoroutine(l);
        }
    }

    public static readonly float tMove = 0.25f;
    protected bool IsMoving = false;
    protected IEnumerator RLerp(Direction d, Action<Orientation> complete = null) {
        float t_ = 0;

        Orientation start = this;
        Orientation end = d * start;

        while (t_ < tMove) {
            var p = t_ / tMove;
            var o = Orientation.Lerp(start, end, 1 - Mathf.Pow(1-p, 3));
            Place(o);

            t_ += Time.deltaTime;
            yield return null;
        }

        var p1 = GameManager.Map.Fold(Mathf.FloorToInt(transform.position.z), Mathf.FloorToInt(transform.position.x));
        GameManager.Map.EntityObstacle(p1);

        Place(end);

        complete(end);
    }


    private static readonly (int, Vector3)[] Faces0 = new (int, Vector3)[]
    {
        (2, -Vector3.forward), // -Z
        (5, Vector3.forward), // +Z
        (4, Vector3.left), // -X
        (3, Vector3.right), // +X
        (6, Vector3.down), // -Y
        (1, Vector3.up)  // +Y
    };

    protected void UpdateFaces(Orientation end) {
        var rot = end.rotation_LS;

        for(int i = 0; i < Faces0.Length; i++)
        {
            Debug.Log(m_Faces[i].Item2);
            m_Faces[i] = (m_Faces[i].Item1, rot * Faces0[i].Item2);
            Debug.Log(m_Faces[i].Item2);
        }

        var ordered = OrderedFaces();
        Debug.Log(string.Join(", ", ordered));
    }

    protected int FindClosestCurrFace(Vector3 dir) {
        var i = m_Faces.OrderBy(f => Vector3.Distance(dir, f.Item2)).First();
        // Debug.Log($"{dir} :: {i.Item2} = {i.Item1}");
        return i.Item1;
    }

    protected int[] OrderedFaces() => Faces0.Select(f => f.Item2).Select(dir => FindClosestCurrFace(dir)).ToArray();
}

public class Orientation {
    public (float, float) position_GRD;
    public Quaternion rotation_LS;

    public Orientation(float z, float x) : this((z, x), Quaternion.identity) { }

    public Orientation((float, float) p, Quaternion r)
    {
        position_GRD = p;
        rotation_LS = r;
    }

    public static Orientation operator *(Direction d, Orientation o)
    {
        var zx = d.ToZX();
        var rot = d.ToQ();
        return new Orientation(
            (zx.Item1 + o.position_GRD.Item1, zx.Item2 + o.position_GRD.Item2),
            rot * o.rotation_LS);
    }

    public static Orientation Lerp(Orientation a, Orientation b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Orientation(
            (Mathf.Lerp(a.position_GRD.Item1, b.position_GRD.Item1, t),
            Mathf.Lerp(a.position_GRD.Item2, b.position_GRD.Item2, t)),
            Quaternion.Slerp(a.rotation_LS, b.rotation_LS, t));
    }

    public static bool operator ==(Orientation a, Orientation b) =>
            (Mathf.FloorToInt(a.position_GRD.Item1) == Mathf.FloorToInt(b.position_GRD.Item1)) &&
            (Mathf.FloorToInt(a.position_GRD.Item2) == Mathf.FloorToInt(b.position_GRD.Item2));

public static bool operator !=(Orientation a, Orientation b) => !(a == b);

    public static implicit operator Orientation(BaseRollable mb) => new Orientation(
        (Mathf.FloorToInt(mb.transform.position.z),
        Mathf.FloorToInt(mb.transform.position.x)),
        mb.RollableRoot.rotation);

}

/// <summary>
/// (D_X*1 + D_Z*2)
/// </summary>
public enum Direction
{
    UP = 1,
    DOWN = -1,
    LEFT = 2,
    RIGHT = -2,
    NONE = 0
};

public static class DirectionExtensions
{
    public static (int, int) ToZX(this Direction d)
    {
        switch (d)
        {
            case Direction.RIGHT: return (-1, 0);
            case Direction.DOWN: return (0, -1);
            case Direction.LEFT: return (1, 0);
            case Direction.UP: return (0, 1);
            case Direction.NONE: return (0, 0);
        }
        return (0, 0);
    }

    private static readonly Quaternion
        RIGHT = Quaternion.AngleAxis(-90, Vector3.right),
        LEFT = Quaternion.AngleAxis(90, Vector3.right),
        UP = Quaternion.AngleAxis(-90, Vector3.forward),
        DOWN = Quaternion.AngleAxis(90, Vector3.forward);

    public static Quaternion ToQ(this Direction d)
    {
        switch (d)
        {
            case Direction.RIGHT: return RIGHT;
            case Direction.DOWN: return DOWN;
            case Direction.LEFT: return LEFT;
            case Direction.UP: return UP;
            case Direction.NONE: return Quaternion.identity;
        }
        return Quaternion.identity;
    }
}