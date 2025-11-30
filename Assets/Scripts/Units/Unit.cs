using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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
    public string unitName;
    public Sprite portrait;

    public List<Tile> path;

    public int maxHealth;
    public int health;
    public int attackDamage;

    public bool inCombatMode = false;
    private int originalMovementRange;

    // novo: referência ao transform/spriterenderer do sprite filho
    public Transform spriteTransform; // arraste o child que contém o SpriteRenderer ou deixe vazio para auto-find
    private SpriteRenderer _spriteRenderer;
    private float _originalSpriteScaleX;

    private Animator animator;

    public CombatHUDController hud;

    private Color _originalColor;



    void Start() {

        animator = GetComponentInChildren<Animator>();

        stats = new UnitStats(Random.Range(60, 60), Random.Range(1, 1), Random.Range(40, 40), Random.Range(100, 100));
        movementRange = Mathf.RoundToInt(stats.speed * 0.1f);
        attackRange = Mathf.RoundToInt(stats.perception * 0.05f);
        attackRange = Mathf.Clamp(attackRange, 1, int.MaxValue);
        movementLeft = movementRange;
        originalMovementRange = movementRange;

        maxHealth = 1 + Mathf.RoundToInt(stats.endurance * 0.25f);
        health = maxHealth;
        attackDamage = 1 + Mathf.RoundToInt(stats.strength * 0.05f);

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

        // Guarda a cor original do sprite
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
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

            // Verifica se path tem elementos antes de acessar
            if (path != null && path.Count > 0)
            {
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
            else
            {
                // Terminou o caminho (ou não tinha), volta pro controle do jogador
                owner.ChangeSelectUnit(this);
            }
        }
    }


    public void SetHighlight(bool enabled)
    {
        // Aqui você liga o que quiser:
        // - sprite outline
        // - trocar cor
        // - mostrar uma borda
        // - ativar um GameObject filho
        // - trocar material
        if (_spriteRenderer != null)
            _spriteRenderer.color = enabled ? Color.yellow : Color.white;

        // Se tiver um GameObject highlight, seria:
        // highlightObject.SetActive(enabled);
    }
    // public void OnPointerDown(PointerEventData eventData)
    //     {
    //         if (!inCombatMode) return;
    //         if (!hud.moveModeActive) return;  // só deixa clicar no modo mover
    //         if (owner != Player.ActivePlayer) return;

    //         // Desliga modo mover
    //         hud.moveModeActive = false;

    //         // Remove highlight de todas as units
    //         foreach (Unit u in owner.playerUnits)
    //             u.SetHighlight(false);

    //         // Agora sim, seleciona esta Unit
    //         owner.ChangeSelectUnit(this);
    //     }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!inCombatMode) return;

        Player p = Player.ActivePlayer;

        // ============================
        // 1) Clicar na própria Unit → Selecionar
        // ============================
        if (owner == p)
        {
            // Só deixa clicar se estiver no modo mover
            if (!hud.moveModeActive) return;

            hud.moveModeActive = false;

            // remove highlight
            foreach (Unit u in owner.playerUnits)
                u.SetHighlight(false);

            // seleciona
            owner.ChangeSelectUnit(this);
            return;
        }

        // ============================
        // 2) Clicar numa Unit inimiga → tentativa de ataque
        // ============================
        Unit attacker = p.selectedUnit;
        Unit target = this;

        // Se não tem atacante selecionado, não faz nada
        if (!attacker) return;

        // Só ataca se for o turno do jogador (verifica se p é o jogador ativo)
        if (p != Player.ActivePlayer) return;

        // Pegar tiles
        Tile targetTile = CombatManager.instance.gridManager.GetTile(target.gridPosition);
        Tile attackerTile = CombatManager.instance.gridManager.GetTile(attacker.gridPosition);

        // Range check (distância manhattan)
        int dist = CombatManager.instance.gridManager.GetHeuristic(targetTile, attackerTile);
        if (dist > attacker.attackRange)
        {
            // Fora do range → tenta mover para perto
            Tile closest = CombatManager.instance.gridManager.GetClosestAttackTile(target, attacker);

            // Se já está parado e não tem caminho → não faz nada
            if (attacker.path.Count == 0)
            {
                attacker.MoveTo(closest.transform.position, closest.gridPosition);
            }
            else
            {
                // Já está se movendo tentando alcançar
                attacker.MoveTo(closest.transform.position, closest.gridPosition);
            }

            return;
        }

        // ============================
        // 3) Dentro do range → ataque direto!
        // ============================
        attacker.Attack(target);
    }


    // Adds health
    public void Heal(int amount)
    {
        health += amount;
        if(health > maxHealth) health = maxHealth;
    }

    // Removes health
    public void TakeDamage(int amount)
    {
        health -= amount;
        
        // Inicia o efeito de dano
        StartCoroutine(DamageEffect());
        
        if(health <= 0)
        {
            StartCoroutine(DeathAnimation());
        }
    }

    private IEnumerator DeathAnimation()
    {
        // Desativa movimento e interações
        inCombatMode = false;
        
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            // animator.SetTrigger("Death"); // Se tiver uma animação de morte
        }

        // Rotaciona para ficar deitado (90 graus no eixo Z)
        float rotationDuration = 0.5f;
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 90f);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, progress);
            yield return null;
        }

        transform.rotation = endRotation;

        // Espera um pouco deitado
        // yield return new WaitForSeconds(1f);

        // // Remove da lista de unidades
        // owner.playerUnits.Remove(this);
        
        // // Destrói o GameObject
        // Destroy(gameObject);
    }

    private IEnumerator DamageEffect()
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 originalPosition = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Fica vermelha
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(Color.red, _originalColor, progress);
            }

            // Tremida
            float shake = Mathf.Sin(elapsed * 20f) * 0.05f;
            transform.position = originalPosition + new Vector3(shake, 0, 0);

            yield return null;
        }

        // Volta ao normal
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
        
        transform.position = originalPosition;
    }

    // Deals damage to target unit
    public void Attack(Unit target)
    {
        Tile attackerTile = CombatManager.instance.gridManager.GetTile(gridPosition);
        Tile targetTile = CombatManager.instance.gridManager.GetTile(target.gridPosition);

        if(CombatManager.instance.gridManager.GetHeuristic(attackerTile, targetTile) <= attackRange)
        {
            target.TakeDamage(attackDamage);
        }
    }

}
