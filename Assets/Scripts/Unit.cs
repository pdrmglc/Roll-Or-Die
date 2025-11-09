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

    void Start() {
        stats = new UnitStats(Random.Range(0, 100), Random.Range(0, 100));
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

    public void MoveTo(Vector3 position, Vector2Int gridPos)
    {
        // ajusta a face antes de iniciar o movimento
        UpdateFacingTowards(position);

        targetPosition = position;
        gridPosition = gridPos;
        isMoving = true;
    }

    // novo método para definir a orientação esquerda/direita
    private void UpdateFacingTowards(Vector3 worldTargetPosition)
    {
        if (spriteTransform == null) return;
        var cam = Camera.main;
        if (cam == null) return;

        // converte posições para tela e checa a diferença horizontal em pixels
        Vector3 screenCurrent = cam.WorldToScreenPoint(transform.position);
        Vector3 screenTarget = cam.WorldToScreenPoint(worldTargetPosition);
        float dx = screenTarget.x - screenCurrent.x;

        // threshold em pixels para evitar flips em movimentos quase verticais/diagonais
        if (Mathf.Abs(dx) < 5f) return;

        bool faceLeft = dx < 0f;

        // prefira usar flipX se houver SpriteRenderer, caso contrário ajuste localScale.x
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = faceLeft;
        }
        else
        {
            Vector3 s = spriteTransform.localScale;
            s.x = Mathf.Abs(_originalSpriteScaleX) * (faceLeft ? -1f : 1f);
            spriteTransform.localScale = s;
        }
    }

    private void HandleMovement()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < STOPPING_DISTANCE)
        {
            transform.position = targetPosition;
            isMoving = false;

            movementLeft -= path[0].moveCost;

            path.RemoveAt(0);
            if (path.Count > 0)
            {
                MoveTo(path[0].transform.position, path[0].gridPosition);
            }
            else
            {
                owner.ChangeSelectUnit(this);
            }
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
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
