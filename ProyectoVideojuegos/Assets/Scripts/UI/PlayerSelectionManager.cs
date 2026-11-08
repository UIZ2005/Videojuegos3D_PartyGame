
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// ==========================================
// PLAYER SELECTION MANAGER
// ==========================================

public class PlayerSelectionManager : MonoBehaviour
{
    // ==========================================
    // CLASE: INTERFAZ DE CADA JUGADOR
    // ==========================================

    [System.Serializable]
    public class PlayerCardUI
    {
        [Header("Panel principal")]
        public GameObject panel;

        [Header("Elementos gráficos RawImage")]

        public RawImage personaje;
        public RawImage nombre;

        public RawImage iconoHabilidad1;
        public RawImage textoHabilidad1;

        public RawImage iconoHabilidad2;
        public RawImage textoHabilidad2;
    }

    // ==========================================
    // CLASE: RECURSOS DE CADA PERSONAJE
    // ==========================================

    [System.Serializable]
    public class CharacterVisuals
    {
        [Header("Identificación")]
        public string nombrePersonaje;

        [Header("Recursos gráficos - Sprite")]

        public Sprite personaje;
        public Sprite nombre;

        public Sprite iconoHabilidad1;
        public Sprite textoHabilidad1;

        public Sprite iconoHabilidad2;
        public Sprite textoHabilidad2;

        [Header("Fondo de la tarjeta")]
        public Sprite fondo;

        [Header("Recursos para el videojuego")]
        public GameObject characterPrefab;
    }

    // ==========================================
    // CLASE: ESTADO DE CADA JUGADOR
    // ==========================================

    private class PlayerSlot
    {
        public bool active;
        public bool confirmed;

        public InputDevice device;

        public int characterIndex;

        public int lastHorizontalDirection;

        public int joinedFrame;
    }

    // ==========================================
    // CONFIGURACIÓN DEL INSPECTOR
    // ==========================================

    [Header("TARJETAS DE LOS JUGADORES")]

    [Tooltip("Orden: Player 1, Player 2, Player 3 y Player 4.")]
    public PlayerCardUI[] playerCards = new PlayerCardUI[4];

    [Header("RECURSOS GRÁFICOS DE LOS PERSONAJES")]

    [Tooltip("Orden: Agua, Fuego, Viento y Rayo.")]
    public CharacterVisuals[] characterVisuals = new CharacterVisuals[4];

    [Header("BOTÓN DE INICIAR")]

    public GameObject btnIniciar;

    // ==========================================
    // VARIABLES INTERNAS
    // ==========================================

    private PlayerSlot[] players = new PlayerSlot[4];

    public static List<GameObject> SelectedPlayerPrefabs =
        new List<GameObject>();

    public static List<Sprite> SelectedPlayerSprites =
        new List<Sprite>();

    public static List<InputDevice> SelectedPlayerDevices =
        new List<InputDevice>();

    private InputSystem_Actions controls;

    // ==========================================
    // CONTADORES
    // ==========================================

    public int ActivePlayerCount
    {
        get
        {
            int count = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].active)
                    count++;
            }

            return count;
        }
    }

    public int ConfirmedPlayerCount
    {
        get
        {
            int count = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].active && players[i].confirmed)
                    count++;
            }

            return count;
        }
    }

    // ==========================================
    // INICIALIZACIÓN
    // ==========================================

    private void Awake()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerSlot();

            players[i].characterIndex = -1;

            ClearPlayerUI(i);

            PlayerCardUI card = GetCard(i);

            if (card != null && card.panel != null)
            {
                card.panel.SetActive(false);
            }
        }

        SelectedPlayerPrefabs.Clear();
        SelectedPlayerSprites.Clear();
        SelectedPlayerDevices.Clear();

        UpdateStartButton();
    }

    // ==========================================
    // INPUT SYSTEM
    // ==========================================

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new InputSystem_Actions();
        }

        controls.Menu.Join.performed += OnJoinPerformed;

        controls.Menu.Navigate.performed += OnNavigatePerformed;

        controls.Menu.Navigate.canceled += OnNavigateCanceled;

        controls.Menu.Confirm.performed += OnConfirmPerformed;

        controls.Menu.Cancel.performed += OnCancelPerformed;

        controls.Menu.Enable();
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.Menu.Join.performed -= OnJoinPerformed;

        controls.Menu.Navigate.performed -= OnNavigatePerformed;

        controls.Menu.Navigate.canceled -= OnNavigateCanceled;

        controls.Menu.Confirm.performed -= OnConfirmPerformed;

        controls.Menu.Cancel.performed -= OnCancelPerformed;

        controls.Menu.Disable();
    }

    // ==========================================
    // OBTENER TARJETA
    // ==========================================

    private PlayerCardUI GetCard(int playerIndex)
    {
        if (playerCards == null ||
            playerIndex < 0 ||
            playerIndex >= playerCards.Length)
        {
            return null;
        }

        return playerCards[playerIndex];
    }

    // ==========================================
    // CONVERSIÓN SPRITE A RAWIMAGE
    // ==========================================

    private void SetSprite(RawImage target, Sprite sprite)
    {
        if (target == null)
            return;

        target.texture = sprite != null ? sprite.texture : null;

        // Si el Sprite ocupa toda la textura,
        // no es necesario modificar los UV.
        // Para sprites recortados de atlas,
        // se debe utilizar otra configuración.
    }

    // ==========================================
    // LIMPIAR INTERFAZ
    // ==========================================

    private void ClearPlayerUI(int playerIndex)
    {
        PlayerCardUI card = GetCard(playerIndex);

        if (card == null)
            return;

        SetSprite(card.personaje, null);
        SetSprite(card.nombre, null);

        SetSprite(card.iconoHabilidad1, null);
        SetSprite(card.textoHabilidad1, null);

        SetSprite(card.iconoHabilidad2, null);
        SetSprite(card.textoHabilidad2, null);

        // Limpiar fondo del panel padre.

        if (card.panel != null)
        {
            Image background = card.panel.GetComponent<Image>();

            if (background != null)
            {
                background.sprite = null;
            }
        }
    }

    // ==========================================
    // UNIR JUGADORES
    // ==========================================

    private void OnJoinPerformed(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        if (!(device is Gamepad) && !(device is Keyboard))
            return;

        if (FindPlayerByDevice(device) >= 0)
            return;

        int slot = FindFreePlayerSlot();

        if (slot < 0)
            return;

        PlayerCardUI card = GetCard(slot);

        if (card == null || card.panel == null)
            return;

        players[slot].active = true;

        players[slot].device = device;

        players[slot].joinedFrame = Time.frameCount;

        players[slot].characterIndex = FindFirstAvailableCharacter();

        card.panel.SetActive(true);

        RefreshPlayerUI(slot);

        RefreshStoredSelections();

        UpdateStartButton();
    }

    // ==========================================
    // NAVEGACIÓN DE PERSONAJES
    // ==========================================

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0 || players[playerIndex].confirmed)
            return;

        Vector2 movement = context.ReadValue<Vector2>();

        int direction = 0;

        if (movement.x < -0.5f)
            direction = -1;

        else if (movement.x > 0.5f)
            direction = 1;

        if (direction == 0)
        {
            players[playerIndex].lastHorizontalDirection = 0;
            return;
        }

        if (direction == players[playerIndex].lastHorizontalDirection)
            return;

        players[playerIndex].lastHorizontalDirection = direction;

        NavigateCharacter(playerIndex, direction);
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex >= 0)
        {
            players[playerIndex].lastHorizontalDirection = 0;
        }
    }

    // ==========================================
    // CONFIRMAR CON INPUT
    // ==========================================

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0)
            return;

        if (players[playerIndex].joinedFrame == Time.frameCount)
            return;

        ConfirmCharacter(playerIndex);
    }

    // ==========================================
    // CANCELAR / SALIR
    // ==========================================

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0)
            return;

        // Si estaba confirmado, vuelve a seleccionar.

        if (players[playerIndex].confirmed)
        {
            players[playerIndex].confirmed = false;

            RefreshStoredSelections();

            UpdateStartButton();

            return;
        }

        PlayerCardUI card = GetCard(playerIndex);

        if (card != null && card.panel != null)
        {
            card.panel.SetActive(false);
        }

        ClearPlayerUI(playerIndex);

        players[playerIndex] = new PlayerSlot();

        players[playerIndex].characterIndex = -1;

        RefreshStoredSelections();

        UpdateStartButton();
    }

    // ==========================================
    // BUSCAR JUGADORES
    // ==========================================

    private int FindPlayerByDevice(InputDevice device)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].active && players[i].device == device)
                return i;
        }

        return -1;
    }

    private int FindFreePlayerSlot()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active)
                return i;
        }

        return -1;
    }

    // ==========================================
    // DISPONIBILIDAD DE PERSONAJES
    // ==========================================

    private int FindFirstAvailableCharacter()
    {
        if (characterVisuals == null)
            return -1;

        for (int i = 0; i < characterVisuals.Length; i++)
        {
            if (IsCharacterAvailable(i, -1))
                return i;
        }

        return -1;
    }

    private bool IsCharacterAvailable(int characterIndex, int requestingPlayer)
    {
        if (characterVisuals == null ||
            characterIndex < 0 ||
            characterIndex >= characterVisuals.Length)
        {
            return false;
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (i != requestingPlayer &&
                players[i].active &&
                players[i].confirmed &&
                players[i].characterIndex == characterIndex)
            {
                return false;
            }
        }

        return true;
    }

    // ==========================================
    // CAMBIAR PERSONAJE
    // ==========================================

    private void NavigateCharacter(int playerIndex, int direction)
    {
        if (players[playerIndex].confirmed)
            return;

        if (characterVisuals == null || characterVisuals.Length == 0)
            return;

        int count = characterVisuals.Length;

        int currentIndex = players[playerIndex].characterIndex;

        if (currentIndex < 0)
        {
            currentIndex = FindFirstAvailableCharacter();

            if (currentIndex < 0)
                return;
        }

        for (int i = 0; i < count; i++)
        {
            currentIndex += direction;

            if (currentIndex < 0)
                currentIndex = count - 1;

            else if (currentIndex >= count)
                currentIndex = 0;

            if (IsCharacterAvailable(currentIndex, playerIndex))
            {
                players[playerIndex].characterIndex = currentIndex;

                RefreshPlayerUI(playerIndex);

                return;
            }
        }
    }

    // ==========================================
    // ACTUALIZAR TARJETA COMPLETA
    // ==========================================

    private void RefreshPlayerUI(int playerIndex)
    {
        PlayerCardUI card = GetCard(playerIndex);

        if (card == null)
            return;

        int characterIndex = players[playerIndex].characterIndex;

        if (characterVisuals == null ||
            characterIndex < 0 ||
            characterIndex >= characterVisuals.Length)
        {
            ClearPlayerUI(playerIndex);
            return;
        }

        CharacterVisuals character = characterVisuals[characterIndex];

        if (character == null)
            return;

        // 1. IMAGEN DEL PERSONAJE

        SetSprite(card.personaje, character.personaje);

        // 2. NOMBRE DEL PERSONAJE

        SetSprite(card.nombre, character.nombre);

        // 3. PRIMER ICONO

        SetSprite(card.iconoHabilidad1, character.iconoHabilidad1);

        // 4. TEXTO DEL PRIMER ICONO

        SetSprite(card.textoHabilidad1, character.textoHabilidad1);

        // 5. SEGUNDO ICONO

        SetSprite(card.iconoHabilidad2, character.iconoHabilidad2);

        // 6. TEXTO DEL SEGUNDO ICONO

        SetSprite(card.textoHabilidad2, character.textoHabilidad2);

        // 7. FONDO DEL PANEL PADRE

        if (card.panel != null)
        {
            Image background = card.panel.GetComponent<Image>();

            if (background != null)
            {
                background.sprite = character.fondo;
            }
        }
    }

    // ==========================================
    // CONFIRMAR PERSONAJE
    // ==========================================

    private void ConfirmCharacter(int playerIndex)
    {
        if (players[playerIndex].confirmed)
            return;

        int characterIndex = players[playerIndex].characterIndex;

        if (characterIndex < 0 ||
            characterIndex >= characterVisuals.Length)
            return;

        if (!IsCharacterAvailable(characterIndex, playerIndex))
        {
            players[playerIndex].characterIndex =
                FindFirstAvailableCharacter();

            RefreshPlayerUI(playerIndex);

            return;
        }

        CharacterVisuals character = characterVisuals[characterIndex];

        if (character.characterPrefab == null ||
            character.personaje == null)
            return;

        players[playerIndex].confirmed = true;

        RefreshStoredSelections();

        RefreshOtherPlayerSelections();

        UpdateStartButton();
    }

    // ==========================================
    // ACTUALIZAR SELECCIONES DE OTROS JUGADORES
    // ==========================================

    private void RefreshOtherPlayerSelections()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active || players[i].confirmed)
                continue;

            int currentIndex = players[i].characterIndex;

            if (!IsCharacterAvailable(currentIndex, i))
            {
                players[i].characterIndex = FindFirstAvailableCharacter();

                RefreshPlayerUI(i);
            }
        }
    }

    // ==========================================
    // GUARDAR SELECCIONES
    // ==========================================

    private void RefreshStoredSelections()
    {
        SelectedPlayerPrefabs.Clear();

        SelectedPlayerSprites.Clear();

        SelectedPlayerDevices.Clear();

        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active || !players[i].confirmed)
                continue;

            int characterIndex = players[i].characterIndex;

            if (characterIndex < 0 ||
                characterIndex >= characterVisuals.Length)
                continue;

            CharacterVisuals character = characterVisuals[characterIndex];

            SelectedPlayerPrefabs.Add(character.characterPrefab);

            SelectedPlayerSprites.Add(character.personaje);

            SelectedPlayerDevices.Add(players[i].device);
        }
    }

    // ==========================================
    // PREPARAR PARTIDA
    // ==========================================

    public bool PrepareMatchConfiguration()
    {
        int activePlayers = 0;

        int confirmedPlayers = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active)
                continue;

            activePlayers++;

            if (players[i].confirmed)
                confirmedPlayers++;

            int characterIndex = players[i].characterIndex;

            if (characterVisuals == null ||
                characterIndex < 0 ||
                characterIndex >= characterVisuals.Length)
            {
                return false;
            }

            CharacterVisuals character = characterVisuals[characterIndex];

            if (character.characterPrefab == null ||
                character.personaje == null)
            {
                return false;
            }
        }

        if (activePlayers < 2 ||
            activePlayers > 4 ||
            confirmedPlayers != activePlayers)
        {
            return false;
        }

        return true;
    }

    // ==========================================
    // INICIAR PARTIDA
    // ==========================================

    public void OnConfirmSelectionButton()
    {
        if (!PrepareMatchConfiguration())
            return;

        SceneManager.LoadScene("EscenaJuego");
    }

    // ==========================================
    // BOTÓN DE INICIAR
    // ==========================================

    private void UpdateStartButton()
    {
        if (btnIniciar == null)
            return;

        Button button = btnIniciar.GetComponent<Button>();

        if (button == null)
            return;

        int activePlayers = 0;

        int confirmedPlayers = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].active)
            {
                activePlayers++;

                if (players[i].confirmed)
                    confirmedPlayers++;
            }
        }

        button.interactable =
            activePlayers >= 2 &&
            confirmedPlayers == activePlayers;
    }
}
