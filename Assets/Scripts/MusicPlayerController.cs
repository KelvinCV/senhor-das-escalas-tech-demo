// ===============================
// MusicPlayerController.cs
//
// Controla a reprodução, pausa e parada da música, notas e vídeo do jogo.
// Gerencia o estado dos spawners, feedback, botões e sincronização com o VideoPlayer.
// ===============================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

// Classe principal do controlador de reprodução musical
public class MusicPlayerController : MonoBehaviour
{
    // ===============================
    // Referências de objetos e UI
    // ===============================
    public GameObject noteSpawnerObj;
    public GameObject backgroundMusicSpawnerObj;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public VideoPlayer videoPlayer; // Arraste o VideoPlayer no Inspector

    [Header("Configuração de Delay do Vídeo")]
    [Tooltip("Tempo em segundos de delay para o vídeo começar após dar play")]
    public float videoStartDelay = 2.5f; // Agora editável no Inspector

    private NoteSpawner noteSpawner;
    private BackgroundMusicSpawner bgSpawner;

    private bool isPaused = false;
    private bool isStopped = true;

    //private float pausedTimeNote = 0f;
    //private float pausedTimeBG = 0f;

    public FeedbackManager feedbackManager;

    // ===============================
    // Inicialização
    // ===============================
    void Start()
    {
        noteSpawner = noteSpawnerObj != null ? noteSpawnerObj.GetComponent<NoteSpawner>() : null;
        bgSpawner = backgroundMusicSpawnerObj != null ? backgroundMusicSpawnerObj.GetComponent<BackgroundMusicSpawner>() : null;

        playButton.onClick.AddListener(OnPlay);
        pauseButton.onClick.AddListener(OnPause);
        stopButton.onClick.AddListener(OnStop);

        OnStop(); // Sempre começa parado

        // Se for a cena final, aciona o play automaticamente
        if (SceneManager.GetActiveScene().name == "GameOverFinal")
        {
            OnPlay();
        }
    }

    // ===============================
    // Lógica do botão Play/Pause/Resume
    // ===============================
    public void OnPlay()
    {
        bool noteTerminado = noteSpawner != null && noteSpawner.MusicaTerminou();
        bool bgTerminado = bgSpawner != null && bgSpawner.MusicaTerminou();

        if (isStopped || noteTerminado || bgTerminado)
        {
            // Restart do zero
            if (noteSpawnerObj != null)
            {
                noteSpawnerObj.SetActive(false);
                noteSpawner.ResetSpawner();
                noteSpawnerObj.SetActive(true);
            }
            if (backgroundMusicSpawnerObj != null)
            {
                backgroundMusicSpawnerObj.SetActive(false);
                bgSpawner.ResetSpawner();
                backgroundMusicSpawnerObj.SetActive(true);
            }
            isStopped = false;
            isPaused = false;

            // Zera score e precisão
            var scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
                scoreManager.ResetScore();

            // Zera o contador de acertos do feedback
            if (feedbackManager != null)
                feedbackManager.ResetConsecutiveHits();

            // Inicia o vídeo com delay configurável no Inspector
            if (videoPlayer != null)
                StartCoroutine(PlayVideoWithDelay(videoStartDelay));
        }
        else if (isPaused)
        {
            // Retoma do ponto pausado
            Time.timeScale = 1f;
            noteSpawner?.ResumeFromPause();
            bgSpawner?.ResumeFromPause();
            isPaused = false;

            // Retoma o vídeo instantaneamente
            if (videoPlayer != null)
                videoPlayer.Play();
        }
        else
        {
            // Se não terminou e não está pausado, Play age como Pause
            Time.timeScale = 0f;
            noteSpawner?.Pause();
            bgSpawner?.Pause();
            isPaused = true;

            // Se o vídeo estiver ativo, pausa também
            if (videoPlayer != null)
                videoPlayer.Pause();
        }
    }

    // ===============================
    // Corrotina para delay do vídeo
    // ===============================
    private IEnumerator PlayVideoWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        videoPlayer.Play();
    }

    // ===============================
    // Lógica do botão Pause
    // ===============================
    public void OnPause()
    {
        if (!isPaused && !isStopped)
        {
            Time.timeScale = 0f;
            noteSpawner?.Pause();
            bgSpawner?.Pause();
            isPaused = true;

            if (videoPlayer != null)
                videoPlayer.Pause();
        }
    }

    // ===============================
    // Lógica do botão Stop
    // ===============================
    public void OnStop()
    {
        Time.timeScale = 1f;
        if (noteSpawnerObj != null)
            noteSpawnerObj.SetActive(false);
        if (backgroundMusicSpawnerObj != null)
            backgroundMusicSpawnerObj.SetActive(false);

        foreach (var note in GameObject.FindObjectsByType<FallingNote>(FindObjectsSortMode.None))
            Destroy(note.gameObject);

        isStopped = true;
        isPaused = false;

        if (feedbackManager != null)
            feedbackManager.ResetConsecutiveHits();

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.frame = 0; // Volta para o início
        }
    }
}