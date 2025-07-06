// ===============================
// NotaButton.cs
//
// Responsável por gerenciar a interação do botão de nota musical,
// incluindo input do usuário, animações, feedbacks e integração com
// sistemas de som e pontuação.
// ===============================

using UnityEngine;
using UnityEngine.EventSystems;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;

// Classe principal do botão de nota musical
public class NotaButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ===============================
    // Configurações e referências
    // ===============================
    public int notaIndex; // Índice da nota para o sistema de som
    public string notaNome; // Nome da nota (ex: "C", "D#")
    public ChordManager chordManager; // Gerenciador de acordes
    public KeyCode teclaAssociada; // Tecla associada (opcional)
    public bool IsHeld => isButtonHeld; // Indica se o botão está pressionado

    public Animator avatarAnimator; // Animator do avatar (opcional)
    public Animator buttonAnimator; // Animator do botão (opcional)

    public MonoBehaviour soundManagerSource; // Referência ao ISoundManager (definir no Inspector)
    private ISoundManager soundManager; // Instância do gerenciador de som

    // ===============================
    // Inicialização
    // ===============================
    public void SetSoundManagerSource(MonoBehaviour novoManager)
    {
        soundManagerSource = novoManager;
        soundManager = soundManagerSource as ISoundManager;
        Debug.Log("[NotaButton] Manager atualizado para: " + soundManager.GetType().Name);
    }

    void Start()
    {
        soundManager = soundManagerSource as ISoundManager;
        // Carrega os key bindings se ainda não foram carregados
        if (GameSettings.keyBindings.Count == 0)
        {
            GameSettings.LoadBindings();
        }
    }

    // ===============================
    // Estado interno
    // ===============================
    private bool isButtonHeld = false; // Indica se o botão está pressionado

    // ===============================
    // Manipulação de input do mouse
    // ===============================
    public void OnPointerDown(PointerEventData eventData) {
        HandleButtonPress();
    }

    public void OnPointerUp(PointerEventData eventData) {
        HandleButtonRelease();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (Input.GetMouseButton(0)) {
            HandleButtonPress();
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isButtonHeld) {
            HandleButtonRelease();
        }
    }

    // ===============================
    // Manipulação de input do teclado
    // ===============================
    void Update()
    {
        // Verifica se há binding para a nota
        KeyCode tecla = GameSettings.keyBindings.ContainsKey(notaNome) 
            ? GameSettings.keyBindings[notaNome] 
            : KeyCode.None;

        if (Input.GetKeyDown(tecla))
            HandleButtonPress();
        
        if (Input.GetKeyUp(tecla))
            HandleButtonRelease();
    }

    // ===============================
    // Lógica principal do botão
    // ===============================
    private void HandleButtonPress()
    {
        if (!isButtonHeld)
        {
            isButtonHeld = true;

            // Toca o som da nota
            soundManager?.TocarNota(notaIndex);

            // Notifica o ChordManager
            if (chordManager != null)
            {
                chordManager.NotePressed(notaNome);
            }

            // Busca a zona correspondente à nota
            GameObject hitZoneObject = GameObject.Find("HitBox_" + notaNome);
            if (hitZoneObject != null)
            {
                NotaHitZone hitZone = hitZoneObject.GetComponent<NotaHitZone>();
                FallingNote noteInZone = hitZone?.GetNoteInZone();

                if (noteInZone != null && noteInZone.IsInTriggerWith(this))
                {
                    // Processa feedback e pontuação
                    ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
                    if (scoreManager != null)
                    {
                        scoreManager.RegisterHit(noteInZone.time);
                        scoreManager.AddScore(1);
                    }

                    FeedbackManager feedbackManager = Object.FindFirstObjectByType<FeedbackManager>();
                    if (feedbackManager != null)
                    {
                        feedbackManager.ShowHitFeedback();
                    }

                    noteInZone.OnButtonPressed();
                }
                else
                {
                    // Exibe feedback de erro se não houver nota válida na zona
                    ShowMissFeedback();
                    // Também registre o erro no ScoreManager, se desejar:
                    Object.FindFirstObjectByType<ScoreManager>()?.RegisterMiss();
                }
            }
            else
            {
                Debug.LogWarning($"NotaButton: Zona de colisão 'HitBox_{notaNome}' não encontrada!");
                ShowMissFeedback();
                Object.FindFirstObjectByType<ScoreManager>()?.RegisterMiss();
            }

            // Animações
            ChangeAvatarAnimation("Pressed");
            ChangeButtonAnimation("Pressed");
        }
    }

    // Exibe feedback visual de erro
    private void ShowMissFeedback()
    {
        FeedbackManager feedbackManager = Object.FindFirstObjectByType<FeedbackManager>();
        if (feedbackManager != null)
        {
            feedbackManager.ShowMissFeedback();
        }
    }

    private void HandleButtonRelease()
    {
        if (isButtonHeld)
        {
            isButtonHeld = false;

            // Para o som da nota
            soundManager?.PararNota(notaIndex);
            
            // Notifica o ChordManager
            if (chordManager != null)
            {
                chordManager.NoteReleased(notaNome);
            }
            else
            {
                Debug.LogWarning($"NotaButton ({notaNome}): ChordManager não atribuído.");
            }

            // Animações
            ChangeAvatarAnimation("Idle");
            ChangeButtonAnimation("Released");
        }
    }

    // ===============================
    // Métodos auxiliares de animação
    // ===============================
    private void ChangeAvatarAnimation(string animationState)
    {
        if (avatarAnimator != null)
        {
            avatarAnimator.SetTrigger(animationState);
        }
    }

    private void ChangeButtonAnimation(string animationState)
    {
        if (buttonAnimator != null)
        {
            Debug.Log($"Ativando animação: {animationState} em {notaNome}");
            buttonAnimator.SetTrigger(animationState);
        }
    }

    // ===============================
    // Métodos para simulação/bot
    // ===============================
    // Versão automática do HandleButtonPress
    public void SimulatedHandleButtonPress(FallingNote note, MidiStreamPlayer midiOverride = null, int channel = 0)
    {
        if (!isButtonHeld && note != null && !note.IsBeingDestroyed && note.IsInTriggerWith(this))
        {
            isButtonHeld = true;

            soundManager ??= soundManagerSource as ISoundManager;

            // Troca de cor automática conforme o tipo de nota
            if (buttonImage != null)
            {
                if (notaNome == "C8vb" || notaNome == "C8va")
                {
                    // Vermelho temporário, depois branco
                    ChangeButtonColorTemporarily(Color.red, Color.white, note.duration);
                }
                else if (notasNaturais.Contains(notaNome))
                {
                    // Amarelo temporário, depois branco
                    ChangeButtonColorTemporarily(Color.yellow, Color.white, note.duration);
                }
                else if (notasSustenidas.Contains(notaNome))
                {
                    // Verde temporário, depois preto
                    ChangeButtonColorTemporarily(Color.green, Color.black, note.duration);
                }
            }

            // Priorize o uso direto de MidiStreamPlayer, se fornecido
            if (midiOverride != null)
            {
                int midi = MidiSoundManager.ObterNotaMidi(notaNome);
                MPTKEvent noteOn = new MPTKEvent()
                {
                    Command = MPTKCommand.NoteOn,
                    Value = midi,
                    Channel = channel,
                    Velocity = 127
                };
                midiOverride.MPTK_PlayEvent(noteOn);
                // Use a duração real da nota!
                StartCoroutine(StopNoteAfterDelay(midiOverride, midi, channel, note.duration));
            }
            else
            {
                soundManager?.TocarNota(notaIndex);
            }

            // Feedbacks e Score
            chordManager?.NotePressed(notaNome);

            Object.FindFirstObjectByType<ScoreManager>()?.RegisterHit(note.time);
            Object.FindFirstObjectByType<ScoreManager>()?.AddScore(1);
            Object.FindFirstObjectByType<FeedbackManager>()?.ShowHitFeedback();

            note.OnButtonPressed();
            ChangeAvatarAnimation("Pressed");
            ChangeButtonAnimation("Pressed");
        }
    }

    // Versão automática do HandleButtonRelease
    public void SimulatedHandleButtonRelease()
    {
        if (isButtonHeld)
        {
            isButtonHeld = false;

            soundManager ??= soundManagerSource as ISoundManager;
            soundManager?.PararNota(notaIndex);

            chordManager?.NoteReleased(notaNome);

            ChangeAvatarAnimation("Idle");
            ChangeButtonAnimation("Released");
        }
    }

    // ===============================
    // Utilitários de cor e corrotinas
    // ===============================
    private IEnumerator StopNoteAfterDelay(MidiStreamPlayer player, int midiValue, int channel, float delay)
    {
        yield return new WaitForSeconds(delay);
        MPTKEvent noteOff = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOff,
            Value = midiValue,
            Channel = channel
        };
        player.MPTK_PlayEvent(noteOff);
    }

    // Conjuntos de nomes de notas para lógica de cor
    private static readonly HashSet<string> notasNaturais = new HashSet<string>
    {
        "C8vb", "D8vb", "E8vb", "F8vb", "G8vb", "A8vb", "B8vb",
        "C", "D", "E", "F", "G", "A", "B", "C8va"
    };
    private static readonly HashSet<string> notasSustenidas = new HashSet<string>
    {
        "C#8vb", "D#8vb", "F#8vb", "G#8vb", "A#8vb",
        "C#", "D#", "F#", "G#", "A#"
    };

    public UnityEngine.UI.Image buttonImage; // Referência à imagem do botão (UI)
    private Coroutine colorRoutine; // Corrotina ativa para cor do botão

    // Troca a cor do botão temporariamente
    private void ChangeButtonColorTemporarily(Color tempColor, Color normalColor, float duration)
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(ButtonColorCoroutine(tempColor, normalColor, duration));
    }

    // Corrotina para animar a cor do botão
    private IEnumerator ButtonColorCoroutine(Color tempColor, Color normalColor, float duration)
    {
        if (buttonImage != null)
        {
            buttonImage.color = tempColor;
            yield return new WaitForSeconds(duration);
            buttonImage.color = normalColor;
        }
        colorRoutine = null;
    }
}
