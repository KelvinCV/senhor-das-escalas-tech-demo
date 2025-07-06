// ===============================
// MainMenuManager.cs
//
// Gerencia a navegação do menu principal, seleção de estágios,
// controle de música do menu e saída do jogo.
// ===============================

using UnityEngine;
using UnityEngine.SceneManagement;
using MidiPlayerTK;

// Classe principal do gerenciador do menu principal
public class MainMenuManager : MonoBehaviour
{
    // ===============================
    // Referências de UI e música
    // ===============================
    public MidiFilePlayer menuMusicPlayer; // Arraste o MidiFilePlayer do menu no Inspector

    // ===============================
    // Métodos de navegação de cenas principais
    // ===============================
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Volta pro Menu Inicial
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial"); // Carrega a cena de tutorial
    }

    public void MainGame()
    {
        SceneManager.LoadScene("MainGame"); // Carrega a cena principal do jogo
    }

    public void GameOverFinal()
    {
        SceneManager.LoadScene("GameOverFinal"); // Carrega a cena de Game Over Final
    }

    // ===============================
    // Seleção de estágios (menus de seleção)
    // ===============================
    public void StageSelect0() { SceneManager.LoadScene("StageSelect0"); }
    public void StageSelect1() { SceneManager.LoadScene("StageSelect1"); }
    public void StageSelect2() { SceneManager.LoadScene("StageSelect2"); }
    public void StageSelect3() { SceneManager.LoadScene("StageSelect3"); }
    public void StageSelect4() { SceneManager.LoadScene("StageSelect4"); }
    public void StageSelect5() { SceneManager.LoadScene("StageSelect5"); }
    public void StageSelect6() { SceneManager.LoadScene("StageSelect6"); }
    public void StageSelectExtra() { SceneManager.LoadScene("StageSelectExtra"); }

    // ===============================
    // Fases do jogo (estágios principais)
    // ===============================
    public void Stage0() { SceneManager.LoadScene("Stage0"); }
    public void Stage01() { SceneManager.LoadScene("Stage01"); }
    public void Stage02() { SceneManager.LoadScene("Stage02"); }
    public void Stage03() { SceneManager.LoadScene("Stage03"); }
    public void Stage04() { SceneManager.LoadScene("Stage04"); }
    public void Stage05() { SceneManager.LoadScene("Stage05"); }
    public void Stage06() { SceneManager.LoadScene("Stage06"); }

    // ===============================
    // Estágios extras
    // ===============================
    public void StageExtra01() { SceneManager.LoadScene("StageExtra01"); }
    public void StageExtra02() { SceneManager.LoadScene("StageExtra02"); }
    public void StageExtra03() { SceneManager.LoadScene("StageExtra03"); }
    public void StageExtra04() { SceneManager.LoadScene("StageExtra04"); }
    public void StageExtra05() { SceneManager.LoadScene("StageExtra05"); }
    public void StageExtra06() { SceneManager.LoadScene("StageExtra06"); }

    // ===============================
    // Sai do jogo (ou para o modo play no editor)
    // ===============================
    public void Sair()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ===============================
    // Liga/desliga a música do menu
    // ===============================
    public void ToggleMenuMusic()
    {
        if (menuMusicPlayer != null)
        {
            if (menuMusicPlayer.MPTK_IsPlaying)
                menuMusicPlayer.MPTK_Stop();
            else
                menuMusicPlayer.MPTK_Play();
        }
    }
}