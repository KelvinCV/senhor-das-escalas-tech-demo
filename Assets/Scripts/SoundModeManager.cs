/*
 * SoundModeManager.cs
 *
 * Gerencia a alternância entre modos de som (MIDI/WAV) e instrumentos MIDI no jogo.
 * Responsável por atualizar dropdowns, ativar/desativar gerenciadores de som e sincronizar botões de nota.
 *
 */

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundModeManager : MonoBehaviour
{
    // Referência ao GameObject do gerenciador MIDI
    public GameObject midiManager;
    // Referência ao GameObject do gerenciador WAV
    public GameObject wavManager;
    // Dropdown para seleção do modo de som (MIDI/WAV)
    public TMP_Dropdown dropdownModoSom;
    // Dropdown para seleção do instrumento MIDI
    public TMP_Dropdown dropdownInstrumentoMIDI;

    // Instância singleton para persistência entre cenas
    private static SoundModeManager instance;

    // Garante singleton e persistência do objeto
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Inicializa o estado dos dropdowns e gerenciadores ao iniciar
    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // Inscreve no evento de troca de cena
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Remove inscrição no evento de troca de cena
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Atualiza referências e listeners ao carregar uma nova cena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        midiManager = GameObject.Find("MidiSoundManager");
        wavManager = GameObject.Find("SoundManager");

#if UNITY_2023_1_OR_NEWER
        var dropdowns = Object.FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None);
#else
        var dropdowns = Object.FindObjectsOfType<TMP_Dropdown>();
#endif
        foreach (var dd in dropdowns)
        {
            if (dd.gameObject.name.Contains("SoundModeDropdown"))
                dropdownModoSom = dd;
            else if (dd.gameObject.name.Contains("InstrumentMIDIDropdown"))
                dropdownInstrumentoMIDI = dd;
        }

        if (dropdownModoSom != null)
        {
            dropdownModoSom.onValueChanged.RemoveAllListeners();
            dropdownModoSom.onValueChanged.AddListener(AlternarModoSom);
        }

        if (dropdownInstrumentoMIDI != null)
        {
            dropdownInstrumentoMIDI.onValueChanged.RemoveAllListeners();
            dropdownInstrumentoMIDI.onValueChanged.AddListener(AlternarInstrumentoMIDI);
        }

        StartCoroutine(SyncDropdownsAfterFrame());
    }

    // Sincroniza valores dos dropdowns após o frame para garantir UI atualizada
    private IEnumerator SyncDropdownsAfterFrame()
    {
        yield return null;

        int modoSomSalvo = PlayerPrefs.GetInt("ModoSom", 0);
        int instrumentoMIDISalvo = PlayerPrefs.GetInt("InstrumentoMIDI", 0);

        Debug.Log($"[SoundModeManager] ModoSom carregado: {modoSomSalvo}");
        Debug.Log($"[SoundModeManager] InstrumentoMIDI carregado: {instrumentoMIDISalvo}");

        if (dropdownModoSom != null)
        {
            dropdownModoSom.value = modoSomSalvo;
            dropdownModoSom.RefreshShownValue();
        }

        if (dropdownInstrumentoMIDI != null)
        {
            dropdownInstrumentoMIDI.value = instrumentoMIDISalvo;
            dropdownInstrumentoMIDI.RefreshShownValue();
        }

        AlternarModoSom(modoSomSalvo);
        AtualizarVisibilidadeDropdownInstrumento(modoSomSalvo);
    }

    // Alterna entre modo MIDI (0) e WAV (1), salva preferência e atualiza UI
    public void AlternarModoSom(int valor)
    {
        PlayerPrefs.SetInt("ModoSom", valor);
        Debug.Log($"[SoundModeManager] ModoSom salvo: {valor}");

        if (valor == 0)
            StartCoroutine(AtivarMidiComDelay());
        else if (valor == 1)
            UsarWav();

        AtualizarVisibilidadeDropdownInstrumento(valor);
    }

    // Ativa o MIDI com delay para garantir inicialização correta
    private IEnumerator AtivarMidiComDelay()
    {
        if (midiManager != null) midiManager.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (wavManager != null) wavManager.SetActive(false);

        MidiSoundManager midiSoundScript = midiManager?.GetComponent<MidiSoundManager>();
        if (midiSoundScript != null)
        {
            midiSoundScript.ReiniciarMidiPlayer();

            int instrumentoMIDISalvo = PlayerPrefs.GetInt("InstrumentoMIDI", 0);
            Debug.Log($"[SoundModeManager] Reaplicando instrumento MIDI salvo: {instrumentoMIDISalvo}");
            midiSoundScript.TrocarInstrumento(instrumentoMIDISalvo);
        }
        else
        {
            Debug.LogError("[SoundModeManager] MidiSoundManager não encontrado!");
        }

        // Atualiza todos os NotaButton para usar o novo manager
        AtualizarNotaButtons(midiSoundScript);

        Debug.Log("[SoundModeManager] Modo MIDI ativado.");
    }

    // Ativa o modo WAV e para todos os sons MIDI
    public void UsarWav()
    {
        if (wavManager != null) wavManager.SetActive(true);
        if (midiManager != null) midiManager.SetActive(false);

        SoundManager wavSoundScript = wavManager?.GetComponent<SoundManager>();
        if (wavSoundScript != null)
        {
            wavSoundScript.PararTodosOsSons();
        }
        else
        {
            Debug.LogError("[SoundModeManager] SoundManager não encontrado!");
        }

        AtualizarNotaButtons(wavSoundScript);
        Debug.Log("[SoundModeManager] Modo WAV ativado.");
    }

    // Troca o instrumento MIDI selecionado e salva preferência
    public void AlternarInstrumentoMIDI(int instrumentoIndex)
    {
        PlayerPrefs.SetInt("InstrumentoMIDI", instrumentoIndex);
        Debug.Log($"[SoundModeManager] Instrumento MIDI salvo: {instrumentoIndex}");

        MidiSoundManager midiSoundScript = midiManager?.GetComponent<MidiSoundManager>();
        if (midiSoundScript != null)
        {
            Debug.Log($"[SoundModeManager] Alterando para instrumento {instrumentoIndex}");
            midiSoundScript.TrocarInstrumento(instrumentoIndex);
        }
        else
        {
            Debug.LogError("[SoundModeManager] MidiSoundManager não encontrado!");
        }
    }

    // Mostra ou esconde o dropdown de instrumento MIDI conforme o modo
    private void AtualizarVisibilidadeDropdownInstrumento(int modoSom)
    {
        if (dropdownInstrumentoMIDI != null)
        {
            dropdownInstrumentoMIDI.gameObject.SetActive(modoSom == 0);
        }
    }

    // Atualiza todos os NotaButton da cena para usar o novo ISoundManager
    private void AtualizarNotaButtons(ISoundManager novoManager)
    {
#if UNITY_2023_1_OR_NEWER
        var notaButtons = Object.FindObjectsByType<NotaButton>(FindObjectsSortMode.None);
#else
        var notaButtons = Object.FindObjectsOfType<NotaButton>();
#endif
        foreach (var notaBtn in notaButtons)
        {
            notaBtn.SetSoundManagerSource((MonoBehaviour)novoManager);
            Debug.Log($"[SoundModeManager] Atualizando botão: {notaBtn.name} para {novoManager.GetType().Name}");
        }
    }
}
