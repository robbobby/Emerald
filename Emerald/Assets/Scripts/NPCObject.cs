using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network = Emerald.Network;
using C = ClientPackets;
using UnityEngine.UI;
using TMPro;
using System;

public class NPCObject : MapObject
{
    public GameObject CameraLocation;
    public Transform NPCTypeIconLocation;
    public Image NPCTypeIcons;
    public Transform NPCTypeLocation;
    public TMP_Text NPCTypeText;
    [HideInInspector]
    public NPCType NPCIcons;

    public override void Start()
    {
        base.Start();

        Model = gameObject;
        ObjectRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        Parent = ObjectRenderer.transform.parent.gameObject;
        NPCIconsDisplay(NPCIcons);
    }

    public override void SetAction()
    {
        if (ActionFeed.Count == 0)
        {
            if (Dead)
                CurrentAction = MirAction.Dead;
            else
                CurrentAction = MirAction.Standing;
        }
        else
        {
            QueuedAction action = ActionFeed[0];
            ActionFeed.RemoveAt(0);

            CurrentAction = action.Action;
            Direction = action.Direction;
            Model.transform.rotation = ClientFunctions.GetRotation(Direction);

            switch (CurrentAction)
            {
                case MirAction.Walking:
                case MirAction.Running:
                    int steps = 1;
                    if (CurrentAction == MirAction.Running) steps = 2;

                    Vector3 targetpos = GameManager.CurrentScene.Cells[(int)action.Location.x, (int)action.Location.y].position;
                    TargetPosition = targetpos;

                    Vector2 back = ClientFunctions.Back(action.Location, Direction, steps);
                    gameObject.transform.position = GameManager.CurrentScene.Cells[(int)back.x, (int)back.y].position;

                    GameManager.CurrentScene.Cells[CurrentLocation.x, CurrentLocation.y].RemoveObject(this);
                    GameManager.CurrentScene.Cells[action.Location.x, action.Location.y].AddObject(this);

                    StartPosition = gameObject.transform.position;
                    TargetDistance = Vector3.Distance(transform.position, targetpos);
                    IsMoving = true;
                    break;
            }

            CurrentLocation = action.Location;
        }

        GetComponentInChildren<Animator>().SetInteger("CurrentAction", (int)CurrentAction);
    }

    public void NPCIconsDisplay(NPCType type)
    {

        Debug.Log(type);
        Debug.Log("starteding");
        switch (type)
        {
            case NPCType.Admin:
                NPCTypeon("Admin", 0);
                return;
            case NPCType.Guild:
                NPCTypeon("Guild", 1);
                return;
            case NPCType.BlackSmith:
                NPCTypeon("BlackSmith", 2);
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
