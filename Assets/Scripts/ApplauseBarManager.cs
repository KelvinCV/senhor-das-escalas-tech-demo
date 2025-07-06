// ===============================
// ApplauseBarManager.cs
//
// Gerencia a barra de aplausos (applause bar) na interface do jogo,
// incluindo animação, feedback visual, efeitos de erro e lógica de game over.
// ===============================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Classe principal da barra de aplausos
public class ApplauseBarManager : MonoBehaviour
{
    // ===============================
    // Configurações e referências
    // ===============================
    public Image applauseBarFill; // Imagem da barra de aplausos (UI)
    public float applauseMax = 100f; // Valor máximo da barra
    public float applauseMin = 0f;   // Valor mínimo da barra
    public float applauseValue = 50f; // Valor atual da barra
    public float applauseIncrease = 10f; // Quanto aumenta ao acertar
    public float applauseDecrease = 10f; // Quanto diminui ao errar
    public float lerpSpeed = 8f; // Velocidade de interpolação visual

    private float targetFill = 0.5f; // Preenchimento alvo (0-1)
    private float initialWidth; // Largura inicial da barra

    private Coroutine currentRoutine; // Corrotina de animação de valor
    private Coroutine flashRoutine;   // Corrotina de efeito de flash

    private Color defaultColor = Color.green; // Cor padrão da barra

    // ===============================
    // Inicialização
    // ===============================
    void Start()
    {
        if (applauseBarFill != null)
        {
            initialWidth = applauseBarFill.rectTransform.sizeDelta.x;
            defaultColor = applauseBarFill.color; // Salva a cor padrão
        }

        applauseValue = Mathf.Clamp(applauseValue, applauseMin, applauseMax);
        targetFill = (applauseValue - applauseMin) / (applauseMax - applauseMin);
        UpdateBarImmediate();
    }

    // ===============================
    // Atualização visual por frame
    // ===============================
    void Update()
    {
        targetFill = (applauseValue - applauseMin) / (applauseMax - applauseMin);

        if (applauseBarFill != null)
        {
            float currentWidth = applauseBarFill.rectTransform.sizeDelta.x;
            float desiredWidth = initialWidth * targetFill;
            float newWidth = Mathf.Lerp(currentWidth, desiredWidth, Time.deltaTime * lerpSpeed);
            applauseBarFill.rectTransform.sizeDelta = new Vector2(newWidth, applauseBarFill.rectTransform.sizeDelta.y);
        }
    }

    // ===============================
    // Eventos de acerto e erro
    // ===============================
    public void OnHit()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AnimateApplauseChange(applauseIncrease, 10));
    }

    public void OnMiss()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AnimateApplauseChange(-applauseDecrease, 10));

        // Inicia o efeito de piscar/brilhar
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashBarEffect());
    }

    // ===============================
    // Animação da mudança de valor da barra
    // ===============================
    private IEnumerator AnimateApplauseChange(float totalChange, int frames)
    {
        float step = totalChange / frames;
        for (int i = 0; i < frames; i++)
        {
            if (!this.isActiveAndEnabled) yield break; // Sai se o objeto foi destruído

            applauseValue += step;
            applauseValue = Mathf.Clamp(applauseValue, applauseMin, applauseMax);

            // Checa game over
            if (applauseValue <= applauseMin)
            {
                applauseValue = applauseMin;
                var gameOverManager = Object.FindFirstObjectByType<GameOverManager>();
                if (gameOverManager != null)
                    gameOverManager.ShowGameOver();
                Object.FindFirstObjectByType<NoteSpawner>()?.SetGameOver();
                Object.FindFirstObjectByType<SoundManager>()?.PararTodosOsSons();
                Object.FindFirstObjectByType<MidiSoundManager>()?.ReiniciarMidiPlayer();
                yield break;
            }
            yield return null;
        }
    }

    // ===============================
    // Efeito visual de erro (piscar)
    // ===============================
    private IEnumerator FlashBarEffect()
    {
        if (applauseBarFill == null) yield break;

        Color flashColor = Color.red;

        applauseBarFill.color = flashColor;
        yield return new WaitForSecondsRealtime(0.15f);

        applauseBarFill.color = defaultColor; // Sempre volta à cor padrão
        if (applauseBarFill.material.HasProperty("_Glow"))
            applauseBarFill.material.SetFloat("_Glow", 1f);

        flashRoutine = null;
    }

    // ===============================
    // Atualiza a barra imediatamente (sem animação)
    // ===============================
    private void UpdateBarImmediate()
    {
        if (applauseBarFill != null)
        {
            float fill = (applauseValue - applauseMin) / (applauseMax - applauseMin);
            applauseBarFill.rectTransform.sizeDelta = new Vector2(initialWidth * fill, applauseBarFill.rectTransform.sizeDelta.y);
        }
    }

    // ===============================
    // Reseta a barra para o valor inicial
    // ===============================
    public void ResetBar()
    {
        // Para corrotinas de animação e flash
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        currentRoutine = null;
        flashRoutine = null;

        // Restaura valor e cor padrão
        applauseValue = 50f; // ou outro valor padrão
        if (applauseBarFill != null)
        {
            applauseBarFill.color = defaultColor; // Restaura a cor padrão salva
            if (applauseBarFill.material.HasProperty("_Glow"))
                applauseBarFill.material.SetFloat("_Glow", 1f);
        }
        UpdateBarImmediate();
    }
}