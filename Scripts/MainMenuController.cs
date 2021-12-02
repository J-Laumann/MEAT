using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    int money;
    public Text moneyText;
    public GameObject storeUI, mainUI, equipUI, equipListUI, salePrefab, howToUI;
    public Weapon[] weapons;
    public GameObject[] meats;

    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        //ALWAYS OWN THE GLOCK
        PlayerPrefs.SetInt("OwnWeapon0", 1);

        money = PlayerPrefs.GetInt("Money", 5000);
        PlayerPrefs.SetInt("Money", money);

        Object[] objs = Resources.LoadAll("Weapons");
        weapons = new Weapon[objs.Length];
        for (int x = 0; x < objs.Length; x++)
        {
            weapons[x] = ((GameObject)objs[x]).GetComponent<Weapon>();
        }

        objs = Resources.LoadAll("Food");
        meats = new GameObject[objs.Length];
        for (int x = 0; x < objs.Length; x++)
        {
            meats[x] = (GameObject)objs[x];
        }

        if(storeUI)
            SetStoreUI();
        
    }

    /// <summary>
    /// Loads the indicated scene
    /// </summary>
    /// <param name="levelName"></param>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void OpenStore()
    {
        storeUI.SetActive(true);
        mainUI.SetActive(false);
    }

    public void CloseStore()
    {
        storeUI.SetActive(false);
        mainUI.SetActive(true);
    }

    public void BuyWeapon(int i)
    {
        money -= weapons[i].weaponCost;
        moneyText.text = "$" + money;

        PlayerPrefs.SetInt("Money", money);

        PlayerPrefs.SetInt("OwnWeapon" + i, 1);

        foreach(Transform child in storeUI.GetComponentInChildren<GridLayoutGroup>().transform) {
            child.GetComponent<Button>().interactable = false;
        }
        

        StartCoroutine(PressKeypad(0f));
        StartCoroutine(PressKeypad(0.5f));
        StartCoroutine(PressKeypad(1f));
        StartCoroutine(DropItem(1.5f, i));
    }

    IEnumerator DropItem(float wait, int i)
    {
        yield return new WaitForSeconds(wait);
        Transform storeGrid = storeUI.GetComponentInChildren<GridLayoutGroup>().transform;
        GameObject obj = storeGrid.GetChild(i).Find("Icon").gameObject;
        obj.transform.SetParent(obj.transform.parent.parent.parent);
        Rigidbody2D iconRB = obj.AddComponent<Rigidbody2D>();
        iconRB.gravityScale = 80;
        yield return new WaitForSeconds(3f);
        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(100, 100);
        obj.transform.SetParent(obj.transform.parent.parent.Find("Depository"));
        obj.transform.localPosition = new Vector2(0, 200);
        Button button = obj.AddComponent<Button>();
        button.onClick.AddListener(delegate { Destroy(obj); });
        SetStoreUI();
    }

    public IEnumerator PressKeypad(float wait)
    {
        yield return new WaitForSeconds(wait);
        Transform trans = storeUI.transform.Find("Keypad");
        Transform button = trans.GetChild(Random.Range(0, trans.childCount));
        button.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(0.3f);
        button.GetComponent<Button>().interactable = true;
    }

    public void SetStoreUI()
    {
        if(moneyText)
            moneyText.text = "$" + money;

        Transform storeGrid = storeUI.GetComponentInChildren<GridLayoutGroup>().transform;
        foreach(Transform child in storeGrid)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            GameObject newSale = Instantiate(salePrefab, storeGrid);
            newSale.transform.Find("Icon").GetComponent<Image>().sprite = weapons[i].weaponSprite;
            newSale.transform.Find("Name").GetComponent<Text>().text = weapons[i].weaponName;
            newSale.transform.Find("Cost").GetComponent<Text>().text = "$" + weapons[i].weaponCost;
            if(PlayerPrefs.GetInt("OwnWeapon" + i, 0) == 0)
            {
                if (money >= weapons[i].weaponCost)
                {
                    int temp = i;
                    newSale.GetComponent<Button>().onClick.AddListener(delegate { BuyWeapon(temp); });
                }
                else
                {
                    newSale.GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                newSale.transform.Find("Icon").gameObject.SetActive(false);
                newSale.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void CloseEquipUI()
    {
        equipUI.SetActive(false);
        mainUI.SetActive(true);
        moneyText.gameObject.SetActive(true);
    }

    public void OpenEquipUI()
    {
        equipUI.SetActive(true);
        mainUI.SetActive(false);
        moneyText.gameObject.SetActive(false);

        Transform equipGrid = equipUI.GetComponentInChildren<GridLayoutGroup>().transform;
        foreach (Transform child in equipGrid)
            Destroy(child.gameObject);

        for(int i = 0; i < weapons.Length; i++)
        {
            if(PlayerPrefs.GetInt("OwnWeapon" + i, 0) == 1)
            {
                GameObject newEquip = Instantiate(salePrefab, equipGrid);
                newEquip.transform.Find("Icon").GetComponent<Image>().sprite = weapons[i].weaponSprite;
                newEquip.transform.Find("Name").GetComponent<Text>().text = weapons[i].weaponName;
                newEquip.transform.Find("Cost").GetComponent<Text>().text = "";
                int temp = i;
                newEquip.GetComponent<Button>().onClick.AddListener(delegate { OpenEquipList(temp); });
            }
        }

        for(int x = 0; x < 3; x++)
        {
            equipUI.transform.Find("Icon" + x).GetComponent<Image>().sprite = weapons[PlayerPrefs.GetInt("Weapon" + x, 0)].weaponSprite;
        }
    }

    public void OpenEquipList(int i)
    {
        equipListUI.SetActive(true);
        equipListUI.transform.Find("Icon").GetComponent<Image>().sprite = weapons[i].weaponSprite;
        Transform buttons = equipListUI.transform.Find("Buttons");
        for (int x = 0; x < buttons.childCount; x++){
            buttons.GetChild(x).Find("Icon").GetComponent<Image>().sprite = weapons[PlayerPrefs.GetInt("Weapon" + x, 0)].weaponSprite;
            Button button = buttons.GetChild(x).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            int tempItem = i;
            int tempSlot = x;
            button.onClick.AddListener(delegate { EquipItem(tempSlot, tempItem); });
        }
    }

    public void CloseEquipList()
    {
        equipListUI.SetActive(false);
    }

    public void EquipItem(int slot, int item)
    {
        PlayerPrefs.SetInt("Weapon" + slot, item);
        equipUI.transform.Find("Icon" + slot).GetComponent<Image>().sprite = weapons[item].weaponSprite;
        CloseEquipList();
    }

    public void OpenHowTo()
    {
        mainUI.SetActive(false);
        moneyText.gameObject.SetActive(false);
        howToUI.SetActive(true);
    }

    public void CloseHowTo()
    {
        mainUI.SetActive(true);
        moneyText.gameObject.SetActive(true);
        howToUI.SetActive(false);
    }
}
