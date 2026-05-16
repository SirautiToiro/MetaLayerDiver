using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NarrowDownCardsWindow : MonoBehaviour,IUIPage
{
    [SerializeField] private GameObject backPanel;

    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private List<Toggle> attributeToggles;
    [SerializeField] private List<Toggle> tierToggles;
    [SerializeField] private List<Toggle> costToggles;
    [SerializeField] private List<Toggle> targetToggles;
    [SerializeField] private List<Toggle> itemGetToggles;

    [SerializeField] private UIPageManager uiPageManager;

    public UIPageManager UIPageManager => throw new System.NotImplementedException();

    /// <summary>
    /// 絞り込みの結果表示する対象を列挙したクラス
    /// </summary>
    public class CardsNarrowDown
    {
        public List<AttributeDefine.Attribute> Attributes;
        public List<TierDefine.Tier> Tiers;
        public List<int> Costs;
        public List<TargetDefine.EffectTarget> EffectTargets;
        public List<ItemGetTypeDefine.ItemGetType> ItemGetTypes;
    }

    public void Init()
    {
        this.gameObject.SetActive(false);
        backPanel.SetActive(false);

        //トグルを全てOFF
        for (int i = 0; i < attributeToggles.Count; i++)
        {
            attributeToggles[i].isOn = false;
        }
        for (int i = 0; i < tierToggles.Count; i++)
        {
            tierToggles[i].isOn = false;
        }
        for (int i = 0; i < costToggles.Count; i++)
        {
            costToggles[i].isOn = false;
        }
        for (int i = 0; i < targetToggles.Count; i++)
        {
            targetToggles[i].isOn = false;
        }
        for (int i = 0; i < itemGetToggles.Count; i++)
        {
            itemGetToggles[i].isOn = false;
        }
    }

    private void Open()
    {
        this.gameObject.SetActive(true);
        backPanel.SetActive(true);
    }

    private void Close()
    {
        this.gameObject.SetActive(false);
        backPanel.SetActive(false);

        CardsNarrowDown result = new CardsNarrowDown();
        result.Attributes = new List<AttributeDefine.Attribute>();
        result.Tiers = new List<TierDefine.Tier>();
        result.Costs = new List<int>();
        result.EffectTargets = new List<TargetDefine.EffectTarget>();
        result.ItemGetTypes = new List<ItemGetTypeDefine.ItemGetType>();

        //絞り込み対象を整理
        for(int i=0;i<attributeToggles.Count;i++)
        {
            if (attributeToggles[i].isOn)
            {
                result.Attributes.Add((AttributeDefine.Attribute)i);
            }
        }

        for (int i = 0; i < tierToggles.Count; i++)
        {
            if (tierToggles[i].isOn)
            {
                result.Tiers.Add((TierDefine.Tier)i);
            }
        }

        for (int i = 0; i < costToggles.Count; i++)
        {
            if (costToggles[i].isOn)
            {
                result.Costs.Add(i);
            }
        }

        for (int i = 0; i < targetToggles.Count; i++)
        {
            if (targetToggles[i].isOn)
            {
                result.EffectTargets.Add((TargetDefine.EffectTarget)i);
            }
        }

        for (int i = 0; i < itemGetToggles.Count; i++)
        {
            if (itemGetToggles[i].isOn)
            {
                result.ItemGetTypes.Add((ItemGetTypeDefine.ItemGetType)i);
            }
        }

        stashPanel.CloseNarrowDownCardsWindow(result);
    }

    public void Reset()
    {
        foreach (var t in attributeToggles)
        {
            t.isOn = false;
        }
        foreach (var t in tierToggles)
        {
            t.isOn = false;
        }
        foreach (var t in costToggles)
        {
            t.isOn = false;
        }
        foreach (var t in targetToggles)
        {
            t.isOn = false;
        }
        foreach (var t in itemGetToggles)
        {
            t.isOn = false;
        }
    }

    public void OnPushed()
    {
        this.Open();
    }

    public void OnPopped()
    {
        this.Close();
    }

    public void OnCovered()
    {
        
    }

    public void OnBecomeTopPage()
    {
        
    }

    public void PushSelf()
    {
        uiPageManager.PushUIPage(this);
    }
}
