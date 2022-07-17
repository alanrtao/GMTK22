using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cinemachine;

public abstract class BaseRollable : MonoBehaviour
{
    public int shield = 0;

    public int MAX_HP;
    public int HP => m_HP;

    public void ChangeHPBy(int by, bool piercing = false) {
        if (!piercing && shield > 0)
        {
            var blockedDmg = Mathf.Min(-by, (this as Player).shield);
            by += blockedDmg;
            shield -= blockedDmg;
        }

        if (by < 0)
        {
            Debug.Log($"Taking actual damage {by}");
            StartCoroutine(TakeDamageAnimation(by));
            m_HP = Mathf.Max(0, m_HP + by);
            if (m_HP == 0) Die();
        }

        else if (by > 0)
        {
            m_HP = Math.Min(MAX_HP, m_HP);
        }
    }

    [SerializeField] protected int m_HP;

    public Transform RollableRoot => m_RollableRoot;
    [SerializeField] protected Transform m_RollableRoot;

    public CinemachineVirtualCamera vCam => m_vCam;
    [SerializeField] private CinemachineVirtualCamera m_vCam;

    public int[] Faces;
    protected (int, Vector3)[] m_Faces;

    protected virtual IEnumerator Start()
    {
        while (!GameManager.Map.done) yield return new WaitForEndOfFrame();

        m_Faces = new (int, Vector3)[Faces0.Length];
        for (int i = 0; i < Faces0.Length; i++)
        {
            m_Faces[i] = (Faces0[i].Item1, Faces0[i].Item2);
        }

        transform.position += Vector3.up * GameManager.Map.Grid(((Orientation)this).position_GRD) * GameManager.Map.AltModifier;
    }

    protected virtual void Update()
    {
        if (m_Faces != null) Faces = m_Faces.Select(i => i.Item1).ToArray();
    }

    protected void Place(Orientation o, float altitude)
    {
        transform.position = new Vector3(
            o.position_GRD.Item2,
            altitude * GameManager.Map.AltModifier,
            o.position_GRD.Item1);
        m_RollableRoot.localRotation = o.rotation_LS;
    }

    public virtual void Move(Direction d) => Move(new List<Direction>() { d });

    private List<Direction> pending;
    public virtual void Move(List<Direction> ds)
    {
        if (IsMoving) return;

        pending = ds;

        IsMoving = true;
        var p0 = (Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x));
        GameManager.Map.SetObstacle(p0.Item1, p0.Item2, null);

        void terminate(Orientation end)
        {
            var p0 = (Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x));
            GameManager.Map.SetObstacle(p0.Item1, p0.Item2, this);
            IsMoving = false;
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, Mathf.RoundToInt(transform.position.z));
            //m_RollableRoot.up = new Vector3[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.left, Vector3.right }
            //    .OrderBy(s => Vector3.Distance(m_RollableRoot.up, s)).First();
            UpdateFaces(end);

            //AfterMovement <--
        }

        StartCoroutine(RLerp(terminate));
    }

    public static readonly float tMove = 0.25f;
    public bool IsMoving = false;
    protected IEnumerator RLerp(Action<Orientation> complete = null) {

        IsMoving = true;
        float t_ = 0;

        if (pending.Count == 0)
        {
            complete(this);
            yield break;
        }

        var d = pending[0];
        pending.RemoveAt(0);

        Orientation start = this;
        Orientation end = d * start;

        while (t_ < tMove) {
            var p = t_ / tMove;
            var o = Orientation.Lerp(start, end, 1 - Mathf.Pow(1 - p, 3));
            Place(o, Mathf.Lerp(
                GameManager.Map.Grid(start.position_GRD),
                GameManager.Map.Grid(end.position_GRD),
                p
                ));

            t_ += Time.deltaTime;
            yield return null;
        }

        Place(end, GameManager.Map.Grid(end.position_GRD));

        if (pending.Count == 0)
        {
            complete(end);
            yield return new WaitForSeconds(0.25f);
            IsMoving = false;
        }
        else
            StartCoroutine(RLerp(complete));
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

        for (int i = 0; i < Faces0.Length; i++)
        {
            m_Faces[i] = (m_Faces[i].Item1, rot * Faces0[i].Item2);
        }
    }

    public int FindClosestCurrFace(Vector3 dir) {
        var i = m_Faces.OrderBy(f => Vector3.Distance(dir, f.Item2)).First();
        return i.Item1;
    }

    public int FindClosestIdxFace(Vector3 dir)
    {
        var dist = float.MaxValue;
        int min = -1;
        for (int i = 0; i < m_Faces.Length; i++) {
            if (Vector3.Distance(dir, m_Faces[i].Item2) < dist) {
                min = i;
                dist = Vector3.Distance(dir, m_Faces[i].Item2);
            }
        }
        return min;
    }

    protected int[] OrderedFaces() => Faces0.Select(f => f.Item2).Select(dir => FindClosestCurrFace(dir)).ToArray();



    public static int CubeFaceToIndex(CubemapFace face)
    {
        switch (face)
        {
            case CubemapFace.NegativeZ: return 0;
            case CubemapFace.PositiveZ: return 1;
            case CubemapFace.NegativeX: return 2;
            case CubemapFace.PositiveX: return 3;
            case CubemapFace.NegativeY: return 4;
            case CubemapFace.PositiveY: return 5;
            default: return -1;
        }
    }

    public static int OriginalNumberToIndex(int original)
    {
        return Faces0.ToList().FindIndex(s => s.Item1 == original);
    }

    private void ClampedAdd(int idx, int add)
    {
        m_Faces[idx].Item1 += add;
        if (m_Faces[idx].Item1 < 0) m_Faces[idx].Item1 = 0;
    }

    public void AddToOriginalOrientationFace(int add, int original)
    {
        var idx = OriginalNumberToIndex(original);
        ClampedAdd(idx, add);
    }

    public void AddToOriginalOrientationFace(int add, CubemapFace face)
    {
        var idx = CubeFaceToIndex(face);
        ClampedAdd(idx, add);
    }


    public void AddToFace(int add, Vector3 dir)
    {
        var idx = FindClosestIdxFace(dir);
        ClampedAdd(idx, add);
    }

    public void AddToAllFaces(int add)
    {
        m_Faces = m_Faces.Select(s => {
            s.Item1 += add;
            if (s.Item1 < 0) s.Item1 = 0;
            return s;
        }).ToArray();
    }

    protected abstract void Die();

    private Vector3 bufP;
    private Quaternion bufR;
    protected ActionStage AttackAction(BaseRollable target, int attack, bool renderAnimation = true) => new ActionStage(0.25f, (t) =>
    {
        if (t == 0)
        {
            Debug.Log("Trigger HP change");
            target.ChangeHPBy(-attack);
            if (renderAnimation)
            {
                bufP = transform.position;
                bufR = RollableRoot.rotation;
            }
            return;
        } else if (t == 1)
        {
            if (renderAnimation)
            {
                transform.position = bufP;
                RollableRoot.rotation = bufR;
                bufP = Vector3.zero;
                bufR = Quaternion.identity;
            }
            return;
        }

        if (renderAnimation)
        {

            var prog = 1 - 2 * Mathf.Abs(t - 0.5f);
            var parab = 1 - (1 - prog) * (1 - prog);

            transform.position = bufP + new Vector3(0, parab * 0.2f, 0) + (target.transform.position - bufP) * 0.2f * prog;

            RollableRoot.rotation = Quaternion.AngleAxis(
                parab * 10f,
                Vector3.Cross(Vector3.up, target.transform.position - transform.position)) *
                bufR;
        }
    });

    protected ActionStage PauseAction(float t) => new ActionStage(t, _ => { });

    [SerializeField] protected CinemachineImpulseSource collisionSource;
    protected abstract IEnumerator TakeDamageAnimation(int damage);


    public class ActionStage
    {
        public float t;
        public Action<float> Do;

        public ActionStage(float t, Action<float> Do) { this.t = t; this.Do = Do; }
    }
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
            (Mathf.RoundToInt(a.position_GRD.Item1) == Mathf.RoundToInt(b.position_GRD.Item1)) &&
            (Mathf.RoundToInt(a.position_GRD.Item2) == Mathf.RoundToInt(b.position_GRD.Item2));

    public static bool operator !=(Orientation a, Orientation b) => !(a == b);

    public static implicit operator Orientation(BaseRollable mb) => new Orientation(
        (Mathf.RoundToInt(mb.transform.position.z),
        Mathf.RoundToInt(mb.transform.position.x)),
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