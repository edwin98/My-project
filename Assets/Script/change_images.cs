using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class change_images : MonoBehaviour
{
    public GameObject[] OBJS;
    List<Sprite> ImageList = new List<Sprite>();
    //需要显示的图片的UI
    public Image Outside, Inside;
    //图片的切换时间
    private float changeImageTimer;

    //外层图片的透明状态
    private bool firstOpen = true;
    //单张图片需要停留的时间（淡出是按停留时间一半算）
    private float eachImageStayTime = 3f;

    void Start()
    {
        //一开始加载需要播的图片集合
        LoadImage();
    }

    /// <summary>
    /// 加载初始图片（图片的目录，想要播的图片都放此目录）
    /// </summary>
    private void LoadImage()
    {
        ImageList = Resources.LoadAll<Sprite>("Image/SlideShow").ToList();
        Outside.sprite = ImageList[0];
        Inside.sprite = ImageList[1];
    }

    void Update()
    {
        //开始轮播图片
        ChangeImage();
    }

    /// <summary>
    /// 图片切换逻辑实现（按照集合顺序轮播，需要考虑播到最后一张的情况）
    /// </summary>
    void ChangeImage()
    {
        changeImageTimer += Time.deltaTime;

        while (changeImageTimer >= eachImageStayTime)
        {
            changeImageTimer = 0f;

            if (firstOpen)
            {
                for (int i = 0; i < ImageList.Count; i++)
                {
                    string spriteName = ImageList[i].name;

                    if (Outside.sprite.name != spriteName) continue;

                    if (i < ImageList.Count - 1)
                    {
                        firstOpen = false;
                        Inside.sprite = ImageList[i + 1];

                        Outside.DOFade(0, eachImageStayTime * 0.5f).OnComplete(() =>
                        {
                            ChangeImage(Inside, Outside);
                        });
                    }
                    else
                    {
                        Inside.sprite = ImageList[0];
                        firstOpen = false;
                        Outside.DOFade(0, eachImageStayTime * 0.5f).OnComplete(() =>
                        {
                            ChangeImage(Inside, Outside);
                        });
                    }
                    break;
                }
            }
            else
            {
                for (int i = 0; i < ImageList.Count; i++)
                {
                    string spriteName = ImageList[i].name;

                    if (Inside.sprite.name != spriteName) continue;
                    if (i < ImageList.Count - 1)
                    {
                        firstOpen = true;
                        Outside.sprite = ImageList[i + 1];
                        Outside.DOFade(1, eachImageStayTime * 0.5f).OnComplete(() =>
                        {
                            ChangeImage(Outside, Inside);
                        });
                    }
                    else
                    {
                        firstOpen = true;
                        Outside.sprite = ImageList[0];

                        Outside.DOFade(1, eachImageStayTime * 0.5f).OnComplete(() =>
                        {
                            ChangeImage(Outside, Inside);
                        });
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 切换轮播图片（判断当前显示的图片，切换另一张图片的下一张显示的图片）
    /// </summary>
    /// <param name="show1"></param>
    /// <param name="show2"></param>
    void ChangeImage(Image show1, Image show2)
    {
        for (int j = 0; j < ImageList.Count; j++)
        {
            string spriteName = ImageList[j].name;
            if (show1.sprite.name != spriteName) continue;
            show2.sprite = j < ImageList.Count - 1 ? ImageList[j + 1] : ImageList[0];
        }
    }
}
