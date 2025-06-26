using System;
using System.ComponentModel;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class PatientProfile
{
    [SerializeField] private string id;
    [SerializeField] private int age;
    [SerializeField] private StudyCategory category;
    [SerializeField] private DateTime timeOfStroke;
    [SerializeField] private Side affectedSide;
    [SerializeField] private Side handedness;

    // Setters and Getters properties
    public string ID { get => id; set => id = value; }

    public int Age { get => age; set => age = value; }

    public DateTime TimeOfStroke { get => timeOfStroke; set => TimeOfStroke = value; }

    [JsonConverter(typeof(StringEnumConverter))]
    public StudyCategory Category { get => category; set => category = value; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Side AffectedSide { get => affectedSide; set => affectedSide = value; }

    [JsonConverter(typeof(StringEnumConverter))]
    public Side Handedness { get => handedness; set => handedness = value; }

    // Constructors
    public PatientProfile(string id, int age, StudyCategory category, DateTime timeOfStroke, Side affectedSide, Side handedness)
    {
        this.id = id;
        this.age = age;
        this.category = category;
        this.timeOfStroke = timeOfStroke;
        this.affectedSide = affectedSide;
        this.handedness = handedness;
    }

    public PatientProfile(PatientProfile another)
    {
        this.id = another.ID;
        this.age = another.Age;
        this.category = another.Category;
        this.timeOfStroke = another.TimeOfStroke;
        this.affectedSide = another.AffectedSide;
        this.handedness = another.Handedness;
    }

    public PatientProfile()
    {
        this.id = "id";
        this.age = 0;
        this.category = StudyCategory.Stroke;
        this.timeOfStroke = DateTime.MinValue;
        this.affectedSide = Side.None;
        this.handedness = Side.None;
    }


    //Copy Methods
    // CopyFrom
    public virtual void CopyFrom(PatientProfile another)
    {
        if (another == null)
            throw new ArgumentNullException(nameof(another));

        this.id = another.ID;
        this.age = another.Age;
        this.category = another.Category;
        this.timeOfStroke = another.TimeOfStroke;
        this.affectedSide = another.AffectedSide;
        this.handedness = another.Handedness;
    }

    // with constructor
    public virtual PatientProfile DeepCopy()
    {
        return new PatientProfile(this);
    }

    // with serialization
    /*public static T Clone<T>(T source)
    {
        var serialized = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(serialized);
    }*/

    // Custom Types
    public enum StudyCategory
    {
        Stroke,
        Control
    }

    public enum Side
    {
        None,
        Left,
        Right
    }


    public void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PrintInfo()
    {
        string message = "";
        message += $"ID : {this.ID.ToString()}\n";
        message += $"Age: {this.Age.ToString()}\n";
        message += $"Category : {this.Category.ToString()}\n";
        message += $"Handedness : {this.Handedness.ToString()}\n";
        message += $"Affected SIde : {this.AffectedSide.ToString()}\n"; 
        message += $"Date of Stroke : {this.timeOfStroke.ToString("dd MM, yyyy")}\n";
        Debug.Log(message);
    }
}
