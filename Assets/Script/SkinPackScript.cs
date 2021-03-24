using CommonClass;
using System;
using System.Data;
using System.IO;
using UnityEngine;

public class SkinPackScript : MonoBehaviour
{
    public GameObject[] Heads;
    public GameObject[] Bodies;
    public GameObject[] Emotions;
    public GameObject[] Subs;
    public GameObject[] Coses;
    public Material[] FaceColors;
    public Material[] BodyColors;
    public Material[] SkinColors;
    public Material[] Tatoos;

    public TextAsset Data;

    void Start()
    {
        DataSet SkinTranslation = new DataSet();
        DataTable Head1 = new DataTable("Head");
        Head1.Columns.Add("Name", typeof(string));
        Head1.Columns.Add("Translation", typeof(string));
        foreach (GameObject game in Heads)
        {
            DataRow row = Head1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Head1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Head1);

        DataTable Body1 = new DataTable("Body");
        Body1.Columns.Add("Name", typeof(string));
        Body1.Columns.Add("Translation", typeof(string));
        foreach (GameObject game in Bodies)
        {
            DataRow row = Body1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Body1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Body1);

        DataTable Emotion1 = new DataTable("Emotion");
        Emotion1.Columns.Add("Name", typeof(string));
        Emotion1.Columns.Add("Translation", typeof(string));
        foreach (GameObject game in Emotions)
        {
            DataRow row = Emotion1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Emotion1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Emotion1);

        DataTable Sub1 = new DataTable("Sub");
        Sub1.Columns.Add("Name", typeof(string));
        Sub1.Columns.Add("Translation", typeof(string));
        foreach (GameObject game in Subs)
        {
            DataRow row = Sub1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Sub1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Sub1);

        DataTable Cos1 = new DataTable("Cos");
        Cos1.Columns.Add("Name", typeof(string));
        Cos1.Columns.Add("Translation", typeof(string));
        foreach (GameObject game in Coses)
        {
            DataRow row = Cos1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Cos1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Cos1);

        DataTable Face1 = new DataTable("Face");
        Face1.Columns.Add("Name", typeof(string));                    //皮肤名
        Face1.Columns.Add("Translation", typeof(string));
        foreach (Material game in FaceColors)
        {
            DataRow row = Face1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Face1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Face1);

        DataTable BodyColor1 = new DataTable("BodyColor");
        BodyColor1.Columns.Add("Name", typeof(string));                    //皮肤名
        BodyColor1.Columns.Add("Translation", typeof(string));
        foreach (Material game in BodyColors)
        {
            DataRow row = BodyColor1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            BodyColor1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(BodyColor1);

        DataTable SkinColor1 = new DataTable("SkinColor");
        SkinColor1.Columns.Add("Name", typeof(string));                    //皮肤名
        SkinColor1.Columns.Add("Translation", typeof(string));
        foreach (Material game in SkinColors)
        {
            DataRow row = SkinColor1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            SkinColor1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(SkinColor1);

        DataTable Tatoo1 = new DataTable("Tatoo");
        Tatoo1.Columns.Add("Name", typeof(string));                    //皮肤名
        Tatoo1.Columns.Add("Translation", typeof(string));
        foreach (Material game in Tatoos)
        {
            DataRow row = Tatoo1.NewRow();
            row["Name"] = game.name;
            row["Translation"] = game.name;
            Tatoo1.Rows.Add(row);
        }
        SkinTranslation.Tables.Add(Tatoo1);

        string path = string.Format("{0}/{1}.json", Environment.CurrentDirectory, name);
        File.Delete(path);
        File.AppendAllText(path, JsonUntity.Object2Json(SkinTranslation));
    }
}
