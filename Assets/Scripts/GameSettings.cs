// ===============================
// GameSettings.cs
//
// Gerencia as configurações globais do jogo, incluindo key bindings,
// instrumento selecionado e modo de som. Fornece métodos para carregar e salvar preferências.
// ===============================

using System.Collections.Generic;
using UnityEngine;

// Classe estática para configurações globais do jogo
public static class GameSettings
{
    // ===============================
    // Configurações globais
    // ===============================
    public static Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>(); // Bindings atuais
    public static int selectedInstrumentIndex = 0; // Índice do instrumento selecionado
    public static int selectedSoundMode = 0; // Modo de som selecionado

    // ===============================
    // Carrega os key bindings do PlayerPrefs ou usa padrão
    // ===============================
    public static void LoadBindings()
    {
        keyBindings.Clear();
        foreach (var key in NotasPadrao.Keys)
        {
            string saved = PlayerPrefs.GetString("KeyBind_" + key, NotasPadrao[key].ToString());
            if (System.Enum.TryParse(saved, out KeyCode loadedKey))
            {
                keyBindings[key] = loadedKey;
            }
            else
            {
                keyBindings[key] = NotasPadrao[key];
            }
        }
    }

    // ===============================
    // Salva os key bindings atuais no PlayerPrefs
    // ===============================
    public static void SaveBindings()
    {
        foreach (var binding in keyBindings)
        {
            PlayerPrefs.SetString("KeyBind_" + binding.Key, binding.Value.ToString());
        }
        PlayerPrefs.Save();
    }

    // ===============================
    // Dicionário padrão de notas para teclas
    // ===============================
    public static Dictionary<string, KeyCode> NotasPadrao => new Dictionary<string, KeyCode>()
    {
        {"C8vb", KeyCode.Z}, {"C#8vb", KeyCode.S}, {"D8vb", KeyCode.X}, {"D#8vb", KeyCode.D},
        {"E8vb", KeyCode.C}, {"F8vb", KeyCode.V}, {"F#8vb", KeyCode.G}, {"G8vb", KeyCode.B},
        {"G#8vb", KeyCode.H}, {"A8vb", KeyCode.N}, {"A#8vb", KeyCode.J}, {"B8vb", KeyCode.M},
        {"C", KeyCode.Q}, {"C#", KeyCode.Alpha2}, {"D", KeyCode.W}, {"D#", KeyCode.Alpha3},
        {"E", KeyCode.E}, {"F", KeyCode.R}, {"F#", KeyCode.Alpha5}, {"G", KeyCode.T},
        {"G#", KeyCode.Alpha6}, {"A", KeyCode.Y}, {"A#", KeyCode.Alpha7}, {"B", KeyCode.U},
        {"C8va", KeyCode.I}
    };
}
