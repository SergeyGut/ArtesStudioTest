using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class ScoreUpdater : ITickable, IInitializable
{
    private float displayScore = 0;
    private float scoreSpeed;
    private int lastDisplayedScoreInt = -1;

    private TextMeshProUGUI scoreText;
    private Dictionary<string, GameObject> unityObjects;
    private IScoreService scoreService;
    private ISettings settings;

    public ScoreUpdater(
        Dictionary<string, GameObject> unityObjects,
        IScoreService scoreService,
        ISettings settings)
    {
        this.unityObjects = unityObjects;
        this.scoreService = scoreService;
        this.settings = settings;
    }
    
    public void Tick()
    {
        displayScore = Mathf.Lerp(displayScore, scoreService.Score, scoreSpeed * Time.deltaTime);
        
        int currentScoreInt = Mathf.RoundToInt(displayScore);
        if (currentScoreInt != lastDisplayedScoreInt)
        {
            scoreText.text = currentScoreInt.ToString();
            lastDisplayedScoreInt = currentScoreInt;
        }
    }

    public void Initialize()
    {
        scoreText = unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>();
        
        var score = scoreService.Score;
        scoreText.text = score.ToString();
        
        scoreSpeed = settings.ScoreSpeed;
        displayScore = score;
        lastDisplayedScoreInt = score;
    }
}
