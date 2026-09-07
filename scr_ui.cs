using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;


public class scr_ui : MonoBehaviour
{

    [SerializeField] private scr_player player;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private List<Image> lives = new();

    [SerializeField] private TextMeshProUGUI middleText;

    int stage = 0;
    


    // Start is called before the first frame update
    void Awake()
    {
        //Get all our components


        if (player != null)
        {
            player.EnemyDestroyed += OnEnemyDestroy;
            player.PlayerDestroyed += OnPlayerDestroyed; 
            player.NewLevel += OnNewLevel;
        }

        OnNewLevel();
    }

    void Start()
    {
        UpdateScore();
        UpdateLives();
    }

    private void OnEnemyDestroy()
    {
        //Update score to match
        UpdateScore();
    }


    private void OnPlayerDestroyed()
    {
        //Remove a life
        if (player.lives <= -1)
        {
            middleText.text = "GAME OVER";
        }
        else
        {
            UpdateLives();
            SetMiddleText("Ready", 2);
        }
    }

    private void OnNewLevel()
    {
        stage++;
        SetMiddleText("Stage " + stage, 2);
    }

    private void UpdateScore()
    {
        score.text = "Score\n" + player.score;
    }

    private void UpdateLives()
    {
        for (int i = 0; i < lives.Count; i++)
        {
            if (i < player.lives)
            {
                lives[i].enabled = true;
            }
            else
            {
                lives[i].enabled = false;
            }
        }
    }

    private void SetMiddleText(string text, float time)
    {
        middleText.text = text;
        Invoke(nameof(ResetMiddleText), time);


    }

    private void ResetMiddleText()
    {
        middleText.text = "";
    }

}
