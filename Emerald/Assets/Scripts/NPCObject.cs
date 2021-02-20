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
    [HideInInspector]
    public GameObject NPCTypeIcons;
    [HideInInspector]
    public TMP_Text NPCTypeText;
    [HideInInspector]
    public NPCType NPCIcons;

    public override void Awake()
    {
        base.Awake();

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
        switch (type)
        {
            case NPCType.Admin:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[0], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Guild:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[1], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.BlackSmith:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[2], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Teleport:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[3], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Appearance:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[4], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Event:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[5], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Accessories:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[6], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Books:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[7], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Bank:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[8], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Exp:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[9], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Weapons:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[10], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.Potions:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[11], NPCTypeIconLocation.position, Quaternion.identity);
                return;
            case NPCType.General:
                NPCTypeIcons = Instantiate(GameScene.NPCIcons[12], NPCTypeIconLocation.position, Quaternion.identity);
                return;
        }

    }
    
}
