// ===============================
// FeedbackManager.cs
//
// Gerencia o feedback visual de acertos e erros na interface do jogo.
// Exibe mensagens, controla o contador de acertos consecutivos e integra com a barra de aplausos.
// ===============================

using UnityEngine;
using TMPro; // Importa o namespace do TextMeshPro

// Classe principal do gerenciador de feedback
public class FeedbackManager : MonoBehaviour
{
    // ===============================
    // Referências e configurações
    // ===============================
    public TextMeshProUGUI feedbackText; // Referência ao componente de texto do TextMeshPro
    public float feedbackDuration = 1.5f; // Duração do feedback na tela
    public ApplauseBarManager applauseBarManager; // Referência à barra de aplausos

    private int consecutiveHits = 0; // Contador de acertos consecutivos

    // ===============================
    // Inicialização
    // ===============================
    private void Start()
    {
        feedbackText.gameObject.SetActive(false); // Esconde o texto inicialmente
    }

    // ===============================
    // Exibe feedback customizado com mensagem e cor
    // ===============================
    public void ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        // Esconde o feedback após a duração
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), feedbackDuration);
    }

    // ===============================
    // Exibe feedback de acerto e incrementa contador
    // ===============================
    public void ShowHitFeedback()
    {
        if (HasActiveNotes())
        {
            consecutiveHits++; // Incrementa o contador de acertos consecutivos
            ShowFeedback($"Acertou! x{consecutiveHits}", Color.green); // Mensagem de acerto com contador
            applauseBarManager?.OnHit();
        }
    }

    // ===============================
    // Exibe feedback de erro e reseta contador
    // ===============================
    public void ShowMissFeedback()
    {
        if (HasActiveNotes())
        {
            consecutiveHits = 0; // Reseta o contador de acertos consecutivos
            ShowFeedback("Errou!", Color.red); // Mensagem de erro com cor vermelha
            applauseBarManager?.OnMiss();
        }
    }

    // ===============================
    // Esconde o feedback visual
    // ===============================
    private void HideFeedback()
    {
        feedbackText.gameObject.SetActive(false);
    }

    // ===============================
    // Verifica se há notas ativas na cena
    // ===============================
    private bool HasActiveNotes()
    {
        // Verifica se há objetos FallingNote ativos na cena usando a nova API
        return Object.FindObjectsByType<FallingNote>(FindObjectsSortMode.None).Length > 0;
    }

    // ===============================
    // Reseta o contador de acertos consecutivos
    // ===============================
    public void ResetConsecutiveHits()
    {
        consecutiveHits = 0;
    }
}