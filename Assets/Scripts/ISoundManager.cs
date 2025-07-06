// ===============================
// ISoundManager.cs
//
// Interface para gerenciamento de modos de som no jogo.
// Implementações devem fornecer métodos para tocar e parar notas.
// ===============================

// Interface para gerenciadores de som (SoundModeManager)
public interface ISoundManager
{
    // Toca a nota correspondente ao índice fornecido
    void TocarNota(int index);
    // Para a nota correspondente ao índice fornecido
    void PararNota(int index);
}
