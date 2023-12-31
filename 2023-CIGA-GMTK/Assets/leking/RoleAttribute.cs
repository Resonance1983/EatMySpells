using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHp
{
    //获取Hp
    public float GetHp();
}

public interface IHealth:IHp
{
    //被杀死时调用
    public void Kill();
    //被攻击时调用
    public float Attack(float damage);
    public float Attack(float damage,ElementTypes type);
}
public interface IBuff
{
    public List<Buff> GetBuffs();
    //添加Buff
    public void AddBuff(Buff buff);
    //执行Buff
    public void ExecuteBuffs();
    //移除Buff
    public void RemoveBuffs(BuffType type);
    //进入下一回合
    public void BuffNext();
    public bool CheckBuff(BuffType type);

    public void CleanTempBuff();
}
