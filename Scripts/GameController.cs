using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{

    /// <summary>
    /// THE GAMEMODE| 0 = Recipes | 1 = Meat Grinder
    /// </summary>
    public int GAMEMODE = 0;
    public Transform meatParent;
    public float meatTimer, meatTimerMin, meatSpeedup;

    public AudioClip sizzle, chaChing, ripSound, failSound, completeSound;


    public GameObject[] meats;
    public Recipe[] recipes;
    public Weapon[] weapons;

    public Recipe activeRecipe;
    public GameObject crossoutPrefab;
    public List<GameObject> currentIngreds;

    public int money, moneyGainedThisGame;
    public Text moneyText, gainedMoneyText, thisGameMoneyText;

    public GameObject recipeUI, recipePrefab, pauseUI;
    public GameObject[] weaponButtons;

    public LayerMask hitMask;

    public GameObject rottingPrefab, poofPrefab, cauldron;
    public bool debuggin, paused, showingRecipe;

    int equippedWeaponIndex;
    public Weapon[] myWeapons;
    private float[] weaponTimers;
    public Weapon equippedWeapon;
    public GameObject weaponEffect;
    public Vector2 weaponEffectLastPos;

    public GameObject cursorCircle;
    public Animator UIAnim;

    // Start is called before the first frame update
    void Start()
    {
        //Load money
        UpdateMoneyText();

        //Load all Resources (Food, Recipes, Weapons)
        Object[] objs = Resources.LoadAll("Food");
        meats = new GameObject[objs.Length];
        for(int x = 0; x < objs.Length; x++)
        {
            meats[x] = (GameObject)objs[x];
        }

        objs = Resources.LoadAll("Recipes");
        recipes = new Recipe[objs.Length];
        for(int x = 0; x < objs.Length; x++)
        {
            recipes[x] = ((GameObject)objs[x]).GetComponent<Recipe>();
        }

        objs = Resources.LoadAll("Weapons");
        weapons = new Weapon[objs.Length];
        for(int x = 0; x < objs.Length; x++)
        {
            weapons[x] = ((GameObject)objs[x]).GetComponent<Weapon>();
        }


        //Setting up Weapons (Equipped and UI)
        myWeapons = new Weapon[3];
        weaponTimers = new float[3];

        for(int i = 0; i < weaponButtons.Length; i++) {
            myWeapons[i] = weapons[PlayerPrefs.GetInt("Weapon" + i, 0)];
            weaponTimers[i] = 0;
            weaponButtons[i].transform.Find("Icon").GetComponent<Image>().sprite = myWeapons[i].weaponSprite;
            int temp = i;
            weaponButtons[i].GetComponentInChildren<Button>().onClick.AddListener(delegate { EquipWeapon(temp); });
        }
        
        EquipWeapon(0);

        //Start Game by repeating SpawnRandomMeat and give us a recipe
        
        Invoke("SpawnRandomMeat", 0);

        if(GAMEMODE == 0)
            NewRecipe();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (showingRecipe)
                ShrinkRecipe();
            else
                TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            EquipWeapon(2);

        if (Input.GetKeyDown(KeyCode.Q) || Input.mouseScrollDelta.y < 0)
        {
            CycleWeaponLeft();
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.mouseScrollDelta.y > 0)
        {
            CycleWeaponRight();
        }

        //DONT DO ANYTHING ELSE IF PAUSED (NO SHOOTING)
        if (paused || showingRecipe)
            return;

        //Count down all timers
        for(int i = 0; i < weaponTimers.Length; i++)
        {
            if (weaponTimers[i] >= 0)
            {
                weaponTimers[i] -= Time.deltaTime;
                Image cooldown = weaponButtons[i].transform.Find("Cooldown").GetComponent<Image>();
                float newY = (float)weaponTimers[i] / (float)myWeapons[i].reloadTime;
                cooldown.fillAmount = newY;
            }
        }

        //Crosshair
        cursorCircle.transform.position = Input.mousePosition;
        float fill;
        if (equippedWeapon.reloadTime == -1)
            fill = 1;
        else
            fill = ((float)equippedWeapon.reloadTime - (float)weaponTimers[equippedWeaponIndex]) / (float)equippedWeapon.reloadTime;
        cursorCircle.GetComponent<Image>().fillAmount = fill;


        //cant shoot if over a UI element
        if (IsPointerOverUIObject())
            return;

        //USING WEAPONS
        if (Input.GetMouseButtonDown(0))
        {
            //SHOOTING (weaponType 0 = Basic Gun, 2 = "box" collider)
            if (equippedWeapon.weaponType == 0 || equippedWeapon.weaponType == 2)
            {
                if (weaponTimers[equippedWeaponIndex] <= 0 || equippedWeapon.reloadTime == -1)
                {
                    Shooting(true);
                }
            }

            //BASIC MELEE
            if (equippedWeapon.weaponType == 1)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                weaponEffect = Instantiate(equippedWeapon.weaponEffect, mousePos, equippedWeapon.weaponEffect.transform.rotation);
            }
        }

        else if (Input.GetMouseButton(0))
        {
            if (equippedWeapon.fullAuto)
            {
                //SHOOTING BASIC GUN (if weaponType is 0)
                if (equippedWeapon.weaponType == 0)
                {
                    if (weaponTimers[equippedWeaponIndex] <= 0 || equippedWeapon.reloadTime == -1)
                    {
                        Shooting(false);
                    }
                }
            }

            if (weaponEffect)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                weaponEffectLastPos = weaponEffect.transform.position;
                weaponEffect.transform.position = mousePos;
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            //SPECIAL MOUSE UP SHOOT
            //  (DOUBLE BARREL)
            if (myWeapons[equippedWeaponIndex].gameObject.name == "001")
            {
                if (weaponTimers[equippedWeaponIndex] <= 0 || equippedWeapon.reloadTime == -1)
                {
                    Shooting(false);
                }
            }

            if (weaponEffect)
            {
                Destroy(weaponEffect);
                weaponEffect = null;
            }
        }

        
    }

    void Shooting(bool mouseDown)
    {
        //Check within "hit range" for meats
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Random.insideUnitCircle * equippedWeapon.spread);
        Collider2D[] cols;
        if (equippedWeapon.weaponType == 2)
            cols = Physics2D.OverlapBoxAll(mousePos, new Vector2(equippedWeapon.range, equippedWeapon.rangeY), 0f, hitMask);
        else
            cols = Physics2D.OverlapCircleAll(mousePos, equippedWeapon.range, hitMask);

        //DEBUGGING
        if (debuggin)
        {
            if (equippedWeapon.weaponType == 2)
                Debug.DrawLine(new Vector2(mousePos.x, mousePos.y + equippedWeapon.rangeY), new Vector2(mousePos.x, mousePos.y - equippedWeapon.rangeY), Color.red, 1);
            else
                Debug.DrawLine(new Vector2(mousePos.x, mousePos.y + equippedWeapon.range), new Vector2(mousePos.x, mousePos.y - equippedWeapon.range), Color.red, 1);
            Debug.DrawLine(new Vector2(mousePos.x - equippedWeapon.range, mousePos.y), new Vector2(mousePos.x + equippedWeapon.range, mousePos.y), Color.red, 1);
        }

        bool hit = false;
        foreach (Collider2D col in cols)
        {
            if (col.gameObject.tag == "Meat")
            {
                FlingMeat meat = col.GetComponent<FlingMeat>();
                if (meat)
                {
                    //SPAWN THE MEAT-SEEKER
                    if(myWeapons[equippedWeaponIndex].gameObject.name == "010")
                    {
                        GameObject obj = Instantiate(equippedWeapon.weaponEffect, mousePos, Quaternion.identity);
                        MeatSeeker seeker;
                        seeker = obj.GetComponent<MeatSeeker>();
                        seeker.targetID = col.gameObject.name.Substring(0, 3);
                        seeker.targetSprite = col.gameObject.GetComponent<SpriteRenderer>().sprite;
                        hit = true;
                        break;
                    }

                    //PINNING MEATS
                    if(myWeapons[equippedWeaponIndex].gameObject.name == "004" || myWeapons[equippedWeaponIndex].gameObject.name == "005")
                    {
                        Vector2 pinPoint = col.transform.InverseTransformPoint(mousePos) * col.transform.localScale.x;
                        StartCoroutine(PinObject(equippedWeapon.weaponEffect.GetComponent<FallAfterTime>().fallTimer, col.gameObject, pinPoint));
                    }

                    hit = true;
                    meat.HitMeat(mousePos, equippedWeapon.damage);
                }
            }
        }

        //REFUND THINGS THAT DIDNT HIT AND DESERVE REFUNDS (MEAT-SEEKER)
        if (!hit)
        {
            if (equippedWeapon.refundMiss)
            {
                return;
            }
        }

        //Effects
        GameObject newEffect = Instantiate(equippedWeapon.weaponEffect, mousePos, equippedWeapon.weaponEffect.transform.rotation);
        AudioSource.PlayClipAtPoint(equippedWeapon.weaponSound, mousePos);

        //SPECIAL (DOUBLE BARREL)
        if(mouseDown && myWeapons[equippedWeaponIndex].gameObject.name == "001")
        {
            return;
        }

        weaponTimers[equippedWeaponIndex] = equippedWeapon.reloadTime;
    }

    void SpawnRandomMeat()
    {
        int spawnID = Random.Range(0, meats.Length);

        //Adds a 25% chance the meat is from you recipe (makes game feel better)
        if (activeRecipe)
        {
            if (Random.Range(0, 4) == 0)
            {
                spawnID = activeRecipe.ingreds[Random.Range(0, activeRecipe.ingreds.Length)];
            }
        }

        //ROTTEN CHANCE
        bool rotten = false;
        if(Random.Range(0,10) == 0)
        {
            rotten = true;
        }

        //"Flip coin" for Left or Right side spawn
        Vector2 spawnPoint;
        float startForce;
        if(Random.Range(0,2) == 0)
        {
            spawnPoint = new Vector2(-12, -4);
            startForce = Random.Range(0.4f, 1f);
        }
        else
        {
            spawnPoint = new Vector2(12, -4);
            startForce = Random.Range(-1f, -0.4f);
        }

        GameObject newMeat = Instantiate(meats[spawnID], spawnPoint, Quaternion.identity, meatParent);
        FlingMeat newMeatController = newMeat.GetComponent<FlingMeat>();
        newMeatController.startForce.x *= startForce;
        newMeatController.rotten = rotten;

        if (meatTimer > meatTimerMin)
            meatTimer -= meatSpeedup;
        Invoke("SpawnRandomMeat", meatTimer);
    }

    public void AddIngredient(GameObject obj)
    {
        currentIngreds.Add(obj);

        //cross out UI
        foreach(Transform child in recipeUI.transform.GetChild(0).Find("Ingreds"))
        {
            if(child.childCount == 0)
            {
                if(child.GetComponent<Image>().sprite == obj.GetComponent<SpriteRenderer>().sprite)
                {
                    GameObject crossoutObj = Instantiate(crossoutPrefab, child);
                    break;
                }
            }
        }

        //Check for glitched objs
        foreach(GameObject ingred in currentIngreds)
        {
            if(ingred == null)
            {
                currentIngreds.Remove(null);    
            }
        }

        //Finish the recipe
        if(currentIngreds.Count >= activeRecipe.ingreds.Length || obj.GetComponent<FlingMeat>().rotten)
        {
            List<int> currentIngredsIDs = new List<int>();
            for(int i = 0; i < currentIngreds.Count; i++)
            {
                int index = -1;
                for(int x = 0; x < meats.Length; x++)
                {
                    if (meats[x] != null)
                    {
                        if (meats[x].name == currentIngreds[i].name.Substring(0, 3))
                        {
                            index = x;
                            break;
                        }
                    }
                }
                currentIngredsIDs.Add(index);
            }

            float rating = 0;
            for (int i = 0; i < activeRecipe.ingreds.Length; i++)
            {
                if (currentIngredsIDs.Contains(activeRecipe.ingreds[i]))
                {
                    rating++;
                    currentIngredsIDs.Remove(activeRecipe.ingreds[i]);
                }
            }
            rating = rating / activeRecipe.ingreds.Length;

            //Check for rotten
            foreach(GameObject meat in currentIngreds)
            {
                if (meat.GetComponent<FlingMeat>().rotten)
                {
                    rating = 0;
                }
            }

            if (rating >= 0.95f)
            {
                AudioSource.PlayClipAtPoint(chaChing, Camera.main.transform.position, 0.5f);
            }
            else if (rating < 0.5f)
            {
                AudioSource.PlayClipAtPoint(failSound, Camera.main.transform.position, 0.5f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(completeSound, Camera.main.transform.position, 0.5f);
            }
            int gain = (int)(activeRecipe.recipeWorth * (float)rating);
            gainedMoneyText.text = "+ $" + gain;
            moneyGainedThisGame += gain;
            money += gain;
            PlayerPrefs.SetInt("Money", money);
            UpdateMoneyText();
            Instantiate(poofPrefab, cauldron.transform);
            NewRecipe();
        }
    }

    void NewRecipe()
    {
        //Reset recipe line/cauldron
        foreach(GameObject obj in currentIngreds)
        {
            Destroy(obj);
        }
        if (activeRecipe != null)
        {
            AudioSource.PlayClipAtPoint(ripSound, Camera.main.transform.position);
            Rigidbody2D recipeRB = recipeUI.transform.GetChild(0).gameObject.AddComponent<Rigidbody2D>();
            recipeUI.transform.GetChild(0).SetParent(recipeUI.transform.parent);
            recipeRB.gravityScale = 300;
            recipeRB.AddTorque(Random.Range(-5, 5));
            Destroy(recipeRB.gameObject, 3f);
        }

        int maxIngreds = 3;
        int minIngreds = 2;

        if (meatTimer <= 1.3f)
            maxIngreds = 4;
        if (meatTimer <= 1f)
        {
            maxIngreds = 5;
            minIngreds = 3;
        }
        if(meatTimer <= meatTimerMin)
        {
            maxIngreds = 6;
            minIngreds = 4;
        }

        Recipe r = recipes[Random.Range(0, recipes.Length)];
        while (r.ingreds.Length < minIngreds || r.ingreds.Length > maxIngreds)
        {
            r = recipes[Random.Range(0, recipes.Length)];
        }

        activeRecipe = r;
        currentIngreds = new List<GameObject>();

        GameObject newRecipe = Instantiate(recipePrefab, recipeUI.transform);
        newRecipe.transform.GetChild(1).GetComponent<Text>().text = activeRecipe.recipeName;
        newRecipe.transform.GetChild(2).GetComponent<Text>().text = "$" + activeRecipe.recipeWorth;
        foreach(int x in activeRecipe.ingreds)
        {
            GameObject newObj = new GameObject();
            Image newImg = newObj.AddComponent<Image>();
            newImg.sprite = meats[x].GetComponent<SpriteRenderer>().sprite;
            newObj.GetComponent<RectTransform>().SetParent(newRecipe.transform.GetChild(3));
            newObj.GetComponent<RectTransform>().localScale = Vector3.one;
            newObj.SetActive(true);
        }

        UIAnim.Play("RecipeIntro");
        Time.timeScale = 0;
        showingRecipe = true;
    }

    public void TogglePause()
    {
        if(paused)
        {
            pauseUI.SetActive(false);
            paused = false;
            Time.timeScale = 1;
        }
        else
        {
            pauseUI.SetActive(true);
            paused = true;
            Time.timeScale = 0;
        }
    }

    public void EquipWeapon(int index)
    {
        Debug.Log("Equippin Weapon " + index);
        if (weaponEffect)
        {
            Destroy(weaponEffect);
            weaponEffect = null;
        }

        if (equippedWeapon)
        {
            weaponButtons[equippedWeaponIndex].transform.Find("Back").GetComponent<Image>().color = Color.white;
        }

        equippedWeaponIndex = index;
        equippedWeapon = myWeapons[index];
        weaponButtons[equippedWeaponIndex].transform.Find("Back").GetComponent<Image>().color = (Color.red + Color.gray) / 2;

        cursorCircle.transform.localScale = new Vector2(equippedWeapon.range, equippedWeapon.rangeY);
    }

    public void CycleWeaponRight()
    {
        int temp = equippedWeaponIndex + 1;
        if(temp >= myWeapons.Length)
        {
            temp = 0;
        }
        EquipWeapon(temp);
    }
    public void CycleWeaponLeft()
    {
        int temp = equippedWeaponIndex - 1;
        if (temp < 0)
        {
            temp = myWeapons.Length - 1;
        }
        EquipWeapon(temp);
    }

    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach(RaycastResult result in results)
        {
            if(result.gameObject.layer == 5)
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator Resume(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        Time.timeScale = 1;
    }

    public void ShrinkRecipe()
    {
        UIAnim.Play("RecipeOutro");
        showingRecipe = false;
        gainedMoneyText.text = "";
        StartCoroutine(Resume(1f));
    }

   
    public IEnumerator PinObject(float time, GameObject pinned, Vector2 pinPoint)
    {
        Rigidbody2D rb2d = pinned.GetComponent<Rigidbody2D>();
        rb2d.constraints = RigidbodyConstraints2D.FreezePosition;
        rb2d.centerOfMass = pinPoint;
        rb2d.angularDrag = 5f;
        pinned.layer = 9;
        yield return new WaitForSeconds(time);
        if (rb2d)
        {
            rb2d.constraints = RigidbodyConstraints2D.None;
            rb2d.angularDrag = 0.05f;
            rb2d.centerOfMass = Vector2.zero;
            rb2d.AddForce(Vector2.one * 0.1f);
        }
        if(pinned)
            pinned.layer = 7;
    }

    public void UpdateMoneyText()
    {
        money = PlayerPrefs.GetInt("Money", 0);
        moneyText.text = "$" + money;
        thisGameMoneyText.text = "$" + moneyGainedThisGame;
    }
}
