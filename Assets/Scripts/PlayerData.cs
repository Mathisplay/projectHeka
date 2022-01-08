using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public GameObject text;

    private Text textVal;
    private int score;
    private int hp;
    void Start()
    {
        score = 0;
        hp = 3;
        textVal = text.GetComponent<Text>();
        UpdateText();
    }
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        UpdateText();
    }
    public void GetPoints(int pts)
    {
        score += pts;
        UpdateText();
    }
    private void UpdateText()
    {
        textVal.text = "HP: ";
        for (int i = 0; i < 3; i++)
        {
            if (i < hp)
            {
                textVal.text += "*";
            }
            else
            {
                textVal.text += " ";
            }
        }
        textVal.text += " PTS: " + score.ToString();
    }
}
