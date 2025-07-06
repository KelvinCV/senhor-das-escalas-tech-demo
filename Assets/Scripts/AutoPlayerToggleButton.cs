// ===============================
// AutoPlayerToggleButton.cs
//
// Gerencia o botão de ativar/desativar o AutoPlayerBot na interface do jogo.
// ===============================

using UnityEngine;
using TMPro;

// Classe do botão de toggle do AutoPlayerBot
public class AutoPlayerToggleButton : MonoBehaviour
{
    // ===============================
    // Referências de UI e Bot
    // ===============================
    public GameObject autoPlayerBotObj; // Arraste o objeto com AutoPlayerBot no Inspector
    public TextMeshProUGUI botaoTexto;  // Texto do botão (UI)

    // ===============================
    // Alterna o estado do AutoPlayerBot e atualiza o texto do botão
    // ===============================
    public void ToggleAutoPlayer()
    {
        if (autoPlayerBotObj != null)
        {
            bool novoEstado = !autoPlayerBotObj.activeSelf; // Inverte o estado atual
            autoPlayerBotObj.SetActive(novoEstado);         // Ativa/desativa o bot
            if (botaoTexto != null)
                botaoTexto.text = novoEstado ? "Desativar Bot" : "Ativar Bot"; // Atualiza texto
        }
    }
}