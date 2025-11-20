// using UnityEngine;

// public class PlayerUnitController : UnitController
// {
//     private Vector2 moveInput;

//     // Referência ao seu UnitMovement ou o script que move seu personagem
//     private UnitMovement movement;

//     public override void Initialize(Unit unit)
//     {
//         base.Initialize(unit);  // salva o Unit na classe base

//         movement = unit.GetComponent<UnitMovement>();
//         if (movement == null)
//             Debug.LogError("PlayerUnitController: Não encontrei UnitMovement no Unit!");
//     }

//     // Aqui o Player lê inputs e toma decisões
//     public override void Tick()
//     {
//         // Se o jogo está no modo GRID, ignore o WASD
//         if (!unit.IsFreeMode)
//             return;

//         ReadInput();
//         ApplyMovement();
//     }

//     private void ReadInput()
//     {
//         // Troque por seu Input System novo depois
//         moveInput = new Vector2(
//             Input.GetAxisRaw("Horizontal"),
//             Input.GetAxisRaw("Vertical")
//         );
//     }

//     private void ApplyMovement()
//     {
//         if (movement != null)
//         {
//             movement.Move(moveInput);
//         }
//     }
// }
