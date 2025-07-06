// ===============================
// GameOverManager.cs
//
// Gerencia a tela de Game Over (fracasso e sucesso), pausa o jogo,
// controla o botão de reinício e para sons de aplauso via MIDI.
// ===============================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using MidiPlayerTK;

// Classe principal do gerenciador de Game Over
public class GameOverManager : MonoBehaviour
{
    // ===============================
    // Referências de UI
    // ===============================
    public GameObject gameOverPanel; // Painel de Game Over na UI (fracasso)
    public GameObject gameOverSuccessPanel; // Painel de sucesso
    public Button restartButton; // Botão de reiniciar (use o mesmo nos dois painéis)

    // ===============================
    // Inicialização
    // ===============================
    void Start()
    {
        gameOverPanel.SetActive(false);
        if (gameOverSuccessPanel != null)
            gameOverSuccessPanel.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
    }

    // ===============================
    // Exibe o painel de Game Over (fracasso)
    // ===============================
    public void ShowGameOver()
    {
        if (gameOverPanel == null)
        {
            Debug.LogWarning("[GameOverManager] gameOverPanel está null. Provavelmente foi destruído na recarga da cena.");
            return;
        }

        Debug.Log("ShowGameOver chamado!");
        gameOverPanel.SetActive(true);
        StartCoroutine(PauseAfterFrame());

        // Para o aplauso após 0.01 segundos (por precaução)
        StartCoroutine(PararAplausoAposDelay(0.01f));
    }

    // ===============================
    // Exibe o painel de sucesso
    // ===============================
    public void ShowGameOverSuccess()
    {
        if (gameOverSuccessPanel == null)
        {
            Debug.LogWarning("[GameOverManager] gameOverSuccessPanel está null.");
            return;
        }

        Debug.Log("ShowGameOverSuccess chamado!");
        gameOverSuccessPanel.SetActive(true);
        StartCoroutine(PauseAfterFrame());

        // Para o aplauso após 4 segundos
        StartCoroutine(PararAplausoAposDelay(4f));
    }

    // ===============================
    // Pausa o jogo após um frame (para garantir atualização da UI)
    // ===============================
    private IEnumerator PauseAfterFrame()
    {
        yield return null; // Espera um frame para garantir que a UI atualize
        Time.timeScale = 0f;
    }

    // ===============================
    // Para o som de aplauso via MIDI após um delay
    // ===============================
    private IEnumerator PararAplausoAposDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa WaitForSecondsRealtime pois Time.timeScale = 0
        var midiPlayer = Object.FindFirstObjectByType<MidiStreamPlayer>();
        if (midiPlayer != null)
        {
            int applausePatch = 126;
            int applauseNote = 39;
            int channel = 0;
            midiPlayer.MPTK_Channels[channel].PresetNum = applausePatch;
            MPTKEvent applauseOff = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOff,
                Value = applauseNote,
                Channel = channel,
                Velocity = 0
            };
            midiPlayer.MPTK_PlayEvent(applauseOff);
        }
    }

    // ===============================
    // Reinicia o jogo (recarrega a cena atual)
    // ===============================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        if (gameOverSuccessPanel != null)
            gameOverSuccessPanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}