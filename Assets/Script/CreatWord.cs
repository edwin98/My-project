using NPOI.XWPF.UserModel;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CreatWord : MonoBehaviour
{
    /// <summary>
    /// 文件路径
    /// </summary>
    private string filePath;

    /// <summary>
    /// 文件名称
    /// </summary>
    private string fileName = "钢铁是怎样炼成的.docx";

    private string path;

    private void Start()
    {
        filePath = Application.dataPath + "/word";
        path = Path.Combine(filePath, fileName);
        if (!File.Exists(path))
        {
            FileStream stream = File.Create(path);
            stream.Close();
        }
    }


    /// <summary>
    /// 创建
    /// </summary>
    public void Create()
    {
        XWPFDocument doc = new XWPFDocument();

        XWPFParagraph paragraph = doc.CreateParagraph();

        paragraph.Alignment = ParagraphAlignment.CENTER;
        XWPFRun run = paragraph.CreateRun();
        run.FontSize = 26;
        run.SetColor("000000");
        run.FontFamily = "黑体";
        run.IsBold = true;
        run.SetText(context[0]);


        paragraph = doc.CreateParagraph();
        paragraph.Alignment = ParagraphAlignment.LEFT;
        run = paragraph.CreateRun();
        run.FontSize = 16;
        run.SetColor("000000");
        run.FontFamily = "仿宋";
        run.SetText(context[1]);
        Debug.Log(context);

        paragraph = doc.CreateParagraph();
        paragraph.Alignment = ParagraphAlignment.LEFT;
        run = paragraph.CreateRun();
        run.FontSize = 16;
        run.SetColor("000000");
        run.FontFamily = "宋体";
        run.IsBold = true;
        run.SetText(context[2]);


        paragraph = doc.CreateParagraph();
        paragraph.Alignment = ParagraphAlignment.LEFT;
        run = paragraph.CreateRun();
        run.FontSize = 16;
        run.SetColor("000000");
        run.FontFamily = "黑体";
        run.IsItalic = true;
        run.SetText(context[3]);

        try
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            doc.Write(fs);
            fs.Close();
            fs.Dispose();
            Process.Start("explorer.exe", filePath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(IOException))
            {
                Debug.Log("创建失败，同名文件被打开！");
            }
        }


    }


    private string[] context = new string[] {

    };
}
