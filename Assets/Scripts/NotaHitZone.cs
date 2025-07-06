// ===============================
// NotaHitZone.cs
//
// Detecta e armazena a nota que está atualmente na zona de acerto (hit zone).
// Permite que outros scripts consultem qual nota está na zona para validação de acertos.
// ===============================

using UnityEngine;

// Classe responsável pela zona de acerto de cada nota
public class NotaHitZone : MonoBehaviour
{
    public string noteName; // Nome da nota associada a esta zona
    
    private FallingNote noteInZone; // Armazena a nota atualmente na zona

    // ===============================
    // Detecta entrada de uma nota na zona
    // ===============================
    private void OnTriggerEnter2D(Collider2D other)
    {
        FallingNote note = other.GetComponent<FallingNote>();
        if (note != null && note.noteName == noteName)
        {
            noteInZone = note; // Armazena a nota na zona
            float tempoEntrada = Time.timeSinceLevelLoad; // Registra o tempo de entrada (opcional)
            float duration = note.duration; // Usa a duração da nota diretamente (opcional)
        }
    }

    // ===============================
    // Detecta saída de uma nota da zona
    // ===============================
    private void OnTriggerExit2D(Collider2D other)
    {
        FallingNote note = other.GetComponent<FallingNote>();
        if (note != null && note == noteInZone)
        {
            noteInZone = null; // Limpa a referência da nota ao sair da zona
        }
    }

    // ===============================
    // Retorna a nota atualmente na zona
    // ===============================
    public FallingNote GetNoteInZone()
    {
        return noteInZone; // Retorna a nota atualmente na zona
    }
}
