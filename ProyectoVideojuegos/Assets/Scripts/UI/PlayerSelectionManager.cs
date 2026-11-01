using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerSelectionManager : MonoBehaviour
{
    private class PlayerSlot
    {
        public bool active;
        public bool confirmed;
        public InputDevice device;
        public int characterIndex;
        public int lastHorizontalDirection;
        public int joinedFrame;
    }

    [Header("Paneles de los jugadores")]
    public GameObject[] playerPanels = new GameObject[4];
    public Image[] playerImages = new Image[4];

    [Header("Personajes disponibles")]
    public Sprite[] characterSprites;
    public GameObject[] characterPrefabs;

    private PlayerSlot[] players = new PlayerSlot[4];

    public static List<GameObject> SelectedPlayerPrefabs = new List<GameObject>();
    public static List<Sprite> SelectedPlayerSprites = new List<Sprite>();
    public static List<InputDevice> SelectedPlayerDevices = new List<InputDevice>();

    private InputSystem_Actions controls;

    public GameObject btnIniciar;

    public int ActivePlayerCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].active)
                {
                    count++;
                }
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
                {
                    count++;
                }
            }
            return count;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerSlot();
            players[i].characterIndex = -1;

            if (playerPanels != null && i < playerPanels.Length && playerPanels[i] != null)
            {
                playerPanels[i].SetActive(false);
            }

            if (playerImages != null && i < playerImages.Length && playerImages[i] != null)
            {
                playerImages[i].sprite = null;
            }
        }

        SelectedPlayerPrefabs.Clear();
        SelectedPlayerSprites.Clear();
        SelectedPlayerDevices.Clear();

        UpdateStartButton();
    }

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
        {
            return;
        }

        controls.Menu.Join.performed -= OnJoinPerformed;
        controls.Menu.Navigate.performed -= OnNavigatePerformed;
        controls.Menu.Navigate.canceled -= OnNavigateCanceled;
        controls.Menu.Confirm.performed -= OnConfirmPerformed;
        controls.Menu.Cancel.performed -= OnCancelPerformed;

        controls.Menu.Disable();
    }

    private void OnJoinPerformed(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        if (!(device is Gamepad) && !(device is Keyboard))
        {
            return;
        }

        if (FindPlayerByDevice(device) >= 0)
        {
            return;
        }

        int slot = FindFreePlayerSlot();

        if (slot < 0)
        {
            return;
        }

        players[slot].active = true;
        players[slot].device = device;
        players[slot].joinedFrame = Time.frameCount;
        players[slot].characterIndex = FindFirstAvailableCharacter();

        playerPanels[slot].SetActive(true);

        RefreshPlayerImage(slot);
        RefreshStoredSelections();
        UpdateStartButton();
    }

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0 || players[playerIndex].confirmed)
        {
            return;
        }

        Vector2 movement = context.ReadValue<Vector2>();
        int direction = 0;

        if (movement.x < -0.5f)
        {
            direction = -1;
        }
        else if (movement.x > 0.5f)
        {
            direction = 1;
        }

        if (direction == 0)
        {
            players[playerIndex].lastHorizontalDirection = 0;
            return;
        }

        if (direction == players[playerIndex].lastHorizontalDirection)
        {
            return;
        }

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

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0)
        {
            return;
        }

        if (players[playerIndex].joinedFrame == Time.frameCount)
        {
            return;
        }

        ConfirmCharacter(playerIndex);
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        int playerIndex = FindPlayerByDevice(context.control.device);

        if (playerIndex < 0)
        {
            return;
        }

        if (players[playerIndex].confirmed)
        {
            players[playerIndex].confirmed = false;
            RefreshStoredSelections();
            UpdateStartButton();
            return;
        }

        playerPanels[playerIndex].SetActive(false);
        playerImages[playerIndex].sprite = null;

        players[playerIndex] = new PlayerSlot();
        players[playerIndex].characterIndex = -1;

        RefreshStoredSelections();
        UpdateStartButton();
    }

    private int FindPlayerByDevice(InputDevice device)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].active && players[i].device == device)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindFreePlayerSlot()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active)
            {
                return i;
            }
        }
        return -1;
    }

    private void NavigateCharacter(int playerIndex, int direction)
    {
        if (players[playerIndex].confirmed)
        {
            return;
        }

        int count = characterSprites.Length;
        int currentIndex = players[playerIndex].characterIndex;

        if (currentIndex < 0)
        {
            currentIndex = FindFirstAvailableCharacter();

            if (currentIndex < 0)
            {
                return;
            }
        }

        for (int i = 0; i < count; i++)
        {
            currentIndex += direction;

            if (currentIndex < 0)
            {
                currentIndex = count - 1;
            }
            else if (currentIndex >= count)
            {
                currentIndex = 0;
            }

            if (IsCharacterAvailable(currentIndex, playerIndex))
            {
                players[playerIndex].characterIndex = currentIndex;
                RefreshPlayerImage(playerIndex);
                return;
            }
        }
    }

    private int FindFirstAvailableCharacter()
    {
        for (int i = 0; i < characterSprites.Length; i++)
        {
            if (IsCharacterAvailable(i, -1))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsCharacterAvailable(int characterIndex, int requestingPlayer)
    {
        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
        {
            return false;
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (i != requestingPlayer && players[i].active && players[i].confirmed &&
                players[i].characterIndex == characterIndex)
            {
                return false;
            }
        }
        return true;
    }

    private void RefreshPlayerImage(int playerIndex)
    {
        int characterIndex = players[playerIndex].characterIndex;

        if (characterIndex < 0 || characterIndex >= characterSprites.Length)
        {
            playerImages[playerIndex].sprite = null;
            return;
        }
        playerImages[playerIndex].sprite = characterSprites[characterIndex];
    }

    private void ConfirmCharacter(int playerIndex)
    {
        if (players[playerIndex].confirmed)
        {
            return;
        }

        int characterIndex = players[playerIndex].characterIndex;

        if (!IsCharacterAvailable(characterIndex, playerIndex))
        {
            players[playerIndex].characterIndex = FindFirstAvailableCharacter();
            RefreshPlayerImage(playerIndex);
            return;
        }

        if (characterIndex < 0)
        {
            return;
        }

        players[playerIndex].confirmed = true;

        RefreshStoredSelections();
        RefreshOtherPlayerSelections();
        UpdateStartButton();
    }

    private void RefreshOtherPlayerSelections()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active || players[i].confirmed)
            {
                continue;
            }

            int currentIndex = players[i].characterIndex;

            if (!IsCharacterAvailable(currentIndex, i))
            {
                players[i].characterIndex = FindFirstAvailableCharacter();
                RefreshPlayerImage(i);
            }
        }
    }

    private void RefreshStoredSelections()
    {
        SelectedPlayerPrefabs.Clear();
        SelectedPlayerSprites.Clear();
        SelectedPlayerDevices.Clear();

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].active && players[i].confirmed)
            {
                int characterIndex = players[i].characterIndex;

                if (characterIndex >= 0 && characterIndex < characterPrefabs.Length)
                {
                    SelectedPlayerPrefabs.Add(characterPrefabs[characterIndex]);
                    SelectedPlayerSprites.Add(characterSprites[characterIndex]);
                    SelectedPlayerDevices.Add(players[i].device);
                }
            }
        }
    }

    public void OnConfirmSelectionButton()
    {
        bool ready = PrepareMatchConfiguration();

        if (!ready)
        {
            return;
        }

        SceneManager.LoadScene("EscenaJuego");
    }

    public bool PrepareMatchConfiguration()
    {
        int activePlayers = 0;
        int confirmedPlayers = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].active)
            {
                continue;
            }

            activePlayers++;

            if (players[i].confirmed)
            {
                confirmedPlayers++;
            }

            int characterIndex = players[i].characterIndex;

            if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
            {
                return false;
            }
        }

        if (activePlayers < 2 || activePlayers > 4 || confirmedPlayers != activePlayers)
        {
            return false;
        }

        return true;
    }

    private void UpdateStartButton()
    {
        if (btnIniciar == null)
        {
            return;
        }

        Button button = btnIniciar.GetComponent<Button>();

        if (button == null)
        {
            return;
        }

        int activePlayers = 0;
        int confirmedPlayers = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].active)
            {
                activePlayers++;

                if (players[i].confirmed)
                {
                    confirmedPlayers++;
                }
            }
        }

        // El botón solo se activa si hay 2 o más jugadores y el 100% de ellos ha confirmado
        button.interactable = (activePlayers >= 2) && (confirmedPlayers == activePlayers);
    }
}