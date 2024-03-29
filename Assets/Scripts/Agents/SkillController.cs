using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [SerializeField] Controller m_controller;
    [SerializeField] PlayerLogic m_player;
    [SerializeField] SkillSlots m_playerSkillSlots;
    [SerializeField] SkillSelection m_skillSelection;

    [SerializeField] List<Skill> m_skillList;    // all the skills with considering skill levels
    List<int> m_learnedIDQueue = new List<int>();
    int m_queueIndex = 0;
    // int m_blessingLevel = 0;
    // int m_furnaceLevel = 0;

    [SerializeField] int m_numSkill;    // total number of skills without considering skill levels
    List<int> m_learnedSkillLV = new List<int>();    // m_learnedSkillLV.Count = m_numSkill

    int m_randomSeed;
    List<int> m_generateList;
    List<bool> m_exclusiveList;

    void Start() {
        for(int i = 0; i < m_skillList.Count; i++){
            m_learnedIDQueue.Add(-1);
        }

        for(int i = 0; i < m_numSkill; i++){
            m_learnedSkillLV.Add(0);
        }

        if(LocalizationManager.m_instance.loadChecker){
            for(int i = 0; i < m_skillList.Count; i++){
                int temp = PlayerPrefs.GetInt("LearnedSkillQueue" + i);
                m_learnedIDQueue[i] = temp;
                if(temp == -1){
                    break;
                }
                if(temp == 2){ // 2, 9
                    // blessing
                    int lv = PlayerPrefs.GetInt("BlessingLevel");
                    for(int j = 0; j < lv; j++){
                        SkillUpdate(temp, true);
                    }
                }
                else if(temp == 9){
                    // furnace
                    int lv = PlayerPrefs.GetInt("FurnaceLevel");
                    for(int j = 0; j < lv; j++){
                        SkillUpdate(temp, true);
                    }
                }
                else{
                    SkillUpdate(temp, true);
                }
            }
            m_player.LoadData();
        }
    }

    void UpdateGenerateList(int level){
        // Get a pool of all the skills of which the requirements are satisfied
        // Debug.Log("Lv. " + level);
        m_generateList = new List<int>();
        m_exclusiveList = new List<bool>();
        for(int s = 0; s < m_skillList.Count; s++){
            m_exclusiveList.Add(false);
        }
        // Debug.Log("Start checking " + m_skillList.Count + " skills requirement.");
        for(int s = 0; s < m_skillList.Count; s++){
            Skill skill = m_skillList[s];
            // already learned
            if(m_learnedSkillLV[skill.m_skillID] != 0 && m_learnedSkillLV[skill.m_skillID] >= skill.m_lv && !skill.m_linear){
                // Debug.Log("Skill" + skill.m_name + " is already learned.");
                continue;
            }
            // lv requirements
            if(skill.m_prerequisiteLV > level){
                // Debug.Log("Skill" + skill.m_name + " should appear in higher levels.");
                continue;
            }
            // skill requirements
            List<int> prerequisite = skill.m_prerequisiteSkill;
            bool satisfied = true;
            // Debug.Log("Start checking prerequisites of " + "Skill" + skill.m_name);
            foreach(int ps in prerequisite){
                if(m_learnedSkillLV[m_skillList[ps].m_skillID] < m_skillList[ps].m_lv){
                    // havent learned yet
                    satisfied = false;
                    break;
                }
            }
            if(satisfied){
                // Debug.Log("Add skill " + skill.m_name);
                m_generateList.Add(s);
            }
        }
        // foreach(int s in m_generateList){
        //     Debug.Log("Skill pool: " + m_skillList[s].m_name + " Lv. " + m_skillList[s].m_lv);
        // }
    }

    public void GenerateLoadSkills(int level){
        // Get three skills from the skill pool
        m_skillSelection.m_activated = true;
        UpdateGenerateList(level);
        for(int i = 0; i < 3; i++){
            int skillIndex = Random.Range(0, m_generateList.Count);
            Skill skill = m_skillList[m_generateList[skillIndex]];
            if(m_exclusiveList[m_generateList[skillIndex]]){
                i--;
                continue;
            }
            m_exclusiveList[m_generateList[skillIndex]] = true;
            foreach(int t in skill.m_exclusivePool){
                m_exclusiveList[t] = true;
            }
            string skillName = LocalizationManager.m_instance.GetLocalisedString(skill.m_name);
            string lv = "";
            if(skill.m_lv != 0){
                lv = LocalizationManager.m_instance.GetLocalisedString("LV") + " " + skill.m_lv;
            }
            if(m_learnedSkillLV[skill.m_skillID] != 0){
                lv = LocalizationManager.m_instance.GetLocalisedString("LV") + " " + m_learnedSkillLV[skill.m_skillID] + " + 1";    // upgrade
            }
            string skillEffect = LocalizationManager.m_instance.GetLocalisedString(skill.m_name+"Effect");
            string skillDescription = LocalizationManager.m_instance.GetLocalisedString(skill.m_name+"Description");
            m_skillSelection.SelectionItems(i, skill.m_sprite, skill.m_ID, skillName, lv, skillEffect, skillDescription);
        }
    }

    public void SkillUpdate(int id, bool load = false){
        // Called in SkillSelection.cs when the confirm button is clicked
        // id: considers skill level
        bool learned = false;
        Skill skill = m_skillList[id];
        switch(id){
            case 0:    // restore hp
            case 1:
                m_player.RegenHP(skill.m_keyValue);
                SoundManager.m_instance.PlayRegenHPSound();
                break;
            case 2:    // increase maximum HP
                m_player.m_maxHP += skill.m_keyValue;
                m_player.InitUI();
                learned = true;
                break;
            case 3:    // special tile
            case 4:
            case 5:
                m_player.LearnSpecial(skill.m_lv);
                learned = true;
                break;
            case 6:    // diagonally swap
            case 7:
            case 8:
                m_player.LearnDiagonal(skill.m_lv);
                learned = true;
                break;
            case 9:    // extra regen
                m_player.SetBonus(skill.m_keyValue);
                learned = true;
                break;
            case 10:    // gourd
            case 11:
            case 12:
                learned = true;
                break;
        }
        if(learned){
            SoundManager.m_instance.PlayLearnSkillSound();
            m_playerSkillSlots.FillSkillSlot(skill);
            if(skill.m_linear){
                if(m_learnedSkillLV[skill.m_skillID] == 0){
                    // havent learned
                    m_learnedIDQueue[m_queueIndex] = id;
                    m_queueIndex ++;
                }
                m_learnedSkillLV[skill.m_skillID] ++;
            }
            else{
                m_learnedSkillLV[skill.m_skillID] = m_skillList[id].m_lv;
                m_learnedIDQueue[m_queueIndex] = id;
                m_queueIndex ++;
            }

        }
        m_player.controllable = true;
        if(!load){
            m_controller.BoardExpand();
            GameManager.Instance.SaveData();
        }
    }

    public bool SkillSelectionActivation(){
        return m_skillSelection.m_activated;
    }

    public void CreatureSkillTrigger(Skill skill){
        int id = skill.m_skillID;
        switch(id){
            case 97:
                m_player.LearnStomp(skill.m_lv);
                break;
        }
    }

    public void CleanCreatureSkills(){
        m_player.LearnStomp(0);
    }

    public void SaveData(){
        for(int i = 0; i < m_skillList.Count; i++){
            // Debug.Log("the " + i + "th skill is " + m_learnedIDQueue[i]);
            PlayerPrefs.SetInt("LearnedSkillQueue"+i, m_learnedIDQueue[i]);
        }
        PlayerPrefs.SetInt("BlessingLevel", m_learnedSkillLV[2]);
        PlayerPrefs.SetInt("FurnaceLevel", m_learnedSkillLV[5]);
    }
}
