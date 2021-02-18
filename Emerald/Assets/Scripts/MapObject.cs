using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Network = Emerald.Network;
using C = ClientPackets;
using UnityEngine.UI;

public class MapObject : MonoBehaviour
{
    public GameSceneManager GameScene
    {
        get { return GameManager.GameScene; }
    }
    [HideInInspector]
    public Renderer ObjectRenderer;

    private Material outlineMaterial;
    public Material OutlineMaterial
    {
        get { return outlineMaterial; }
        set
        {
            if (outlineMaterial == value) return;

            var mats = ObjectRenderer.materials.ToList();
            mats.Add(value);
            ObjectRenderer.materials = mats.ToArray();
            outlineMaterial = ObjectRenderer.materials[ObjectRenderer.materials.Length - 1];
        }
    }



    public GameObject NameLabelObject;
    public Transform NameLocation;
    [HideInInspector]
    public Renderer HealthBar;
    [HideInInspector]
    public TMP_Text NameLabel;
    [HideInInspector]
    public GameObject Model;
    [HideInInspector]
    public GameObject Parent;
    [Range(0f, 10f)]
    public float MoveSpeed;
    [Range(0f, 10f)]
    public float OutlineWidth;

    public string Name;
    [HideInInspector]
    public MonsterClass MobClass;
    [HideInInspector]
    public NPCType NPCIcons;
    public string NameTextcolour;
    public Transform NPCTypeIconLocation;
    public Image NPCTypeIcons;
    public Transform NPCTypeLocation;
    public TMP_Text NPCTypeText;


    public int Light;
    [HideInInspector]

    private bool dead;
    public bool Dead
    {
        get { return dead; }
        set
        {
            if (dead == value) return;

            dead = value;

            if (dead)
                gameObject.layer = 0;
        }
    }

    public bool Blocking = true;

    private byte percentHealth;
    public byte PercentHealth
    {
        get { return percentHealth; }
        set
        {
            if (percentHealth == value) return;
            percentHealth = value;

            if (HealthBar != null)
                HealthBar.material.SetFloat("_Fill", value / 100F);

            if (this != GameManager.User.Player) return;

            GameScene.HPGlobe.material.SetFloat("_Percent", 1 - value / 100F);
        }
    }
    public float HealthTime;

    [HideInInspector]
    public bool IsMoving;
    //[HideInInspector]
    public Vector3 TargetPosition;
    [HideInInspector]
    public Vector3 StartPosition;
    [HideInInspector]
    public float TargetDistance;

    [HideInInspector]
    public uint ObjectID;
    [HideInInspector]
    public Vector2Int CurrentLocation;
    [HideInInspector]
    public MirDirection Direction;
    [HideInInspector]
    public List<QueuedAction> ActionFeed = new List<QueuedAction>();
   // [HideInInspector]
    public MirAction CurrentAction;
    [HideInInspector]
    public int ActionType;

    private byte scale;
    public byte Scale
    {
        get { return scale; }
        set
        {
            if (scale == value) return;

            scale = value;
            float s = value / 100f;
            Parent.transform.localScale *= s;
        }
    }

    public virtual void Start()
    {
        CurrentAction = MirAction.Standing;        
        NameLabel = Instantiate(NameLabelObject, NameLocation.position, Quaternion.identity, gameObject.transform).GetComponent<TMP_Text>();

        switch (gameObject.layer)
        {
            case 9: //Monster
                SetMonbsterNameColour(MobClass);
                return;
            case 10: //NPC
                NPCoverhdead(NPCIcons);
                break;
        }
        SetNameLabel();
    }

    protected virtual void Update()
    {
        if (CurrentAction == MirAction.Standing || CurrentAction == MirAction.Dead)
        {
            SetAction();
            return;
        }

        if (IsMoving)
        {
            var distance = (TargetPosition - StartPosition) * MoveSpeed * Time.deltaTime;
            var newpos = transform.position + distance;

            if (Vector3.Distance(StartPosition, newpos) >= TargetDistance)
            {
                transform.position = new Vector3(TargetPosition.x, transform.position.y, TargetPosition.z);
                IsMoving = false;
                SetAction();
                return;
            }

            transform.position = newpos;
        }

        if (HealthBar.gameObject.activeSelf && Time.time > HealthTime)
            HealthBar.gameObject.SetActive(false);
    }

    public virtual void SetAction()
    {
    }

    public void SetNameLabel()
    {
        NameLabel.text = Name;
    }

    public virtual void OnSelect()
    {
        outlineMaterial.SetFloat("_ASEOutlineWidth", OutlineWidth);
        outlineMaterial.SetColor("_ASEOutlineColor", Color.red);
        NameLabel.gameObject.SetActive(true);
    }

    public virtual void OnDeSelect()
    {
        outlineMaterial.SetFloat("_ASEOutlineWidth", 0);
        outlineMaterial.SetColor("_ASEOutlineColor", Color.clear);
        NameLabel.gameObject.SetActive(false);
    }

    public virtual void StruckBegin()
    {
        GetComponentInChildren<Animator>()?.SetBool("Struck", true);
    }

    public virtual void StruckEnd()
    {
        GetComponentInChildren<Animator>()?.SetBool("Struck", false);
    }

    public void DieEnd()
    {
        CurrentAction = MirAction.Dead;
    }
    public void SetMonbsterNameColour(MonsterClass MobClass)
    {
        switch (MobClass)
        {
            case MonsterClass.Elite:
                NameTextcolour = "F3940C";
                SetMonbsterName();
                return;
            case MonsterClass.Boss:
                NameTextcolour = "F39E0A";
                SetMonbsterName();
                return;
        }
        SetNameLabel();

    }
    public void SetMonbsterName()
    {
        NameLabel.text = "<color=#" + NameTextcolour + ">" + Name + "</color>";
    }
    public void NPCoverhdead(NPCType type)
    {

        switch (type)
        {
            case NPCType.Admin:
                NPCTypeon("Admin", 0);
                return;
            case NPCType.Guild:
                NPCTypeon("Guild", 1);
                return;
            case NPCType.BackSmirth:
                NPCTypeon("BackSmirth", 2);
                return;
            case NPCType.Teleport:
                NPCTypeon("Teleport", 3);
                return;
            case NPCType.Appearance:
                NPCTypeon("Admin", 4);
                return;
            case NPCType.Event:
                NPCTypeon("Event", 5);
                return;
            case NPCType.Accessories:
                NPCTypeon("Accessories", 6);
                return;
            case NPCType.Books:
                NPCTypeon("Books", 7);
                return;
            case NPCType.Bank:
                NPCTypeon("Bank", 8);
                return;
            case NPCType.Exp:
                NPCTypeon("Exp", 9);
                return;
            case NPCType.Weapons:
                NPCTypeon("Weapons", 10);
                return;
            case NPCType.Potions:
                NPCTypeon("Potions", 11);
                return;
            case NPCType.General:
                NPCTypeon("General", 12);
                return;
        }

    }

    public void NPCTypeon(string Type, int image)
    {
        NPCTypeText.text = "<color=yellow>" + Type + "</color>";
        NPCTypeText = Instantiate(NameLabelObject, NPCTypeLocation.position, Quaternion.identity, gameObject.transform).GetComponent<TMP_Text>();
        NPCTypeIcons.GetComponent<SpriteRenderer>().sprite = Instantiate(GameScene.NPCIcons[image], NPCTypeIconLocation.position, Quaternion.identity, gameObject.transform);
    }
}
