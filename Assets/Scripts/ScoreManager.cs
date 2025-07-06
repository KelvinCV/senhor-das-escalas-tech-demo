/*
 * ScoreManager.cs
 *
 * Gerencia a pontuação, precisão e acertos consecutivos do jogador durante a partida.
 * Atualiza a interface de pontos e precisão, controla bônus por sequência de acertos e permite resetar o progresso.
 *
 */

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    // Referência ao texto de pontuação na UI
    public TextMeshProUGUI scoreText;
    // Pontuação atual do jogador
    private int score = 0;
    // Total de notas na música
    private int totalNotes = 0;
    // Total de notas acertadas
    private int hitNotes = 0;
    // Referência ao texto de precisão na UI
    public TextMeshProUGUI accuracyText;
    // Contador de acertos consecutivos
    private int consecutiveHits = 0;
    // Guarda os tempos das notas já acertadas para evitar contagem dupla
    private HashSet<float> temposAcertados = new HashSet<float>();

    // Inicializa a UI de pontuação
    private void Start()
    {
        UpdateScoreText();
    }

    // Adiciona pontos ao score e atualiza a UI
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    // Atualiza o texto de pontuação na UI
    private void UpdateScoreText()
    {
        scoreText.text = $"Pts: {score}";
    }

    // Define o total de notas da música e reseta acertos
    public void SetTotalNotes(int total)
    {
        totalNotes = total;
        hitNotes = 0;
        UpdateAccuracyText();
    }

    // Registra um acerto, atualiza precisão e aplica bônus a cada 20 acertos seguidos
    public void RegisterHit(float time)
    {
        if (!temposAcertados.Contains(time))
        {
            temposAcertados.Add(time);
            hitNotes++;
            consecutiveHits++;
            UpdateAccuracyText();

            // Bônus de 10 pontos a cada 20 acertos consecutivos
            if (consecutiveHits > 0 && consecutiveHits % 20 == 0)
            {
                score += 10;
                UpdateScoreText();
            }
        }
    }

    // Zera o contador de acertos consecutivos ao errar uma nota
    public void RegisterMiss()
    {
        consecutiveHits = 0;
    }

    // Atualiza o texto de precisão na UI
    private void UpdateAccuracyText()
    {
        float percent = totalNotes > 0 ? (hitNotes * 100f / totalNotes) : 0f;
        accuracyText.text = $"Prec.: {percent:0}% ({hitNotes}/{totalNotes})";
    }

    // Retorna a precisão percentual atual
    public float GetAccuracyPercent()
    {
        return totalNotes > 0 ? (hitNotes * 100f / totalNotes) : 0f;
    }

    // Reseta score, acertos, sequência e tempos registrados
    public void ResetScore()
    {
        score = 0;
        hitNotes = 0;
        consecutiveHits = 0;
        temposAcertados.Clear();
        UpdateScoreText();
        UpdateAccuracyText();
    }
}