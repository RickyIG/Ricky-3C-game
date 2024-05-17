using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GiveValue : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    [SerializeField]
    private TextMeshProUGUI _highScoreText;
    
    // Start is called before the first frame update
    void Start()
    {
        string getScoreText = StaticData.scoreValueToKeep;
        string getHighScoreText = StaticData.highScoreValueToKeep;

        _scoreText.text = getScoreText;
        _highScoreText.text = getHighScoreText;
        
    }

    // Update is called once per frame
    // void Update()
    // {
        
    // }
}
