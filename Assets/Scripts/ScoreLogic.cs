using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ScoreLogic : MonoBehaviour
{
    [SerializeField] string m_mainMenuScene;
    [SerializeField] GameObject m_photoPrefab;
    [SerializeField] GameObject m_scorePrefab;
    [SerializeField] float m_photoHeight = 200;
    [SerializeField] float m_photoDelta = 500;
    [SerializeField] float m_baseScoreHeight = 100;
    [SerializeField] float m_scoreDelta = -30;
    [SerializeField] float m_totalScoreDelta = -50;
    [SerializeField] float m_maxScorePhoto = 1000;
    [SerializeField] float m_minScorePhoto = 100;
    [SerializeField] float m_scorePhotoNotTaken = 10;
    [SerializeField] float m_maxPhotoDistance = 5;
    [SerializeField] float m_baseTime = 60;
    [SerializeField] float m_baseTimeScore = 1000;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        calculateScores();
    }

    public void onContinuePress()
    {
        SceneManager.LoadScene(m_mainMenuScene);
    }

    class PhotoInfos
    {
        public int x;
        public int y;
        public string name;
        public float distance;
        public Texture2D pic;
        public float score;
    }

    void calculateScores()
    {
        var photos = getScorablePhotos();
        var photoNotTakenScore = photos.Count * m_scorePhotoNotTaken * (CameraLogic.maxPhotoCount - CameraLogic.photos.Count);
        var timeScore = m_baseTimeScore / (LevelMap.instance.time / m_baseTime);

        instanciatePhotos(photos);
        instanciateScores(photos, photoNotTakenScore, timeScore);
    }

    List<PhotoInfos> getScorablePhotos()
    {
        List<PhotoInfos> m_scores = new List<PhotoInfos>();

        foreach (var p in CameraLogic.photos)
        {
            int bestX = 0;
            int bestY = 0;
            float bestDistance = float.MaxValue;
            string bestName = "";

            foreach (var i in LevelMap.instance.importantPoints)
            {
                var d = new Vector2(p.pos.x - i.x, p.pos.y - i.y).magnitude;
                if (d < bestDistance)
                {
                    bestX = i.x;
                    bestY = i.y;
                    bestName = i.name;
                    bestDistance = d;
                }
            }

            if (bestDistance < m_maxPhotoDistance)
            {
                var s = new PhotoInfos();
                s.x = bestX;
                s.y = bestY;
                s.name = bestName;
                s.distance = bestDistance;
                s.pic = p.photo;
                m_scores.Add(s);
            }
        }

        for (int i = m_scores.Count - 1; i >= 0; i--)
        {
            bool isBest = true;
            for (int j = 0; j < m_scores.Count; j++)
            {
                if (i == j)
                    continue;
                var a = m_scores[i];
                var b = m_scores[j];
                if (a.x == b.x && a.y == b.y && a.distance > b.distance)
                {
                    isBest = false;
                    break;
                }
            }

            if (!isBest)
                m_scores.RemoveAt(i);
        }

        foreach(var s in m_scores)
            s.score = (m_maxScorePhoto - m_minScorePhoto) * (1 - s.distance / m_maxPhotoDistance) + m_minScorePhoto;

        return m_scores;
    }

    void instanciatePhotos(List<PhotoInfos> photos)
    {
        float leftPos = - (photos.Count - 1) * m_photoDelta / 2;

        for(int i = 0; i < photos.Count; i++)
        {
            var photo = photos[i];
            var p = Instantiate(m_photoPrefab, transform);
            p.transform.localPosition = new Vector3(leftPos + m_photoDelta * i, m_photoHeight, 0);
            var pObj = p.transform.Find("Photo").GetComponent<Image>();
            pObj.sprite = Sprite.Create(photo.pic, new Rect(0, 0, photo.pic.width, photo.pic.height), new Vector2(photo.pic.width / 2, photo.pic.height / 2));
        }
    }

    void instanciateScores(List<PhotoInfos> photos, float photoNotTakenScore, float timeScore)
    {
        var totalScore = Mathf.FloorToInt(timeScore) + Mathf.FloorToInt(photoNotTakenScore);
        foreach (var p in photos)
            totalScore += Mathf.FloorToInt(p.score);

        float height = m_baseScoreHeight;

        Action<string, float> lambda = new Action<string, float>((string name, float score) =>
        {
            var obj = Instantiate(m_scorePrefab, transform);
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, height, obj.transform.localPosition.z);
            var label = obj.transform.Find("Label").GetComponent<Text>();
            var element = obj.transform.Find("Element").GetComponent<Text>();

            label.text = name;
            element.text = Mathf.FloorToInt(score) + " points";

            height += m_scoreDelta;
        });

        foreach(var p in photos)
            lambda(p.name + " :", p.score);

        lambda("Photos restantes (" + (CameraLogic.maxPhotoCount - CameraLogic.photos.Count) + ") :", photoNotTakenScore);

        int min = Mathf.FloorToInt(LevelMap.instance.time / 60);
        int s = Mathf.FloorToInt(LevelMap.instance.time - min * 60);
        lambda("Temps (" + min + "m " + s + "s) :", timeScore);

        height += m_totalScoreDelta - m_scoreDelta;
        lambda("Total :", totalScore);
    }
}
