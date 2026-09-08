using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(AttackController))]
[RequireComponent(typeof(CharacterSkillState))]
public class PlayerInputHandler : MonoBehaviour
{
    private static readonly string[] SkillSlotIds =
    {
        "Slot1", "Slot2", "Slot3", "Slot4", "Slot5",
        "Slot6", "Slot7", "Slot8", "Slot9", "Slot10"
    };

    private const string BasicAttackSkillId = "basic_attack";
    private const string DefaultBasicAttackSlot = "Slot1";

    private PlayerControls controls;
    private PlayerMovementController movement;
    private AttackController attackController;
    private CharacterSkillState skillState;

    // NEW — the InputAction references themselves, so Update() can poll IsPressed() each frame
    // rather than only reacting to the initial press edge via .performed.
    private InputAction[] slotActions;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementController>();
        attackController = GetComponent<AttackController>();
        skillState = GetComponent<CharacterSkillState>();
        controls = new PlayerControls();

        slotActions = new[]
        {
            controls.Gameplay.Slot1, controls.Gameplay.Slot2, controls.Gameplay.Slot3,
            controls.Gameplay.Slot4, controls.Gameplay.Slot5, controls.Gameplay.Slot6,
            controls.Gameplay.Slot7, controls.Gameplay.Slot8, controls.Gameplay.Slot9,
            controls.Gameplay.Slot10
        };
    }

    private void Start()
    {
        KeybindManager.Instance.LoadBindings(SkillSlotIds);
        skillState.OnSkillLearned += HandleSkillLearned;
        EnsureBasicAttackDefaultBinding();

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        skillState.OnSkillLearned -= HandleSkillLearned;
        controls.Gameplay.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = controls.Gameplay.Move.ReadValue<Vector2>();
        movement.SetHeldDirection(moveInput);

        // NEW — poll every held slot every frame. TryUseSkill's own cooldown gate (both the
        // optimistic local check and the server's authoritative re-check) does the actual
        // rate-limiting; this just means "keep attempting while held," not "attempting = succeeding."
        for (int i = 0; i < slotActions.Length; i++)
        {
            if (slotActions[i].IsPressed())
            {
                TryUseSlot(SkillSlotIds[i]);
            }
        }
    }

    private void TryUseSlot(string slotId)
    {
        string skillId = KeybindManager.Instance.GetSkillIdForSlot(slotId);
        if (string.IsNullOrEmpty(skillId)) return;
        attackController.TryUseSkill(skillId);
    }

    private void EnsureBasicAttackDefaultBinding()
    {
        if (!skillState.KnowsSkill(BasicAttackSkillId)) return;

        bool slotAlreadyBound = !string.IsNullOrEmpty(KeybindManager.Instance.GetSkillIdForSlot(DefaultBasicAttackSlot));
        bool basicAttackAlreadyBoundSomewhere = KeybindManager.Instance.GetSlotForSkillId(BasicAttackSkillId) != null;

        if (slotAlreadyBound || basicAttackAlreadyBoundSomewhere) return;

        KeybindManager.Instance.SetBinding(DefaultBasicAttackSlot, BasicAttackSkillId);
    }

    private void HandleSkillLearned(SkillDefinitionSO skill)
    {
        if (skill.skillId != BasicAttackSkillId) return;

        bool slotAlreadyBound = !string.IsNullOrEmpty(KeybindManager.Instance.GetSkillIdForSlot(DefaultBasicAttackSlot));
        bool basicAttackAlreadyBoundSomewhere = KeybindManager.Instance.GetSlotForSkillId(BasicAttackSkillId) != null;

        if (slotAlreadyBound || basicAttackAlreadyBoundSomewhere) return;

        KeybindManager.Instance.SetBinding(DefaultBasicAttackSlot, BasicAttackSkillId);
    }
}