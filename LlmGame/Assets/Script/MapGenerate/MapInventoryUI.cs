using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapInventoryUI : MonoBehaviour
{
    public Player player;

    private Canvas canvas;
    private GameObject panel;
    private Button inventoryButton;
    private Transform weaponContainer;
    private Transform armorContainer;
    private Button passiveButton;
    private GameObject passivePanel;
    private Text statsText;
    private WeaponSlotUI weaponSlot;
    private Dictionary<BodyPartType, ArmorSlotUI> armorSlots = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "MapGenerate" || scene.name == "MapGenerate02")
        {
            GameObject go = new GameObject("MapInventoryUI");
            go.AddComponent<MapInventoryUI>();
        }
    }

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        BuildUI();
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(TogglePanel);
        panel.SetActive(false);
    }

    private void BuildUI()
    {
        canvas = new GameObject("MapInventoryCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        inventoryButton = CreateButton(canvas.transform, "InventoryButton", "Inventory", new Vector2(120, 40), new Vector2(-70, -40), new Vector2(1, 1));

        panel = new GameObject("InventoryPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(420, 520);
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);

        statsText = CreateText(panel.transform, "StatsText", 14);

        Image wsImage = CreateImage(panel.transform, "WeaponSlot", new Vector2(60, 60));
        weaponSlot = wsImage.gameObject.AddComponent<WeaponSlotUI>();
        weaponSlot.slotImage = wsImage;

        GameObject armorSlotsParent = new GameObject("ArmorSlots", typeof(RectTransform));
        armorSlotsParent.transform.SetParent(panel.transform);
        HorizontalLayoutGroup armorLayout = armorSlotsParent.AddComponent<HorizontalLayoutGroup>();
        armorLayout.childControlHeight = false;
        armorLayout.childControlWidth = false;
        armorLayout.spacing = 5f;
        RectTransform asrt = armorSlotsParent.GetComponent<RectTransform>();
        asrt.sizeDelta = new Vector2(0, 70);
        foreach (BodyPartType type in System.Enum.GetValues(typeof(BodyPartType)))
        {
            Image slotImg = CreateImage(armorSlotsParent.transform, type.ToString() + "Slot", new Vector2(50, 50));
            ArmorSlotUI slot = slotImg.gameObject.AddComponent<ArmorSlotUI>();
            slot.slotType = type;
            slot.slotImage = slotImg;
            armorSlots[type] = slot;
        }

        GameObject weaponInv = new GameObject("WeaponInventory", typeof(RectTransform));
        weaponInv.transform.SetParent(panel.transform);
        weaponContainer = weaponInv.transform;
        HorizontalLayoutGroup weaponLayout = weaponInv.AddComponent<HorizontalLayoutGroup>();
        weaponLayout.childControlHeight = false;
        weaponLayout.childControlWidth = false;
        weaponLayout.childForceExpandWidth = false;
        weaponLayout.childForceExpandHeight = false;
        weaponLayout.spacing = 5f;

        GameObject armorInv = new GameObject("ArmorInventory", typeof(RectTransform));
        armorInv.transform.SetParent(panel.transform);
        armorContainer = armorInv.transform;
        HorizontalLayoutGroup armorInvLayout = armorInv.AddComponent<HorizontalLayoutGroup>();
        armorInvLayout.childControlHeight = false;
        armorInvLayout.childControlWidth = false;
        armorInvLayout.childForceExpandHeight = false;
        armorInvLayout.childForceExpandWidth = false;
        armorInvLayout.spacing = 5f;

        passiveButton = CreateButton(panel.transform, "PassiveButton", "Passive Items", new Vector2(160, 30), Vector2.zero, new Vector2(0.5f, 0.5f));
        passiveButton.onClick.AddListener(TogglePassivePanel);

        passivePanel = new GameObject("PassivePanel", typeof(RectTransform), typeof(Image));
        passivePanel.transform.SetParent(canvas.transform);
        RectTransform pprt = passivePanel.GetComponent<RectTransform>();
        pprt.anchorMin = new Vector2(0.5f, 0.5f);
        pprt.anchorMax = new Vector2(0.5f, 0.5f);
        pprt.pivot = new Vector2(0.5f, 0.5f);
        pprt.sizeDelta = new Vector2(300, 300);
        passivePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        Text passiveText = CreateText(passivePanel.transform, "PassiveText", 14);
        passiveText.text = "Passive inventory";
        passivePanel.SetActive(false);
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchored, Vector2 anchor)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        Image img = obj.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.2f);
        Button btn = obj.GetComponent<Button>();
        Text txt = CreateText(obj.transform, "Text", 14);
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        return btn;
    }

    private Text CreateText(Transform parent, string name, int fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero;
        Text txt = obj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = fontSize;
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperLeft;
        return txt;
    }

    private Image CreateImage(Transform parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        return obj.GetComponent<Image>();
    }

    public void TogglePanel()
    {
        bool active = !panel.activeSelf;
        panel.SetActive(active);
        if (active)
            RefreshUI();
    }

    private void TogglePassivePanel()
    {
        passivePanel.SetActive(!passivePanel.activeSelf);
    }

    public void RefreshUI()
    {
        if (player == null) return;

        statsText.text = $"ATK: {player.attack + player.bonusAttack}\n" +
                         $"DEF: {player.defense + player.bonusDefense}\n" +
                         $"FOC: {player.focus + player.bonusFocus}\n" +
                         $"HP: {player.currentHP}/{player.maxHP + player.bonusMaxHP}\n" +
                         $"MP: {player.currentMP}/{player.maxMP + player.bonusMaxMP}";

        weaponSlot?.SetWeapon(player.equippedWeapon);

        foreach (var kvp in armorSlots)
        {
            ArmorData a;
            if (player.equippedArmorByPart != null && player.equippedArmorByPart.TryGetValue(kvp.Key, out a) && a != null && a.icon != null)
                kvp.Value.slotImage.sprite = a.icon;
            else
                kvp.Value.slotImage.sprite = null;
        }

        foreach (Transform t in weaponContainer)
            Destroy(t.gameObject);
        foreach (Transform t in armorContainer)
            Destroy(t.gameObject);

        foreach (var item in new List<Item>(player.inventoryItems))
        {
            if (item is Weapon w)
                CreateWeaponEntry(w);
        }
        foreach (var armor in new List<ArmorData>(player.inventoryArmors))
            CreateArmorEntry(armor);
    }

    private void CreateWeaponEntry(Weapon weapon)
    {
        Image img = CreateImage(weaponContainer, weapon.itemName ?? weapon.name, new Vector2(50, 50));
        if (weapon.icon != null) img.sprite = weapon.icon;
        img.gameObject.AddComponent<CanvasGroup>();
        WeaponDragHandler drag = img.gameObject.AddComponent<WeaponDragHandler>();
        drag.weapon = weapon;
    }

    private void CreateArmorEntry(ArmorData armor)
    {
        Image img = CreateImage(armorContainer, armor.itemName ?? armor.name, new Vector2(50, 50));
        if (armor.icon != null) img.sprite = armor.icon;
        img.gameObject.AddComponent<CanvasGroup>();
        InventoryItemDragHandler drag = img.gameObject.AddComponent<InventoryItemDragHandler>();
        drag.armorData = armor;
    }
}
