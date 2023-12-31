using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellsManager : MonoBehaviour
{
    private static SpellsManager _instance;

    public static SpellsManager GetInstance()
    {
        return _instance;
    }

    public int magicCostLimit = 5; //Limit of Magic Cost
    public int magicCost = 0;

    //Current Memory that can be used to learn
    public int currentMemory = 20;
    public int memoryLimit = 20;

    public int fixedSpellNum = 5;

    public int initMagicAmount = 80;
    private int _magicAmount = 80;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }
    
    //魔法量相关
    public static int GetMagicAmount()
    {
        return _instance._magicAmount;
    }

    public static void addMagicAmount(int addMagicAmount)
    {
        _instance._magicAmount += addMagicAmount;
    }

    public static bool TryUseMagicAmount(int value)
    {
        if (_instance._magicAmount - value <= 0)
        {
            return false;
        }
        _instance._magicAmount -= value;
        return true;
    }

    public static void AddMagicAmount(int value)
    {
        _instance._magicAmount += value;
    }
    public static void SetMagicAmount(int value)
    {
        _instance._magicAmount = value;
    }

    //学习技能List
    public List<Spells> learnedSpells = new List<Spells>();

    //All Spells
    public List<Spells> allSpells = new List<Spells>();

    public List<Spells> fixedSpells = new List<Spells>();

    public static Spells GetSpell(string name)
    {
        var spell = Resources.Load<Spells>($"SkillData/{name}");
        return spell;
    }

    public void AddFixSpell(Spells spell)
    {
        spell.isFixed = true;
        if (!fixedSpells.Contains(spell))
            fixedSpells.Add(spell);
    }

    public void FixedSpellReturnToHand()
    {
        foreach (Spells spell in fixedSpells)
        {
            if (!learnedSpells.Contains(spell))
            {
                learnedSpells.Add(spell);
            }
        }
    }

    //TODO:添加学习法术接口

    public bool LearnSpell(string name)
    {
        Spells targetSpell;
        foreach (Spells spell in allSpells)
        {
            if (spell.name == name)
            {
                targetSpell = spell;

               
                if (!learnedSpells.Contains(spell))
                {
                    spell.OnAdding();
                    //Check if magic cost is sufficient
                    if (magicCost + spell.magicCost > magicCostLimit)
                        return false;

                    //Check if memory coast is sufficient
                    if (currentMemory < spell.finalMemoryCost)
                        return false;

                    //Successfully learn spell
                    currentMemory -= spell.finalMemoryCost;
                    magicCost += spell.magicCost;
                    spell.learnNum++;
                    if (spell.learnNum >= fixedSpellNum)
                        AddFixSpell(spell);
                    learnedSpells.Add(spell);
                    return true;
                }
            }
        }
        return false;
    }

    public bool SpellLearned(string name)
    {
        foreach (Spells spell in learnedSpells)
        {
            if (spell.name == name)
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveSpell(string name)
    {
        foreach (Spells spell in learnedSpells)
        {
            if (spell.name == name)
            {
                magicCost -= spell.magicCost;
                if (magicCost < 0)
                    magicCost = 0;
                learnedSpells.Remove(spell);
                return;
            }
        }
    }

    public void GainMemory(int memoryGained)
    {
        currentMemory += memoryGained;
    }

    public void GainMagicCostLimit(int magicCostLimitGained)
    {
        magicCostLimit += magicCostLimitGained;
    }

    public void GainMagicCost(int magicCostGained)
    {
        magicCost += magicCostGained;
    }

    public void GainMagicCost(Spells spell)
    {
        magicCost += spell.magicCost;
    }

    public void OnPlayerRoundStart()
    {
        //When Round Starts 
    }

    public void OnPlayerRoundEnd()
    {
        //When Round Ends
    }

    public bool UseSpell(Monster monster, Spells spell, bool usedByMonster = false)
    {
        if (magicCost - spell.magicCost < 0)
            return false;

        magicCost -= spell.magicCost;
        spell.OnCastByMonster(monster, usedByMonster);
        RemoveSpell(spell.name);
        return true;
    }

    public  void Initialize()
    {
        //Init
        learnedSpells.Clear();
        magicCost = 0;
        currentMemory = memoryLimit;
    }
}
