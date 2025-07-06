// ===============================
// FallingNote.cs
//
// Responsável por controlar o comportamento das notas que caem na tela,
// incluindo movimento, detecção de colisão, feedback visual e destruição.
// ===============================

using UnityEngine;

// Classe principal da nota que cai
public class FallingNote : MonoBehaviour
{
    // ===============================
    // Configurações e estado
    // ===============================
    public string noteName; // Nome da nota (ex: "C", "D#")
    public float time; // Tempo de spawn ou referência musical
    public float fallSpeed = 200f; // Velocidade de queda da nota
    public bool debugTiming = false; // Ativa logs de tempo para debug
    private float spawnTime; // Momento em que a nota foi criada

    private bool isInTrigger = false; // Indica se está na zona de acerto
    private NotaButton currentButton; // Botão atualmente associado

    private float destroyThreshold = -50f; // Limite inferior para destruir a nota

    public float duration { get; set; } // Duração da nota (para notas longas)
    private bool isBeingDestroyed = false; // Indica se a nota está sendo destruída
    private float destroyTimer = 0f; // Tempo acumulado para interpolação de destruição

    public bool IsBeingDestroyed { get; private set; } = false; // Propriedade pública para consulta externa

    // ===============================
    // Inicialização
    // ===============================
    void Start()
    {
        spawnTime = Time.time; // Armazena o tempo de spawn
    }

    // ===============================
    // Atualização por frame
    // ===============================
    void Update()
    {
        if (isBeingDestroyed)
        {
            // Interpola a escala da nota com base no tempo acumulado
            destroyTimer += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(1f, 0f, destroyTimer / duration);
            transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // Destrói o objeto quando a interpolação termina
            if (destroyTimer >= duration)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Faz a nota cair
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // Verifica se a nota saiu da tela
            if (transform.position.y < destroyThreshold)
            {
                Destroy(gameObject);
            }
        }
    }

    // ===============================
    // Detecção de entrada na zona de acerto
    // ===============================
    void OnTriggerEnter2D(Collider2D other)
    {
        NotaButton button = other.GetComponent<NotaButton>();
        if (button != null && button.notaNome == noteName)
        {
            isInTrigger = true;
            currentButton = button;

            if (debugTiming)
            {
                float timeToReach = Time.time - spawnTime;
                Debug.Log($"[FallingNote] Nota {noteName} chegou ao botão após {timeToReach:F3} segundos.");
            }
        }
    }

    // ===============================
    // Detecção de saída da zona de acerto
    // ===============================
    void OnTriggerExit2D(Collider2D other)
    {
        NotaButton button = other.GetComponent<NotaButton>();
        if (button != null && button.notaNome == noteName)
        {
            isInTrigger = false;
            currentButton = null;
        }
    }

    // ===============================
    // Lógica de destruição ao pressionar o botão
    // ===============================
    public void OnButtonPressed(bool forcar = false)
    {
        if ((isInTrigger || forcar) && !isBeingDestroyed)
        {
            isBeingDestroyed = true; // Inicia o processo de destruição
            IsBeingDestroyed = true; // Marca a nota como destruída
            destroyTimer = 0f; // Reseta o timer

            // Remove a nota da zona de colisão imediatamente
            isInTrigger = false;
            currentButton = null;

            Debug.Log($"[FallingNote] Nota {noteName} foi pressionada e está sendo destruída.");
        }
    }

    // ===============================
    // Verifica se a nota está na zona de acerto do botão
    // ===============================
    public bool IsInTriggerWith(NotaButton button)
    {
        return isInTrigger && currentButton == button;
    }
}
