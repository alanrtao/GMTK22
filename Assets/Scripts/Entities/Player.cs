using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public abstract class Player : BaseRollable
{
    public static Player Instance { get; private set; }

    public int MAX_STAMINA;
    protected int stamina;


    [Header("The Damage Next Attack is going to deal")]
    public int NextAtkDmg = 0;
    public int NextAtkMultiplier = 1;

    public PlayerItems MyItems;
    public int ViewWuso;
    public int Wuso
    {
        get => m_Wuso;
        set
        {
            if (value != m_Wuso) {
                m_Wuso = Mathf.Min(20, Mathf.Max(0, value));
                Debug.Log($"Player wuso changed to {m_Wuso}");
            }
        }
    }
    protected int m_Wuso = 0;

    void Awake()
    {
        Instance = this;
        m_HP = MAX_HP;
    }

    public void GoToNextScene()
    {
        blocked = true;
        DontDestroyOnLoad(gameObject);
        if (!IsInPlayableLevel)
            SceneManager.sceneLoaded += (prev, next) => StartCoroutine(Start_());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    protected override IEnumerator Start()
    {
        var vignette = profile.GetSetting<Vignette>();
        var c = vignette.color;
        c.value = Color.black;
        var i = vignette.intensity;
        i.value = 0;
        blocked = true;
        IsMoving = true;
        StartCoroutine(base.Start());
        while (IsMoving) yield return null;

        StartCoroutine(Start_());
    }

    protected IEnumerator Start_()
    {
        if (IsInPlayableLevel)
        {
            blocked = true;
            while (!GameManager.Map.done) yield return null;
            GameManager.Map.SetObstacle(this);
            Turn();
            blocked = false;
        }
    }

    public static bool IsInPlayableLevel => true; // SceneManager.GetActiveScene().buildIndex % 2 == 0;

    public bool blocked;

    public int ViewStamina;
    protected override void Update()
    {
        ViewStamina = stamina;
        ViewWuso = Wuso;
        base.Update();
        if (blocked || IsMoving || GameManager.Pool.InEnemyTurn || SceneManager.GetActiveScene().buildIndex % 2 == 0) return;
        if (stamina > 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(Direction.UP);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(Direction.DOWN);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(Direction.LEFT);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(Direction.RIGHT);
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                Ultimate();
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                TurnEnd();
            }
        } else
        {
            TurnEnd();
        }
    }

    public abstract void Ultimate();

    [SerializeField] protected BaseAnimation normalAttackVfx, ultimateVfx;
    public virtual void Attack(int damage, BaseEnemy enemy, bool renderAnimation = true, bool addToWuso = true)
    {
        if (MyItems != null)
        {
            MyItems.ActivateAllItems(Items.ActivateStates.BeforeAttack);
        }

        Debug.Log(damage);
        NextAtkDmg += damage; 
        NextAtkDmg *= NextAtkMultiplier;

        if (this is Warrior) 
        {
            


        }
        if (addToWuso)
        {
            Wuso += NextAtkDmg;
        }
        if (renderAnimation) normalAttackVfx.gameObject.SetActive(true);
        Debug.Log($"Dealing {NextAtkDmg} damage");
        StartCoroutine(PlayOneshot(AttackAction(enemy, NextAtkDmg, renderAnimation)));
    }

    public IEnumerator PlayOneshot(ActionStage action)
    {
        blocked = true;
        float t = 0;
        action.Do(0);
        yield return new WaitForEndOfFrame();
        t += Time.deltaTime;
        while (t < action.t)
        {
            action.Do(t / action.t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        action.Do(1);
        yield return new WaitForEndOfFrame();
        blocked = false;
    }

    public override void Move(Direction d)
    {
        blocked = true;
        Orientation curr = this;
        Orientation pred = d * curr;

        foreach (var e in GameManager.Pool.Pool)
        {
            if (e == pred)
            {
                stamina = Mathf.Max(0, stamina - 1);
                var top = FindClosestCurrFace(Vector3.up);
                Debug.Log(string.Join(", ", OrderedFaces()));
                Attack(top, e);
                Debug.Log($"Player attacks enemy at {pred.position_GRD}");
                return;
            }
        }

        // Debug.Log($"Player: {curr.position_GRD} => {pred.position_GRD}");

        if (GameManager.Map.Legal<Player>(Mathf.RoundToInt(pred.position_GRD.Item1), Mathf.RoundToInt(pred.position_GRD.Item2)))
        {
            base.Move(d);
            stamina = Mathf.Max(0, stamina - 1);
        } else
        {
            Debug.Log("invalid move");
        }
        blocked = false;
    }

    public void Turn()
    {
        Debug.Log("Player turn start");
        shield = 0;
        stamina = MAX_STAMINA;
    }

    public virtual void TurnEnd()
    {
        Debug.Log("Player turn end");
        if (MyItems != null)
        {
            Player.Instance.MyItems.ActivateAllItems(Items.ActivateStates.EndTurn);
        }
        GameManager.Pool.StartEnemyTurns();
    }

    protected override void Die()
    {
        blocked = true;
        SceneManager.LoadScene(0);
    }

    [SerializeField] PostProcessProfile profile;
    const float tDamage = 0.5f;
    protected override IEnumerator TakeDamageAnimation(int damage)
    {
        blocked = true;
        Wuso += damage;

        collisionSource.GenerateImpulse(1 + Mathf.Log(damage));
        yield return new WaitForFixedUpdate();
        float t = 0;

        var vignette = profile.GetSetting<Vignette>();
        while (t < tDamage * (1 + Mathf.Log(damage)))
        {
            var p = t / tDamage;
            t += Time.deltaTime;

            var c = vignette.color;
            c.value = ColorExtensions.LerpOpaque(Color.red, Color.black, 1 - Mathf.Pow(1 - p, 3));
            vignette.color = c;

            var i = vignette.intensity;
            i.value = 0.2f * Mathf.Pow(1 - p, 3);
            vignette.intensity = i;

            yield return new WaitForEndOfFrame();
        }

        blocked = false;
    }
}
