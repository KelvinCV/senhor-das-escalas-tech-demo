// ===============================
// AutoPlayerBot.cs
//
// Responsável por simular um jogador automático (bot) que acerta as notas
// automaticamente, útil para demonstração, testes ou modo autoplay.
// ===============================

using UnityEngine;
using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;

// Classe principal do bot automático
public class AutoPlayerBot : MonoBehaviour
{
    // ===============================
    // Configurações e referências
    // ===============================
    public MonoBehaviour soundManagerSource; // Fonte do gerenciador de som
    private NotaButton[] notaButtons; // Todos os botões de nota
    private NotaHitZone[] hitZones; // Todas as zonas de acerto
    private bool pronto = false; // Indica se o bot já inicializou
    public MidiStreamPlayer midiPlayer; // Player MIDI para simulação
    public int midiChannel = 0; // Canal MIDI a ser usado

    private bool modoMIDI_Esperando = false; // Se o bot está aguardando para sincronizar
    private float tempoDeEspera = 0f; // Tempo de espera acumulado

    // Mapeia notas ativas para suas corrotinas de soltura
    private Dictionary<FallingNote, Coroutine> notasAtivas = new Dictionary<FallingNote, Coroutine>();

    // ===============================
    // Inicialização do bot
    // ===============================
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f); // Pequeno delay para garantir que tudo está carregado

#if UNITY_2023_1_OR_NEWER
        notaButtons = Object.FindObjectsByType<NotaButton>(FindObjectsSortMode.None);
        hitZones = Object.FindObjectsByType<NotaHitZone>(FindObjectsSortMode.None);
#else
        notaButtons = Object.FindObjectsOfType<NotaButton>();
        hitZones = Object.FindObjectsOfType<NotaHitZone>();
#endif

        // Configura o gerenciador de som para todos os botões
        foreach (var btn in notaButtons)
        {
            if (btn != null && soundManagerSource != null)
                btn.SetSoundManagerSource(soundManagerSource);
        }

        pronto = true;
        Debug.Log("[AutoPlayerBot] Inicialização completa.");
    }

    // ===============================
    // Atualização por frame
    // ===============================
    void Update()
    {
        if (!pronto) return; // Só executa se já inicializou

        // Aguarda sincronização MIDI, se necessário
        if (modoMIDI_Esperando)
        {
            tempoDeEspera += Time.deltaTime;
            if (tempoDeEspera < 0.5f) return;
            modoMIDI_Esperando = false;
        }

        // Percorre todas as zonas de acerto
        foreach (var hitZone in hitZones)
        {
            var note = hitZone.GetNoteInZone();
            var btn = FindButtonForNote(hitZone.noteName);

            if (btn == null) continue;

            // Pressiona e agenda soltura se ainda não foi agendado
            if (note != null && !note.IsBeingDestroyed && note.IsInTriggerWith(btn))
            {
                if (!btn.IsHeld && !notasAtivas.ContainsKey(note))
                {
                    btn.SimulatedHandleButtonPress(note, midiPlayer, midiChannel);
                    Coroutine c = StartCoroutine(SoltarBotaoDepois(btn, note, note.duration));
                    notasAtivas[note] = c;
                }
            }
        }

        // Solta botões imediatamente se a nota foi destruída antes do tempo
        var notasParaRemover = new List<FallingNote>();
        foreach (var kvp in notasAtivas)
        {
            var note = kvp.Key;
            var btn = FindButtonForNote(note?.noteName);
            if (note == null || note.IsBeingDestroyed)
            {
                if (btn != null && btn.IsHeld)
                    btn.SimulatedHandleButtonRelease();
                if (kvp.Value != null)
                    StopCoroutine(kvp.Value);
                notasParaRemover.Add(note);
            }
        }
        foreach (var n in notasParaRemover)
            notasAtivas.Remove(n);
    }

    // ===============================
    // Corrotina para soltar o botão após a duração da nota
    // ===============================
    private IEnumerator SoltarBotaoDepois(NotaButton btn, FallingNote note, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (btn != null && btn.IsHeld)
            btn.SimulatedHandleButtonRelease();
        notasAtivas.Remove(note);
    }

    // ===============================
    // Busca o botão correspondente ao nome da nota
    // ===============================
    private NotaButton FindButtonForNote(string noteName)
    {
        foreach (var btn in notaButtons)
        {
            if (btn != null && btn.notaNome == noteName)
                return btn;
        }
        return null;
    }

    // ===============================
    // Pausa o bot para sincronizar com eventos MIDI
    // ===============================
    public void PausarParaEsperarMIDI()
    {
        modoMIDI_Esperando = true;
        tempoDeEspera = 0f;
    }
}
