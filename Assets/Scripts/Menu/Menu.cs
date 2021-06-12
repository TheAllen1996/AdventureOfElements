using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum LocStringKey{
    GameTitle,
    Start,
    Load,
    Language,
    Quit,
    English,
    Chinese,
    Back
}

[System.Serializable]
public class UIElement{
    public LocStringKey m_locStringKey;
    public GameObject m_gameObject;
}

public class Menu : MonoBehaviour
{
    [SerializeField] List<UIElement> m_uiElements = new List<UIElement>();

    List<MenuIndicator> m_menuIndicators = new List<MenuIndicator>();

    List<Button> m_menuBtn = new List<Button>();

    Language m_language;

    int m_selectionIndex;
    const float k_maxSelectionCooldown = 0.25f;
    float m_selectionCooldown;

    void Start(){
        m_language = LocalizationManager.m_instance.GetLanguage();
        for(int index = 0; index < m_uiElements.Count; ++index)
        {
            // Get assigned LocStringKey
            LocStringKey locStringKey = m_uiElements[index].m_locStringKey;
            // Set Localized Text to Text Field
            Text textObj = m_uiElements[index].m_gameObject.GetComponent<Text>();    // get text component
            if(textObj)
            {
                textObj.text = LocalizationManager.m_instance.GetLocalisedString(locStringKey.ToString());
            }

            // Store all Buttons for Selection
            Button buttonObj = m_uiElements[index].m_gameObject.GetComponent<Button>();
            if(buttonObj)
            {
                m_menuBtn.Add(buttonObj);
            }

            MenuIndicator indicator = m_uiElements[index].m_gameObject.GetComponent<MenuIndicator>();
            if(indicator)
            {
                m_menuIndicators.Add(indicator);
            }
        }
    }

    void Update() {
        UpdateSelection();
        CheckLanguage();
    }

    public void UpdateSelection(int mouseSelection=-1){
        if(mouseSelection != -1){
            m_menuIndicators[m_selectionIndex].SelectedState(false);
            m_selectionIndex = mouseSelection;
            m_menuIndicators[m_selectionIndex].SelectedState(true);
        }
        if(m_selectionCooldown < 0.0f){
            float verticalMovement = Input.GetAxis("Vertical");    // so it supports controller?
            bool selectionChanged = false;
            int oldSelectionIndex = m_selectionIndex;
            if(verticalMovement > 0.1f){
                --m_selectionIndex;
                selectionChanged = true;
            }
            else if (verticalMovement < -0.1f)
            {
                ++m_selectionIndex;
                selectionChanged = true;
            }

            if (selectionChanged)
            {
                m_selectionIndex = Mathf.Clamp(m_selectionIndex, 0, m_menuIndicators.Count - 1);
                m_menuBtn[m_selectionIndex].Select();
                m_menuIndicators[oldSelectionIndex].SelectedState(false);
                m_menuIndicators[m_selectionIndex].SelectedState(true);
                m_selectionCooldown = k_maxSelectionCooldown;

                //UIManager.m_Instance.PlayNextSound();
            }
        }
        else
        {
            m_selectionCooldown -= Time.deltaTime;
        }
    }

    public void OnStartClicked(){
        // UIManager.m_Instance.PlayConfirmSound();
        SceneManager.LoadScene("GameScene");
    }

    public void OnLoadClicked(){
        // Havent implemented yet.
        Debug.Log("Load nothing.");
    }

    public void OnOptionClicked(){
        // Havent implemented yet.
        Debug.Log("Settings.");
        UIManager.m_instance.SetMenuState(MenuPageType.Language);
    }

    public void OnBackToMainMenuClicked(){
        Debug.Log("BackToMainMenu.");
        UIManager.m_instance.SetMenuState(MenuPageType.MainMenu);
    }

    public void OnQuitClicked(){
        // play confirm sound
        Debug.Log("Quit.");
        Application.Quit();
    }

    void CheckLanguage(){
        Language tempLanguage = LocalizationManager.m_instance.GetLanguage();
        if(tempLanguage == m_language){
            return;
        }
        else{
            m_language = tempLanguage;
            UpdateText();
        }
    }

    public void OnSetEnglishClicked(){
        LocalizationManager.m_instance.SetupLocalization(Language.English);
        // UpdateText();
    }

    public void OnSetChineseClicked(){
        LocalizationManager.m_instance.SetupLocalization(Language.Chinese);
        // UpdateText();
    }

    void UpdateText(){
        for(int index = 0; index < m_uiElements.Count; ++index)
        {
            // Get assigned LocStringKey
            LocStringKey locStringKey = m_uiElements[index].m_locStringKey;
            // Set Localized Text to Text Field
            Text textObj = m_uiElements[index].m_gameObject.GetComponent<Text>();    // get text component
            if(textObj)
            {
                textObj.text = LocalizationManager.m_instance.GetLocalisedString(locStringKey.ToString());
            }
        }
    }
}