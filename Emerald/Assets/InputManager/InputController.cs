// GENERATED AUTOMATICALLY FROM 'Assets/InputManager/InputController.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputController : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputController()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputController"",
    ""maps"": [
        {
            ""name"": ""UI"",
            ""id"": ""7e2ed109-e820-41cf-a13e-e38467293bb0"",
            ""actions"": [
                {
                    ""name"": ""Inventory"",
                    ""type"": ""Button"",
                    ""id"": ""790212eb-81c4-44f3-977d-991dc2c7d438"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Character"",
                    ""type"": ""Button"",
                    ""id"": ""7a4da4cf-51da-4d71-9167-72c512a4f4bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Skills"",
                    ""type"": ""Button"",
                    ""id"": ""01f7add7-1e48-47cd-9849-5c80cf3d1188"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Options"",
                    ""type"": ""Button"",
                    ""id"": ""9d67b844-cd3a-4108-aebb-523c1b35d90c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Guild"",
                    ""type"": ""Button"",
                    ""id"": ""7541bb9d-891e-4f86-8146-5946bb777895"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MiniMap"",
                    ""type"": ""Button"",
                    ""id"": ""583c0773-5ae8-438d-8845-003fcbbeb09a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""23449613-34d6-44ca-9c67-940f5a3514a3"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e50fc71a-1fc2-4f47-a859-8ff42ef62eb5"",
                    ""path"": ""<Keyboard>/f9"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27091022-5cd9-4d94-ba88-54325281d1e8"",
                    ""path"": ""<Keyboard>/f10"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Character"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""12ad3251-931b-4797-a339-60155f8fb845"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Character"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""92ace2bc-f0d0-4c14-9538-38e2488f8779"",
                    ""path"": ""<Keyboard>/f11"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Skills"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0cacaeea-3eaf-4274-9c2d-f1d43a7b76be"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Skills"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d4312e58-f729-4529-a75f-df7c411aa435"",
                    ""path"": ""<Keyboard>/f12"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Options"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d91bd967-b92a-4833-a928-feeaf3d5264d"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Options"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4a051fea-5338-48cf-b293-c26c1ede645b"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Guild"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7e41aba1-f75a-4845-b27c-c159cf43a908"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MiniMap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Inventory = m_UI.FindAction("Inventory", throwIfNotFound: true);
        m_UI_Character = m_UI.FindAction("Character", throwIfNotFound: true);
        m_UI_Skills = m_UI.FindAction("Skills", throwIfNotFound: true);
        m_UI_Options = m_UI.FindAction("Options", throwIfNotFound: true);
        m_UI_Guild = m_UI.FindAction("Guild", throwIfNotFound: true);
        m_UI_MiniMap = m_UI.FindAction("MiniMap", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Inventory;
    private readonly InputAction m_UI_Character;
    private readonly InputAction m_UI_Skills;
    private readonly InputAction m_UI_Options;
    private readonly InputAction m_UI_Guild;
    private readonly InputAction m_UI_MiniMap;
    public struct UIActions
    {
        private @InputController m_Wrapper;
        public UIActions(@InputController wrapper) { m_Wrapper = wrapper; }
        public InputAction @Inventory => m_Wrapper.m_UI_Inventory;
        public InputAction @Character => m_Wrapper.m_UI_Character;
        public InputAction @Skills => m_Wrapper.m_UI_Skills;
        public InputAction @Options => m_Wrapper.m_UI_Options;
        public InputAction @Guild => m_Wrapper.m_UI_Guild;
        public InputAction @MiniMap => m_Wrapper.m_UI_MiniMap;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Inventory.started -= m_Wrapper.m_UIActionsCallbackInterface.OnInventory;
                @Inventory.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnInventory;
                @Inventory.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnInventory;
                @Character.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCharacter;
                @Character.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCharacter;
                @Character.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCharacter;
                @Skills.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSkills;
                @Skills.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSkills;
                @Skills.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSkills;
                @Options.started -= m_Wrapper.m_UIActionsCallbackInterface.OnOptions;
                @Options.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnOptions;
                @Options.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnOptions;
                @Guild.started -= m_Wrapper.m_UIActionsCallbackInterface.OnGuild;
                @Guild.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnGuild;
                @Guild.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnGuild;
                @MiniMap.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMiniMap;
                @MiniMap.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMiniMap;
                @MiniMap.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMiniMap;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Inventory.started += instance.OnInventory;
                @Inventory.performed += instance.OnInventory;
                @Inventory.canceled += instance.OnInventory;
                @Character.started += instance.OnCharacter;
                @Character.performed += instance.OnCharacter;
                @Character.canceled += instance.OnCharacter;
                @Skills.started += instance.OnSkills;
                @Skills.performed += instance.OnSkills;
                @Skills.canceled += instance.OnSkills;
                @Options.started += instance.OnOptions;
                @Options.performed += instance.OnOptions;
                @Options.canceled += instance.OnOptions;
                @Guild.started += instance.OnGuild;
                @Guild.performed += instance.OnGuild;
                @Guild.canceled += instance.OnGuild;
                @MiniMap.started += instance.OnMiniMap;
                @MiniMap.performed += instance.OnMiniMap;
                @MiniMap.canceled += instance.OnMiniMap;
            }
        }
    }
    public UIActions @UI => new UIActions(this);
    public interface IUIActions
    {
        void OnInventory(InputAction.CallbackContext context);
        void OnCharacter(InputAction.CallbackContext context);
        void OnSkills(InputAction.CallbackContext context);
        void OnOptions(InputAction.CallbackContext context);
        void OnGuild(InputAction.CallbackContext context);
        void OnMiniMap(InputAction.CallbackContext context);
    }
}
