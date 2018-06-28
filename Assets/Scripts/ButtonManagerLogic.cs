using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NRand;

public class ButtonManagerLogic : MonoBehaviour
{
    [SerializeField] float m_buttonActionTime = 5;
    [SerializeField] float m_minDeltaActionTime = 5;
    [SerializeField] float m_maxDeltaActionTime = 20;
    [SerializeField] float m_multipleButtonChance = 0.1f;
    [SerializeField] float m_maxMultipleButtonChance = 0.6f;
    [SerializeField] float m_timeReductionDelay = 50;

    static List<ButtonInteractableLogic> m_buttons = new List<ButtonInteractableLogic>();

    float m_currentTime = 0;
    float m_delayToNext = 0;

    public static void addButton(ButtonInteractableLogic b)
    {
        m_buttons.Add(b);
    }

    public static void removeButton(ButtonInteractableLogic b)
    {
        m_buttons.Remove(b);
    }

    void Start()
    {
        selectNextDelay();
    }
    
    void Update()
    {
        m_currentTime += Time.deltaTime;
        m_delayToNext -= Time.deltaTime;

        if (m_delayToNext <= 0)
            startButtons();
    }

    void selectNextDelay()
    {
        float min = m_minDeltaActionTime / (1 + m_currentTime / m_timeReductionDelay);
        float max = m_maxDeltaActionTime / (1 + m_currentTime / m_timeReductionDelay);

        m_delayToNext = new UniformFloatDistribution(min, max).Next(new StaticRandomGenerator<DefaultRandomGenerator>());
    }

    void startButtons()
    {
        float multiplier = 1 + m_currentTime / m_timeReductionDelay;
        var gen = new StaticRandomGenerator<DefaultRandomGenerator>();

        float multiButtonProbability = Mathf.Min(m_maxMultipleButtonChance, m_multipleButtonChance * multiplier);
        var dMultibutton = new BernoulliDistribution(multiButtonProbability);
        var dButtons = new UniformIntDistribution(m_buttons.Count - 1);
        do
        {
            m_buttons[dButtons.Next(gen)].startButton(m_buttonActionTime);
        }
        while (dMultibutton.Next(gen)) ;

        selectNextDelay();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "delay " + m_delayToNext);
    }
}
