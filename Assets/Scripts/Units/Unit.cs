using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Unit : MonoBehaviour, IPointerDownHandler
{
    public Vector2Int gridPosition;
    public float moveSpeed = 5f;
    public bool isMoving = false;
    private Vector3 targetPosition;

    private const float STOPPING_DISTANCE = 0.01f;

    public UnitStats stats;
    public int movementRange = 3;
    public int attackRange = 1;
    public int movementLeft;

    public Player owner;

    public List<Tile> path;

    public bool inCombatMode = false;
    private int originalMovementRange;

    // novo: referência ao transform/spriterenderer do sprite filho
    public Transform spriteTransform; // arraste o child que contém o SpriteRenderer ou deixe vazio para auto-find
    private SpriteRenderer _spriteRenderer;
    private float _originalSpriteScaleX;

    private Animator animator;


    void Start() {

        animator = GetComponentInChildren<Animator>();

        stats = new UnitStats(Random.Range(60, 60), Random.Range(1, 1));
        movementRange = Mathf.RoundToInt(stats.speed * 0.1f);
        attackRange = Mathf.RoundToInt(stats.perception * 0.05f);
        attackRange = Mathf.Clamp(attackRange, 1, int.MaxValue);
        movementLeft = movementRange;
        originalMovementRange = movementRange;

        // tenta achar SpriteRenderer no filho se não atribuído
        if (spriteTransform == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null)
                spriteTransform = _spriteRenderer.transform;
        }
        else
        {
            _spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }

        if (spriteTransform != null)
            _originalSpriteScaleX = spriteTransform.localScale.x;
        else
            Debug.LogWarning($"Unit {name}: spriteTransform não encontrado. A orientação horizontal não funcionará.");
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }
    public static Unit Selected
    {
        get
        {
            Player p = Player.ActivePlayer;
            if (p == null) return null;
            return p.selectedUnit;
        }
    }


    public void SetCombatMode(bool enabled)
    {
        inCombatMode = enabled;
        if (!enabled)
        {
            movementLeft = int.MaxValue;
        }
        else
        {
            movementLeft = movementRange;
        }
        
    }

    public void MoveTo(Vector3 targetPos, Vector2Int gridPos)
    {
        targetPosition = targetPos;
        gridPosition = gridPos;
        isMoving = true;

        if (animator != null)
        {
            // Ativa animação de movimento
            animator.SetBool("IsWalking", true);

            // Calcula a direção do movimento
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Atualiza os parâmetros da animação (para andar e olhar na direção certa)
            animator.SetFloat("InputX", direction.x);
            animator.SetFloat("InputY", direction.y);

            // Também guarda a última direção (para quando parar)
            animator.SetFloat("LastInputX", direction.x);
            animator.SetFloat("LastInputY", direction.y);
        }
    }


    

    private void HandleMovement()
    {
        if (!isMoving) return;
        if (!inCombatMode) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < STOPPING_DISTANCE)
        {
            transform.position = targetPosition;
            isMoving = false;

            if (animator != null)
            {
                // Só desliga a animação, sem mudar LastInput
                animator.SetBool("IsWalking", false);
            }

            movementLeft -= path[0].moveCost;
            path.RemoveAt(0);

            if (path.Count > 0)
            {
                // Continua para o próximo tile no caminho
                MoveTo(path[0].transform.position, path[0].gridPosition);
            }
            else
            {
                // Terminou o caminho, volta pro controle do jogador
                owner.ChangeSelectUnit(this);
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!inCombatMode) return;
        if (owner != null)
        {
            owner.ChangeSelectUnit(this);

        }
        else
        {
            Debug.LogError("Unit has no owner assigned!"); // Debug log
        }
    }
}
